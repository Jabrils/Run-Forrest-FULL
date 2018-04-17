using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeuralNet;

public class ctrl : MonoBehaviour {
    public static string lvlsPath = (Application.isMobilePlatform) ? Path.Combine(Application.persistentDataPath, "Courses") : Path.Combine(Application.dataPath, "Courses");
    public static string forrestPath = (Application.isMobilePlatform) ? Path.Combine(Application.persistentDataPath, "Forrests") : Path.Combine(Application.dataPath, "Forrests");
    public static string dataPath = (Application.isMobilePlatform) ? Path.Combine(Application.persistentDataPath, "Fitness Data"): Path.Combine(Application.dataPath, "Fitness Data");
    public static string menuPath = (Application.isMobilePlatform) ? Path.Combine(Application.persistentDataPath, "Menu") : Path.Combine(Application.dataPath, "Menu");

    public enum GameMode { Train, Test, Campaign };
    public GameMode mode;
    public enum IntelMode { AI, Human, None };
    public IntelMode intelli;
    public enum CamMode { Free, Track, POV, Follow, Top, Lock };
    public CamMode Camy;
    public enum COMode { Random, Slice};
    public COMode CrossOver;
    public enum SelMode { Percent, Top2};
    public SelMode selMode;

    public bool randomizeIniWeights, saved, saveTheData, campAdd, slowMode;
    public int day, mLaps = 2, camp, mDays;
    public Vector2 attempt;
    public GameObject forrest, startSp, t, aR, cFailed, cSucc, Ff;
    public Text vT, fT, camButt, dT, lpT, bfT, wT;
    public Slider oS;
    public Button SB, snEB;
    public InputField nfF;
    public Image sI, pI, cI;
    public ForrestCTRL F;

    public RectTransform[] rT;
    public Text[] inp;
    public float[][] attIniData;
    public string[] attIniString;
    float mutationRate = 1f;
    float mutationProb = .05f;
    public NN highestFit = new NN();
    public float highestAvg;
    public string highestFitBrain;
    public List<string> loadedBrains;
    public List<ForrestCTRL> startFatt = new List<ForrestCTRL>();
    public List<ForrestCTRL> allFatt = new List<ForrestCTRL>();
    Image[] aRs;
    menuParams mP;

    // 1, 2, 3, 4, 5
    GameObject[] graphs = new GameObject[2];
    int graphWidth = 860;
    float graphBarWidth;
    List<float> fits = new List<float>();
    List<float> avgs = new List<float>();

    Vector3[] camStarts = new Vector3[2];


    // Use this for initialization
    void Start()
    {
        menuParams.Check4MenuParam();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        campAdd = true;

        // 
        if (GameObject.Find("menuParams"))
        {
            mP = GameObject.Find("menuParams").GetComponent<menuParams>();

            if (mP.sesh)
            {
                    StreamReader sr = new StreamReader(Path.Combine(Application.dataPath, "sesh.txt"));
                string[] load = sr.ReadToEnd().Split('\n');
                    sr.Close();

                mP.seshMark.y = load.Length;

                if(mP.seshMark.x>mP.seshMark.y)
                {
                    Application.Quit();
                    if (Application.isEditor)
                    {
                        GoToMenu();
                    }
                }

                string[] load2 = load[(int)mP.seshMark.x-1].Split(',');

                    // laps, days, course, CO, Mode, attempts, ini
                    mP.laps = int.Parse(load2[0]);
                    mP.maxDays = int.Parse(load2[1]);
                mP.lvlName = load2[2];
                mP.lvlData = menu.ReturnLvlData(load2[2]);
                mP.CO = (COMode)int.Parse(load2[3]);
                mP.mode = (GameMode)int.Parse(load2[4]);
                mP.attempts = int.Parse(load2[5]);
                mP.randomizeWeights = bool.Parse(load2[6]);
                mP.storedName = load2[7];
            }

                mode = mP.mode;
                CrossOver = mP.CO;
                selMode = mP.SEL;
                randomizeIniWeights = mP.randomizeWeights;
                attempt.y = mP.attempts;
                mLaps = mP.laps;
                slowMode = mP.slowMode;

            // 
            if (mode == GameMode.Test)
            {
                attempt.y = mP.brains.Count;
            }
            else if(mode == GameMode.Campaign)
            {
                attempt.y = 1;
            }
            // 
            if (mP.brains.Count > 0)
            {
                // 
                for (int i = 0; i < mP.brains.Count; i++)
                {
                    loadedBrains.Add(mP.brains[i].Split('\n')[1]);
                }
            }


            // 
            lvlBuilder lb = gameObject.AddComponent<lvlBuilder>();

            // 
            if (mode == GameMode.Test || mode == GameMode.Train)
            {
                lb.saveFile = mP.lvlData;
                lb.reverse = mP.reverse;
            }
            else
            {
                mP.lvlName = "Campaign Course #" + (mP.campaignProgress + 1);
                lb.saveFile = (mP.campaignProgress < 4) ? mP.campaign[(int)Mathf.Floor(mP.campaignProgress)] : "FINAL";
                lb.reverse = (mP.campaignProgress % 1) != 0;
            }

        }

        // 
        graphs[0] = GameObject.Find("FitnessGraph");
        graphs[1] = GameObject.Find("AverageGraph");

        // This will disable the UI if in race mode
            rT[0].gameObject.SetActive(mode == GameMode.Test || mode == GameMode.Campaign);
            rT[1].gameObject.SetActive(mode == GameMode.Train);

        // Set the appropriate version number on the top left of screen
        vT.text = "v" + Application.version;

        // Set all of the input texts to "?" instead of the default "New Text" :)
        for (int i = 0; i < inp.Length; i++)
        {
            inp[i].text = "?";
        }

        // This will hide the starting marker
        startSp.GetComponent<Renderer>().enabled = false;

        // This grabs a ref to the camera's starting position & rotation
        camStarts[0] = Camera.main.transform.position;
        camStarts[1] = Camera.main.transform.eulerAngles;

        // This sets the text for the control & camera mode buttons
        //ctButt.text = intelli.ToString();
        camButt.text = Camy.ToString();

        // attIniData is Attempt Initializer, we first need to define how many brains this ini will hold, it will hold our max attempt count 
        attIniData = new float[(int)attempt.y][];

        // 
        attIniString = new string[(int)attempt.y];

        // then every brian will hold 29 values, in our case floats, so we'll loop through every brain & set their value size to hold 29 floats, which will be a fully randomized brain
        for (int i = 0; i < attIniData.Length; i++)
        {
            attIniData[i] = new float[29];
        }

        // 
        if (loadedBrains == null || loadedBrains.Count < 1)
        {
            // 
            if (randomizeIniWeights)
            {
                RandomizeWholeDay();
            }
        }
        else
        {
            SetWholeDay();
        }

    }

    void GenGraph(GameObject who, string high, List<float> eval, float max)
    {
        Color32[] gCol = new Color32[]
        {
            new Color32(161,255,171,255),
            new Color32(255,161,161,255),
            new Color32(255,252,161,255),
        };

        for (int i = 0; i < Mathf.Round(graphWidth / graphBarWidth); i++)
        {
            GameObject t = new GameObject();
            t.transform.SetParent(who.transform);
            t.AddComponent<Image>();
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.transform.localPosition = Vector3.zero + (Vector3.right * (i * graphBarWidth));
            rt.transform.localScale = Vector3.one;
            rt.pivot = Vector2.zero;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.localEulerAngles = Vector3.zero;
            rt.sizeDelta = new Vector2(graphBarWidth, (eval[i]/max)*130);

            Image im = rt.GetComponent<Image>();

                if (eval[i] == max)
                {
                    im.color = gCol[2];
                }
                else if(i==0 || eval[i] > eval[i-1])
                {
                    im.color = gCol[0];
                }
                else
                {
                    im.color = gCol[1];
                }

        }

        Text disp = who.GetComponentInChildren<Text>();
        disp.transform.SetAsLastSibling();
        disp.text = high+ ": " + Mathf.Round(max*1000)/1000;

    }

    // 
    void Update()
    {

        // Will load the menu if power is pressed
        if (Input.GetAxis("Power") > 0)
        {
            GoToMenu();
        }

        // Restart the scene when we press restart
        if (Input.GetAxis("Restart") > 0)
        {
            RestartRoom();
        }

        // Check to see if there are any active Forrests
        bool activeFs = allFatt.Count > 0;

        // If so then perform this 
        if (activeFs)
        {
            F = allFatt[ReturnHighest(allFatt)];
            t.transform.position = new Vector3(F.transform.position.x, 3.5f, F.transform.position.z);
            oS.value = F.movement;
            fT.text = mP.lvlName + (mP.reverse ? "(R)" : string.Empty) + "\n" + mP.CO + "-" + mP.SEL + "\n" + Mathf.Round(F.fitness * 1000) / 1000;
            lpT.text = ((slowMode) ? attempt.x+1 + "/" + attempt.y + "\n" : "") + F.lap.x + "/" + F.lap.y;
            wT.text = "" + F.myName + "\n" + mP.lvlName + (mP.reverse ? "(R)" : string.Empty) + "\n" + Mathf.Round(F.fitness * 1000) / 1000;

            // 
            for (int i = 0; i < inp.Length; i++)
            {
                try
                {
                    inp[i].text = "" + Mathf.Round(F.inp[i] * 100) / 100;
                }
                catch
                {
                    inp[i].text = "?";
                }
            }

            // 
            if (Camy == CamMode.Lock)
            {
                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                Camera.main.transform.position = new Vector3(F.transform.position.x, 50, F.transform.position.z);
            }
            else if (Camy == CamMode.Top)
            {
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation,Quaternion.Euler(new Vector3(90, F.transform.eulerAngles.y, 0)), Time.deltaTime * 10);
                Camera.main.transform.position = new Vector3(F.transform.position.x, 25, F.transform.position.z);
            }
            else if (Camy == CamMode.Free)
            {
                Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x + Input.GetAxis("Horizontal"), -100, 100), Mathf.Clamp(Camera.main.transform.position.y + (Input.GetAxis("Mouse ScrollWheel") * -10), 5, 100), Mathf.Clamp(Camera.main.transform.position.z + Input.GetAxis("Vertical"), -100, 100));
            }
            else if (Camy == CamMode.Track)
            {
                Camera.main.transform.Translate(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Mouse ScrollWheel") * 10, Input.GetAxis("Vertical")));
                Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x, -100, 100), Mathf.Clamp(Camera.main.transform.position.y, 2, 100), Mathf.Clamp(Camera.main.transform.position.z, -100, 100));
                Camera.main.transform.LookAt(F.transform);
            }
            else if (Camy == CamMode.POV)
            {
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, F.transform.localRotation, Time.deltaTime*10);
                Camera.main.transform.position = F.transform.position + (Vector3.up / 2);
            }
            else if (Camy == CamMode.Follow)
            {
                if (Vector3.Distance(Camera.main.transform.position, F.transform.position) > 3)
                {
                    Camera.main.transform.Translate(Vector3.forward * .15f);
                }

                Camera.main.transform.LookAt(F.transform.position+(Vector3.up*.5f));
            }
        }
    }

    public void ImStuck()
    {
        F.fitness = 0;
        F.Freeze();
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("menu");
    }

    /// <summary>
    /// restarts the room of course
    /// </summary>
    public void RestartRoom()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// This will update the Active Run Badges to reflect the accurate number of attempts left
    /// </summary>
    public void UpdateActiveRunBadges()
    {
        if (!slowMode)
        {
            // 
            if (allFatt.Count < aRs.Length)
            {
                SetActiveRunBadges(Color.black);

                // 
                for (int i = 0; i < allFatt.Count; i++)
                {
                    aRs[i].color = Color.white;
                }
            }
        }
    }

    /// <summary>
    /// This will turn all active run badges back to the inputted color
    /// </summary>
    void SetActiveRunBadges(Color col)
    {
        // 
        aRs = aR.GetComponentsInChildren<Image>();

        // 
        foreach (Image im in aRs)
        {
            im.color = col;
        }

    }

    /// <summary>
    /// This will set each brain configuration for the whole day randomly
    /// </summary>
    void SetWholeDay()
    {
        if (mode == GameMode.Train)
        {
           attIniData = ParseBrain2(attIniData,true);
        }
        else
        {
            attIniData = ParseBrain2(attIniData, false);
        }
    }

    /// <summary>
    /// This will parse the brain from a string into a float[]
    /// </summary>
    /// <param name="size"></param>
    /// <param name="randomize"></param>
    /// <returns></returns>
    float[] ParseBrain1(string data, int size)
    {
        float[] ret = new float[size];

        // set each brain 
            for (int j = 0; j < ret.Length; j++)
            {
                ret[j] = float.Parse(data.Split(',')[j]);
            }

        return ret;
    }

    /// <summary>
    /// This will parse the brain from a string into a float[][]
    /// </summary>
    /// <param name="size"></param>
    /// <param name="randomize"></param>
    /// <returns></returns>
    float[][] ParseBrain2(float[][] size, bool randomize)
    {
        float[][] data = size;

        // set each brain 
        for (int i = 0; i < data.Length; i++)
        {
            int p = (randomize) ? Random.Range(0, loadedBrains.Count) : i;
            for (int j = 0; j < data[i].Length; j++)
            {
                data[i][j] = float.Parse(loadedBrains[p].Split(',')[j]);
            }
        }

        return data;
    }

    /// <summary>
    /// This will set each brain configuration for the whole day randomly
    /// </summary>
    void RandomizeWholeDay()
    {
        // set each brain for Data
        for (int i = 0; i < attIniData.Length; i++)
        {
            for (int j = 0; j < attIniData[i].Length; j++)
            {
                attIniData[i][j] = RandomWeight();
            }
        }
    }

    /// <summary>
    /// Use this function to get a random weight value
    /// </summary>
    /// <returns></returns>
    float RandomWeight()
    {
        return Random.Range(-4f, 4f);
    }

    /// <summary>
    /// This will spawn Forrest at the start marker
    /// </summary>
    public void SpawnForrest(int num)
    {
        if (!slowMode)
        {
            // loop through the num count & spawn a every Forrest attempt for that day
            for (int i = 0; i < num; i++)
            {
                ForrestCTRL f = Instantiate(forrest).GetComponent<ForrestCTRL>();

                // Set the position to be that of the starting sphere
                f.transform.position = new Vector3(startSp.transform.position.x, f.transform.position.y, startSp.transform.position.z);

                // Set the angle to be that of the starting sphere
                f.transform.eulerAngles = new Vector3(f.transform.eulerAngles.x, startSp.transform.eulerAngles.y, f.transform.eulerAngles.z);

                // Set the brain to the corresponding attention ini data
                if (mode == GameMode.Train)
                {
                    f.SetBrain(attIniData[i]);
                }
                else if (mode == GameMode.Test)
                {
                    f.SetBrain(attIniData[i], mP.brains[i].Split('\n')[0]);
                }
                else if (mode == GameMode.Campaign)
                {
                    string name = mP.campNN.Split('\n')[0];
                    string pass = mP.campNN.Split('\n')[1];

                    f.SetBrain(ParseBrain1(pass, 29), name);
                }

                // Add the Forrest attemp to the list of All Forrest Attempts
                allFatt.Add(f);
            }

            // Need to clear & make way for a new day
            startFatt.Clear();

            // This will copy that list for later use
            for (int i = 0; i < allFatt.Count; i++)
            {
                startFatt.Add(allFatt[i]);
            }
        }
        else
        {
            if (day ==0)
            {
                day++;
            }

            // 
            if ((int)attempt.x == (int)attempt.y-1)
            {
                LearnFromAttempts();
                SetHighest();
                SetGraphs();
                attempt.x = -1;
            startFatt.Clear();
                day++;
            }
            attempt.x++;

            ForrestCTRL f = Instantiate(forrest).GetComponent<ForrestCTRL>();

            // Set the position to be that of the starting sphere
            f.transform.position = new Vector3(startSp.transform.position.x, f.transform.position.y, startSp.transform.position.z);

            // Set the angle to be that of the starting sphere
            f.transform.eulerAngles = new Vector3(f.transform.eulerAngles.x, startSp.transform.eulerAngles.y, f.transform.eulerAngles.z);

            // Set the brain to the corresponding attention ini data
            if (mode == GameMode.Train)
            {
                f.SetBrain(attIniData[(int)attempt.x]);
            }
            else if (mode == GameMode.Test)
            {
                f.SetBrain(attIniData[(int)attempt.x], mP.brains[(int)attempt.x].Split('\n')[0]);
            }

            allFatt.Add(f);
            startFatt.Add(f);

            // 
            f.Reset();

        }
    }

    /// <summary>
    /// This will spawn Forrest at the start marker
    /// </summary>
    public void RespawnForrest(int num)
    {
        GameObject[] grabPast = GameObject.FindGameObjectsWithTag("Passive");
        if (!slowMode)
        {
            // loop through the num count & spawn a every Forrest attempt for that day
            for (int i = 0; i < num; i++)
            {
                ForrestCTRL f = grabPast[i].GetComponent<ForrestCTRL>();

                // Set the position to be that of the starting sphere
                f.transform.position = new Vector3(startSp.transform.position.x, f.transform.position.y, startSp.transform.position.z);

                // Set the angle to be that of the starting sphere
                f.transform.eulerAngles = new Vector3(f.transform.eulerAngles.x, startSp.transform.eulerAngles.y, f.transform.eulerAngles.z);

                // Set the brain to the corresponding attention ini data
                f.SetBrain(attIniData[i]);

                // 
                f.Reset();

                // Add the Forrest attemp to the list of All Forrest Attempts
                allFatt.Add(f);
            }

            // Need to clear & make way for a new day
            startFatt.Clear();

            // This will copy that list for later use
            for (int i = 0; i < allFatt.Count; i++)
            {
                startFatt.Add(allFatt[i]);
            }
        }
        else
        {
            // 
            if ((int)attempt.x == (int)attempt.y-1)
            {
                LearnFromAttempts();
                SetHighest();
                SetGraphs();
                attempt.x = -1;
            startFatt.Clear();
                day++;
            }
            attempt.x++;

            ForrestCTRL f = grabPast[(int)attempt.x].GetComponent<ForrestCTRL>();

            // Set the position to be that of the starting sphere
            f.transform.position = new Vector3(startSp.transform.position.x, f.transform.position.y, startSp.transform.position.z);

            // Set the angle to be that of the starting sphere
            f.transform.eulerAngles = new Vector3(f.transform.eulerAngles.x, startSp.transform.eulerAngles.y, f.transform.eulerAngles.z);

            // Set the brain to the corresponding attention ini data
            if (mode == GameMode.Train)
            {
                f.SetBrain(attIniData[(int)attempt.x]);
            }
            else if (mode == GameMode.Test)
            {
                f.SetBrain(attIniData[(int)attempt.x], mP.brains[(int)attempt.x].Split('\n')[0]);
            }

            allFatt.Add(f);
            startFatt.Add(f);

            // 
            f.Reset();
        }
    }

    /// <summary>
    /// This starts a new day
    /// </summary>
    public void NewDay()
    {
        if (!slowMode)
        {
            // If not day 0
            if (day > 0)
            {
                SetHighest();

                // 
                if (mode == GameMode.Train)
                {
                    LearnFromAttempts();
                }
            }

            // 
            if (day > 0)
            {
                if (!saved && mode != GameMode.Test && mode != GameMode.Campaign)
                {
                    // Spawn all new day
                    RespawnForrest((int)attempt.y);
                }
                else
                {
                    Check4UI();
                }
            }
            else
            {
                SpawnForrest((int)attempt.y);
            }

            // 
            SetActiveRunBadges(Color.white);

            SetGraphs();

            // 
            if (mP.sesh && day == mP.maxDays)
            {
                if (mP.seshMark.x <= mP.seshMark.y)
                {
                    SaveForrest(AddTimeFormated("forData(" + mP.seshMark.x + ")"), highestFitBrain);
                    SaveData("forData(" + mP.seshMark.x + ")");
                    mP.seshMark.x++;
                    RestartRoom();
                }
            }

            // 
            // Increase the day by 1
            day++;

            UpdateActiveRunBadges();
        }
        else
        {
            // 
            if (day <= 1)
            {
                SpawnForrest((int)attempt.y);
            }
            else
            {
                if (!saved && mode != GameMode.Test && mode != GameMode.Campaign)
                {
                    // Spawn all new day
                    RespawnForrest((int)attempt.y);
                }
                else
                {
                    Check4UI();
                }
            }


        }

        // Update the text that displays the day
        dT.text = "Day\n" + day;
    }

    private void SetHighest()
    {
        // We'll need a reference to the highest Forrest in our finished Forrest list
        ForrestCTRL tF = startFatt[ReturnHighest(startFatt)];
        float avgFitTemp = GetAverageFitness(startFatt);

        // This saves a record of the fitness performances
        fits.Add(tF.fitness);

        // This saves a record of the fitness average performances
        avgs.Add(avgFitTemp);


        // If this new Forrest beat the Highest Forrest make him the new highest
        if (tF.fitness > highestFit.fitness)
        {
            // Create a new NN to store the highest brain from the training session
            highestFit = new NN(tF.nn.inputs, tF.nn.hL);

            // Set that fitness to the new record Forrest which is tF
            highestFit.SetFitness(tF.fitness);

            // Then set the weights to the new record Forrest tF
            highestFit.IniWeights(tF.nn.GetBrain());

            // Finally change the highest fit brain string to our new record weights
            highestFitBrain = highestFit.ReadBrain();
        }

        // If this new Forrest beat the Highest Forrest make him the new highest
        if (avgFitTemp > highestAvg)
        {
            highestAvg = avgFitTemp;
        }
    }

    void SetGraphs()
    {
        //
        graphBarWidth = (float)graphWidth / (float)day;

        // 
        ResetGraph(graphs[0]);
        ResetGraph(graphs[1]);

        GenGraph(graphs[0], "Top Fitness", fits, (float)highestFit.fitness);
        GenGraph(graphs[1], "Average Fitness", avgs, highestAvg);
    }

    void ResetGraph(GameObject who)
    {
        RectTransform[] rr = who.GetComponentsInChildren<RectTransform>();

        for (int i = 1; i < rr.Length-1; i++)
        {
            Destroy(rr[i].gameObject);
        }
    }

    /// <summary>
    /// This checks to see which UI we should enable if any
    /// </summary>
    void Check4UI()
    {
        if (mode == GameMode.Train)
        {
            // Enable the saving UI
            sI.gameObject.SetActive(saved);

            // 
            if (saved)
            {
                bfT.text = "Best Fitness: " + Mathf.Round((float)highestFit.fitness * 1000) / 1000;
            }
        }
        else if (mode == GameMode.Test)
        {
            // Enable the saving UI
            pI.gameObject.SetActive(mode == GameMode.Test);

            Text[] t = pI.GetComponentsInChildren<Text>();

            List<NN> ns = new List<NN>();

            // 
            foreach (ForrestCTRL fC in startFatt)
            {
                ns.Add(fC.nn);
            }

            int sN = (ns.Count < 3 ? ns.Count : 3);

            // 
            for (int i = 0; i < sN; i++)
            {
                NN win = ns[ReturnHighest(ns)];
                t[i].text = (i + 1) + placeSuff(i) + " Place!\n" + win.name + "\n" + Mathf.Round((float)win.fitness * 1000) / 1000;
                ns.Remove(win);
            }
        }
        else if(mode == GameMode.Campaign)
        {
            if (campAdd)
            {
                print("Check");
                mP.campScore += highestFit.fitness;
                campAdd = false;
            }

            cI.gameObject.SetActive(mode == GameMode.Campaign);
            Text t = cI.GetComponentInChildren<Text>();
            string showTxt = "";

            bool beatCourse = startFatt[0].lap.x > startFatt[0].lap.y;
            bool beatCamp = mP.campaignProgress > 4; // 4 because it starts at 0 remember

            cFailed.gameObject.SetActive(!beatCourse || beatCamp);
            cSucc.gameObject.SetActive(beatCourse && !beatCamp);

            showTxt = (beatCourse) ? "Congrats! Here is your score so far! Continue onwards to the next campaign course!\n" + Mathf.Round((float)mP.campScore * 100000) / 100000  : "Sorry! You have failed to beat the Campaign! Here is your score, now get back to training. Better luck next time!\n" + Mathf.Round((float)mP.campScore * 100000) / 100000;
            showTxt = (beatCamp) ? "YOU DID IT! YOU BEAT THE CAMPAIGN!!! Here is your final score\n" + Mathf.Round((float)mP.campScore * 100000) / 100000 +"" : showTxt;
            t.text = showTxt;

            if (beatCamp)
            {
                print("BEAT GAME Screenshot");
                //ScreenCapture.CaptureScreenshot(AddTimeFormated(startFatt[0].myName + "_winner.png"));
            }

        }
    }

    public void NextCourse()
    {
        mP.campaignProgress+=.5f;
        RestartRoom();
    }

    /// <summary>
    /// Generates a placing suffix
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    string placeSuff(int i)
    {
        if (i == 0)
        {
            return "rst";
        }
        else if (i==1)
        {
            return "nd";
        }
        else
        {
            return "rd";
        }
    }

    /// <summary>
    /// This prevents the user from being able to save a Forrest without a unique name
    /// </summary>
    public void CheckNameInput()
    {
        snEB.interactable = nfF.text != "";
    }

    //
    public void SaveNExit(bool save)
    {
        if (save)
        {
            SaveForrest(nfF.text, highestFitBrain);

            if (saveTheData)
            {
                SaveData(nfF.text);
            }
        }

            SceneManager.LoadScene("menu");
    }

    // 
    public void SaveTheData()
    {
        saveTheData = saveTheData ? (false) : (true);
    }

    // 
    void SaveForrest(string n, string b)
    {
        if (!Directory.Exists(forrestPath))
        {
            Directory.CreateDirectory(forrestPath);
        }

        string forrestFile = Path.Combine(forrestPath,"Forrest " + n + ".frst");

        StreamWriter sW = new StreamWriter(forrestFile);

        sW.Write(n+"\n"+b);
        sW.Close();
    }

    void SaveData(string n)
    {
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        string comb = AddTimeFormated(n);

        string dataFile = Path.Combine(dataPath, "Forrest " + comb + ".csv");

        string dataF = "";
        string dataA = "";

        for (int i = 0; i < fits.Count; i++)
        {
            dataF += fits[i] + ((i < fits.Count - 1) ? "," : "");
            dataA += avgs[i] + ((i < fits.Count - 1) ? "," : "");
        }

        StreamWriter sW = new StreamWriter(dataFile);

        sW.Write(dataF + "\n" + dataA);
        sW.Close();

    }

    private static string AddTimeFormated(string n)
    {
        string comb = n + "_" + System.DateTime.Now;
        comb = comb.Replace('/', '-');
        comb = comb.Replace(':', '-');
        comb = comb.Replace(' ', '_');
        return comb;
    }

    /// <summary>
    /// This enables Forrest saving & waits for the day to end
    /// </summary>
    public void Save()
    {
        saved = true;
        SB.interactable = false;
        SB.GetComponentInChildren<Text>().text = "Waiting for day to end...";
    }

    // 
    float GetAverageFitness(List<ForrestCTRL> fC)
    {
        float avg = 0;
        for (int i = 0; i < fC.Count; i++)
        {
            avg += fC[i].fitness;
        }

        avg /= fC.Count;

        return avg;
    }

    /// <summary>
    /// This is where Forrest will pair up his best attempts & learn from them, hopefully
    /// </summary>
    void LearnFromAttempts()
    {
        // First get a reference to the gameobject of all past attempts
        GameObject[] pastAttempts = GameObject.FindGameObjectsWithTag("Passive");

        // Now create a new list to store them all in
        List<NN> allNN = new List<NN>();

        // Next loop through those past attempts, grab a ref to their NN & add them into this linq list of all NNs
        for (int i = 0; i < pastAttempts.Length; i++)
        {
            allNN.Add(pastAttempts[i].GetComponent<ForrestCTRL>().nn);
        }

        NN[] parents= new NN[0];

        // Get the sum of all fitness scores

        // Loop through & assign all NNs a probability based on max above

        // 

        // 
        if (CrossOver == COMode.Random)
        {
            RandomCrossOver(allNN);
        }
        if (CrossOver == COMode.Slice)
        {
            SliceCrossOver(allNN);
        }
    }

    NN[] Get2ProbBrains(List<NN> allNN)
    {
        // set a reference for our fitness sum
        double fitSum = 0;

        // now we need an array to store all the probabilities that are mapped to our allNN list
        double[] allProb = new double[allNN.Count];

        // add all fitness scores to fitness sum
        for (int i = 0; i < allNN.Count; i++)
        {
            fitSum += allNN[i].fitness;
            //print("allNN["+i+"].fitness = " + allNN[i].fitness);
        }

        // now assign our probabilities to our allProb array
        for (int i = 0; i < allProb.Length; i++)
        {
            allProb[i] = allNN[i].fitness / fitSum;
        }

        // now we reorder the array from highest to lowest
        double[] newProb = BubbleSort(false, allProb);

        for (int i = 0; i < newProb.Length; i++)
        {
            //print("newProb[" + i + "]*fitMum = " + newProb[i] * fitSum);
        }

        // create a new array that represents TP range max
        double[] ranges = new double[newProb.Length];

        // set the first range to be the first all
        ranges[0] = newProb[0];

        // set the rest of the ranges based on the previous range
        for (int i = 1; i < ranges.Length; i++)
        {
            ranges[i] = newProb[i] + ranges[i - 1];
        }

        // we need 2 NNs to store our parents
        NN[] ps = new NN[2];

        ps[0] = FindProbParent(allNN, fitSum, newProb, ranges);
        ps[1] = FindProbParent(allNN, fitSum, newProb, ranges);

        if (day > 3)
        {
            while (ps[1] == ps[0])
            {
                ps[1] = FindProbParent(allNN, fitSum, newProb, ranges);
            }
        }

        //print(ps[0].fitness + " & " + ps[1].fitness);
        return ps;
    }

    NN FindProbParent(List<NN> allNN, double fitSum, double[] newProb, double[] ranges)
    {
        NN ret = new NN();

        // randomly choose a value in that range
        float choser = Random.value;

        // get a reference to 2 intergers that we need to reverse & remap back to the allNN list for our 2 parents
        int reversal = 0;

        // if the choser value is less than our first range then we will need to reverse the first in the ranges array
        if (choser < ranges[0])
        {
            reversal = 0;
        }

        // now check for if the chooser value falls inbetween the range for any other ranges & if so set the reversal to that
        for (int i = 1; i < ranges.Length; i++)
        {
            if (choser > ranges[i - 1] && choser < ranges[i])
            {
                reversal = i;
            }
        }

        // 
        for (int i = 0; i < allNN.Count; i++)
        {
            // vvv really good for debugging the whole reversal process vvv
            //print(newProb[reversal] * fitSum + " =? " + allNN[i].fitness + " " + IsApproximatelyEqualTo(newProb[reversal] * fitSum,allNN[i].fitness, .00001f));

            if (IsApproximatelyEqualTo(newProb[reversal] * fitSum, allNN[i].fitness, .00001f))
            {
               ret = allNN[i];
                break;
            }
        }
        return ret;
    }

    bool IsApproximatelyEqualTo(double initialValue, double value, double maximumDifferenceAllowed)
    {
        double a = (initialValue > value) ? initialValue : value;
        double b = (initialValue < value) ? initialValue : value;

        // Handle comparisons of floating point values that may not be exactly the same
        return ((a - b) < maximumDifferenceAllowed);
    }

    double[] BubbleSort(bool low2High, double[] prob)
    {
        int checker = 1;
        while (checker != 0)
        {
            checker = 0;

            if (low2High)
            {
                for (int i = 1; i < prob.Length; i++)
                {
                    if (prob[i - 1] > prob[i])
                    {
                        double temp = prob[i - 1];
                        prob[i - 1] = prob[i];
                        prob[i] = temp;
                        checker++;
                    }
                }
            }
            else
            {
                for (int i = 1; i < prob.Length; i++)
                {
                    if (prob[i - 1] < prob[i])
                    {
                        double temp = prob[i - 1];
                        prob[i - 1] = prob[i];
                        prob[i] = temp;
                        checker++;
                    }
                }
            }
        }

        return prob;
    }

    NN[] GetFittest2Brains(List<NN> allNN)
    {
        // We need 2 new NNs to hold the top 2 performers
        NN[] parents = new NN[2];

        // Set the First Parent to the result of our highest fitness
        parents[0] = allNN[ReturnHighest(allNN)];

        // 
        if (parents[0].fitness > highestFit.fitness)
        {
            highestFit = parents[0];
            //highestFitBrain = highestFit.ReadBrain();
        }

        // Remove that highest performant from the list of all NNs IF there is more than one on the list so that we can run the process again & find the second highest
        if (allNN.Count > 1)
        {
            allNN.Remove(allNN[ReturnHighest(allNN)]);
        }

        // Second parent is chosen
        parents[1] = allNN[ReturnHighest(allNN)];
        return parents;
    }

    /// <summary>
    /// Makes a slice in one of the parents & inserts the other parent for cross over
    /// </summary>
    /// <param name="parents"></param>
    void SliceCrossOver(List<NN> aNN)
    {
        NN[] parents = new NN[2];

        if (selMode == SelMode.Top2)
        {
            parents = GetFittest2Brains(aNN);
        }

        // Loop through all day attempts & set the attempt ini string to a new string offspring
        for (int i = 0; i < attIniString.Length; i++)
        {
            if (selMode == SelMode.Percent)
            {
                parents = Get2ProbBrains(aNN);
            }

            if (i > attIniString.Length - 3)
            {
                RandomizeAParentWeights(parents);
            }

            // Set each attempt
            attIniString[i] = GenerateOffspringBrain(parents);
        }

        // Set the attempt ini data to the attempt ini string
        for (int i = 0; i < attIniData.Length; i++)
        {
            for (int j = 0; j < attIniData[i].Length; j++)
            {
                attIniData[i][j] = float.Parse(attIniString[i].Split(',')[j]);
            }
        }
    }

    /// <summary>
    /// This will randomize a parent's weights at random
    /// </summary>
    /// <param name="parents"></param>
    void RandomizeAParentWeights(NN[] parents)
    {
        // set r that will hold a randomly generated brain
        float[] r = new float[29];

        // loop through & generate a brain
        for (int k = 0; k < r.Length; k++)
        {
            r[k] = RandomWeight();
        }

        // coin2 chooses the parent to randomize
        int coin2 = Random.Range(0, parents.Length);

        // randomize the weights for the chosen parent
        parents[coin2].IniWeights(r);
    }

    /// <summary>
    /// This returns a brain in string form after slice crossing over the parents
    /// </summary>
    /// <param name="parents"></param>
    /// <returns></returns>
    string GenerateOffspringBrain(NN[] parents)
    {
        // First create slice start & stop points
        int start = Random.Range(0, 29);
        int stop = Random.Range(start, 29);

        // Then create the offspring brain string
        string offBrain = "";

        // Loop through the first selected parent values all the way until we hit the start of the cut & add that to the offspring's brain
        for (int i = 0; i < start; i++)
        {
            offBrain += parents[0].ReadBrain().Split(',')[i] + ",";
        }

        // Loop through the second selected parent values from the start of the cut to the end of the cut & add that to the offspring's brain
        for (int i = start; i < stop; i++)
        {
            offBrain += parents[1].ReadBrain().Split(',')[i] + ",";
        }

        // Finally loop through the first selected parent values again from the end of the cut to the end of the brain sequence & add that to the offspring's brain
        for (int i = stop; i < 29; i++)
        {
            // Checks for if at the end of loop or not for comma
            bool com = i != 28;

            // The adding part
            offBrain += parents[0].ReadBrain().Split(',')[i] + (com ? "," : string.Empty);
        }

        // Last thing we need to to is mutate the offspring's bring using probability so lets create a new offspring brain
        string newOffBrain = "";

        // Then loop through the old brain & using probability mutate the element
        for (int i = 0; i < offBrain.Split(',').Length; i++)
        {
            // Mutation = between 0-1
            float mut = Random.value;

            // if mut < mutation probability then mutate the element
            bool doMut = mut < mutationProb;

            // Checks for if at the end of loop or not for comma
            bool com = i != offBrain.Split(',').Length-1;

            // The adding part, sorry this is so scary looking :/
            newOffBrain += (float.Parse(offBrain.Split(',')[i]) + ((doMut) ? Random.Range(-mutationRate, mutationRate) : 0)) + (com ? "," : string.Empty);
        }
        
        // Return the off spring brain
        return newOffBrain;
    }

    /// <summary>
    /// This takes a list of NN as inputs & creates a new generation from the list by randomizing each element in the sequence from 2 parents
    /// </summary>
    /// <param name="parents"></param>
    void RandomCrossOver(List<NN> aNN)
    {
        NN[] parents = new NN[2];

        // 
        if (selMode == SelMode.Top2)
        {
            parents = GetFittest2Brains(aNN);
        }
        // This randomizes the next gen with a gene randomly choosen from a parent, but only for the first 10
        for (int i = 0; i < attIniData.Length; i++)
        {

            // 
            if (selMode == SelMode.Percent)
            {
                parents = Get2ProbBrains(aNN);
            }

            // if generating for brain max - 2
            if (i > attIniData.Length - 3)
            {
                RandomizeAParentWeights(parents);
            }

            for (int j = 0; j < attIniData[i].Length; j++)
            {
                // coin randomizes between the 2 parents, this is used for selecting between 2 elements in a brain
                int coin = Random.Range(0, parents.Length);

                // Mutation = between 0-1
                float mut = Random.value;

                // if mut < mutation probability then mutate the element
                bool doMut = mut < mutationProb;

                // Generate a new brain by choosing 1/2 elements from a parent for each element in the brain sequence
                attIniData[i][j] = parents[coin].GetBrain()[j] + ((doMut) ? Random.Range(-mutationRate, mutationRate) : 0);
            }
        }
    }

    /// <summary>
    /// Return the NN with the highest fitness
    /// </summary>
    /// <param name="allNN"></param>
    /// <returns></returns>
    int ReturnHighest(List<NN> allNN)
    {
        double checker = 0;
        int id = 0;

        // Check for the highest fitness within 
        for (int i = 0; i < allNN.Count; i++)
        {
            id = (allNN[i].fitness > checker) ? i : id;
            checker = (allNN[i].fitness > checker) ? allNN[i].fitness : checker;
        }

        return id;
    }

    /// <summary>
    /// Return the NN with the highest fitness
    /// </summary>
    /// <param name="allNN"></param>
    /// <returns></returns>
    int ReturnHighest(List<ForrestCTRL> allNN)
    {
        float checker = 0;
        int id = 0;

        // Check for the highest fitness within 
        for (int i = 0; i < allNN.Count; i++)
        {
            id = (allNN[i].fitness > checker) ? i : id;
            checker = (allNN[i].fitness > checker) ? allNN[i].fitness : checker;
        }

        return id;
    }

    /// <summary>
    /// Resets the camera back to the topdown position
    /// </summary>
    public void ResetCamy()
    {
            Camera.main.transform.parent = null;
        Camera.main.transform.position = camStarts[0];
        Camera.main.transform.eulerAngles = camStarts[1];
    }

    /// <summary>
    /// Toggles the control from human to AI
    /// </summary>
    public void ToggleControl()
    {
        intelli = intelli == IntelMode.Human ? IntelMode.AI : IntelMode.Human;
        //ctButt.text = intelli.ToString();
    }

    /// <summary>
    /// Toggles the camera from top to follow
    /// </summary>
    public void ToggleCamera()
    {
        int enumL = System.Enum.GetValues(typeof(CamMode)).Length-1;

        Camy = (Camy != (CamMode)enumL ? Camy += 1 : (CamMode)0);
        camButt.text = Camy.ToString();
    }

    public void ReverseNTest()
    {
        mP.reverse = mP.reverse ? false : true;
        RestartRoom();
    }
}
