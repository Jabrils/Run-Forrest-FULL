using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour {
    public Text vT;
    public RectTransform theMenu, theList, theCList, logo;
    public Scrollbar sB, cO, sel;
    public Image sBI, lvlIm, forrIm, cIB;
    public Color[] sBC;
    public Text sBT, cOT, lST, aST, theLvlTxt, forrCountTxt, sOT;
    public Slider lS, aS;
    public Toggle rT, rcT, smT;
    public Button[] coursesButts, forrestsButts;
    public Button RUN, bSC;
    public int offsetLvl, offsetForr;
    public bool listingOpen, listingCOpen;
    string[] menuLoad;

    menuParams mP;

    Vector3[] list = new Vector3[]
    {
        new Vector3(1400,-80),
        new Vector3(430,-80)
    };

    Vector3[] listC = new Vector3[]
{
        new Vector3(148,-159),
        new Vector3(-960,115)
};

    Vector3[] men = new Vector3[]
    {
        Vector3.zero,
        new Vector3(-345,0)
    };

    float[] logoX = new float[] { 0, -395 };

    // 
    void Start()
    {
        // Set the appropriate version number on the top left of screen
        vT.text = "v" + Application.version;

        menuParams.Check4MenuParam();

        // reference to our menu params
        mP = GameObject.Find("menuParams").GetComponent<menuParams>();
        mP.Reset();
        mP.adCount.x++;

        // 
        if(Application.isMobilePlatform && mP.adCount.x>=mP.adCount.y)
        {
            //Advertisement.Show();
            mP.adCount.x = 0;
        }

        // 
        coursesButts = GameObject.Find("Levels").GetComponentsInChildren<Button>();

        // 
        forrestsButts = GameObject.Find("Forrests").GetComponentsInChildren<Button>();

        // Now entirely sure what I have to do this to make Campaign work, too lazy to investigate in depth. >_>
        ToggleListing();
        ToggleListing();

        menuLoad = LoadMenuData();

        sB.value = Mathf.Round(int.Parse(menuLoad[0]));
        cO.value = Mathf.Round(int.Parse(menuLoad[1]));
        sel.value = Mathf.Round(int.Parse(menuLoad[2]));
        smT.isOn = bool.Parse(menuLoad[3]);
        rT.isOn = bool.Parse(menuLoad[4]);
        rcT.isOn = bool.Parse(menuLoad[5]);
        lS.value = int.Parse(menuLoad[6]);
        aS.value = int.Parse(menuLoad[7]);

    }

    void SaveMenuData()
    {
        StreamWriter sW = new StreamWriter(Path.Combine(ctrl.menuPath, "setts.set"));
            string write = "0,0,0,false,true,false,1,4";
        //sW.Write(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", sB.value.ToString(), cO.value.ToString(), smT.isOn.ToString(), rT.isOn.ToString(), rcT.isOn.ToString(), lS.value.ToString(), aS.value.ToString()));
        sW.Write(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", sB.value, cO.value, sel.value, smT.isOn, rT.isOn, rcT.isOn, lS.value, aS.value));
        sW.Close();
    }

    string[] LoadMenuData()
    {
        CheckForMenuFolder();

        string dat = "";

        try
        {
            StreamReader sR = new StreamReader(Path.Combine(ctrl.menuPath, "setts.set"));
            dat = sR.ReadToEnd();
            sR.Close();
        }
        catch
        {
            string write = "0,0,0,false,true,false,1,4";
            StreamWriter sW = new StreamWriter(Path.Combine(ctrl.menuPath, "setts.set"));
            sW.Write(write);
            sW.Close();

            dat = write;
        }

        return dat.Split(',');
        }

    void CheckForMenuFolder()
    {
        if (!Directory.Exists(ctrl.menuPath))
        {
            Directory.CreateDirectory(ctrl.menuPath);
        }
    }

    // 
    void Update()
    {
        // Will exit the game if on the menu screen
        if (Input.GetAxis("Power") > 0)
        {
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.BackQuote))
        {
            mP.sesh = true;
        }
        else
        {
            mP.sesh = false;
        }

        listingOpen = theList.localPosition == list[1];
        listingCOpen = theCList.localPosition == listC[1];
    
        cIB.enabled = (!listingOpen);
        forrCountTxt.text = (listingCOpen && mP.brains.Count>0) ? "Choose Your Forrest: " + mP.brains[mP.brains.Count-1].Split('\n')[0] : forrCountTxt.text;

        bSC.interactable = (!listingOpen && mP.brains.Count > 0);
        bSC.gameObject.SetActive(listingCOpen);

        if (listingOpen)
        {
            sB.value = Mathf.Round(sB.value);
            mP.mode = (ctrl.GameMode)sB.value;
            sBI.color = sBC[(int)sB.value];
            sBT.text = ((ctrl.GameMode)sB.value).ToString();

            cO.value = Mathf.Round(cO.value);
            mP.CO = (ctrl.COMode)cO.value;
            cOT.text = ((ctrl.COMode)cO.value).ToString();

            sel.value = Mathf.Round(sel.value);
            mP.SEL = (ctrl.SelMode)sel.value;
            sOT.text = ((ctrl.SelMode)sel.value).ToString();

            mP.laps = (int)lS.value;
            lST.text = "Laps: " + lS.value;

            mP.attempts = (int)aS.value;
            aST.text = "Attempts per day: " + aS.value;

            mP.slowMode = smT.isOn;
            mP.randomizeWeights = rT.isOn;
            mP.reverse = rcT.isOn;

            bool isTraining = ((int)sB.value == 0);

            cO.interactable = isTraining;
            sel.interactable = isTraining;
            rT.interactable = isTraining && mP.brains.Count == 0;
            aS.interactable = isTraining;
            smT.interactable = isTraining;

            bool holdForRace = (mP.mode == ctrl.GameMode.Test && mP.brains.Count == 0);

            RUN.interactable = (mP.lvlData != "" && mP.lvlData != null && !holdForRace);
        }
    }

    /// <summary>
    /// Brings over the listing for Forrest Loading
    /// </summary>
    public void ToggleListing()
    {
        if(listingCOpen)
        {
            ToggleCListing();
        }

        theMenu.localPosition = (theMenu.localPosition == men[1]) ? men[0] : men[1];
        theList.localPosition = (theList.localPosition == list[1]) ? list[0] : list[1];
        logo.localPosition = (logo.localPosition.x == logoX[1]) ? new Vector3(logoX[0], logo.localPosition.y, logo.localPosition.z) : new Vector3(logoX[1], logo.localPosition.y, logo.localPosition.z);
        SetCourseButtons();
        SetForrButtons();
    }

    /// <summary>
    /// Brings over the listing for Forrest Loading
    /// </summary>
    public void ToggleCListing()
    {
        if (listingOpen)
        {
            mP.Reset();
            ToggleListing();
        }

        theMenu.localPosition = (theMenu.localPosition == men[1]) ? men[0] : men[1];
        theCList.localPosition = (theCList.localPosition == listC[1]) ? listC[0] : listC[1];
        theCList.localScale = (theCList.localPosition == listC[1]) ? Vector3.one*1.75f : Vector3.one;
        logo.localPosition = (logo.localPosition.x == logoX[1]) ? new Vector3(logoX[0], logo.localPosition.y, logo.localPosition.z) : new Vector3(logoX[1], logo.localPosition.y, logo.localPosition.z);
        SetForrButtons();
    }

    /// <summary>
    /// Goes to room
    /// </summary>
    /// <param name="room"></param>
    public void GoToRoom(string room)
    {
        SaveMenuData();
        SceneManager.LoadScene(room);
    }

    public void SwitchOffSlowMode()
    {
        smT.isOn = false;
    }

    /// <summary>
    /// Setting the Forrest Load Buttons
    /// </summary>
    void SetForrButtons()
    {
        string[] forrs = LoadAllForrestNames();
        Button[] bs = forrIm.GetComponentsInChildren<Button>();

        // 
        bool scroll = (forrs.Length > 16);

        //forrCountTxt.text = "Forrests Loaded: " + mP.brains.Count;

        //
        for (int i = 0; i < (scroll ? 16 : forrs.Length); i++)
        {
            string pass = forrs[i+offsetForr];
            string data = ReturnForrData(i + offsetForr);
            bs[i + 2].onClick.AddListener(() =>
            {
                mP.campNN = data;

                if (!mP.brains.Contains(data))
                {
                    mP.brains.Add(data);
                }
                else
                {
                    mP.brains.Remove(data);
                }

                forrCountTxt.text = "Forrests Loaded: " + mP.brains.Count;
            });
            bs[i + 2].GetComponentInChildren<Text>().text = forrs[i + offsetForr];
        }

        // set the max offset for scrolling
        int offsetMax = forrs.Length - 16;

        // 
        forrestsButts[0].onClick.RemoveAllListeners();
        forrestsButts[1].onClick.RemoveAllListeners();

        // Add button listeners to the up & down buttons
        forrestsButts[0].onClick.AddListener(() => { ResetButtons(forrestsButts,true); offsetForr -= (offsetForr > 0) ? 1 : 0; SetForrButtons(); });
        forrestsButts[1].onClick.AddListener(() => { ResetButtons(forrestsButts, true); offsetForr += (offsetForr < offsetMax) ? 1 : 0; SetForrButtons(); });

        // This is only for up & down buttons
        forrestsButts[0].interactable = scroll && offsetForr != 0;
        forrestsButts[1].interactable = scroll && offsetForr != offsetMax;

        // this is for ALL other buttons
        for (int i = forrs.Length + 2; i < coursesButts.Length; i++)
        {
            forrestsButts[i].interactable = false;
            forrestsButts[i].GetComponent<Image>().color = Color.clear;
            forrestsButts[i].GetComponentInChildren<Text>().text = "";
        }
    }

    // 
    void SetCourseButtons()
    {
        string[] lvls = LoadAllLevelNames();
        Button[] bs = lvlIm.GetComponentsInChildren<Button>();

        // 
        bool scroll = (lvls.Length > 16);

        //
        for (int i = 0; i < (scroll ? 16 : lvls.Length); i++)
        {
            string pass = lvls[i+offsetLvl];
            string data = ReturnLvlData(i + offsetLvl);
            bs[i + 2].onClick.AddListener(() => { theLvlTxt.text = pass; mP.lvlName = pass ; mP.lvlData = data; });
            bs[i+2].GetComponentInChildren<Text>().text = lvls[i + offsetLvl];
        }

        // set the max offset for scrolling
        int offsetMax = lvls.Length - 16;

        //
        coursesButts[0].onClick.RemoveAllListeners();
        coursesButts[1].onClick.RemoveAllListeners();

        // Add button listeners to the up & down buttons
        coursesButts[0].onClick.AddListener(() => { ResetButtons(coursesButts, false); offsetLvl -= (offsetLvl>0) ? 1 : 0;  SetCourseButtons(); });
        coursesButts[1].onClick.AddListener(() => { ResetButtons(coursesButts, false); offsetLvl += (offsetLvl < offsetMax) ? 1 : 0; SetCourseButtons(); });

        // This is only for up & down buttons
            coursesButts[0].interactable = scroll && offsetLvl!=0;
            coursesButts[1].interactable = scroll && offsetLvl!=offsetMax;

        // this is for ALL other buttons
        for (int i = lvls.Length + 2; i < coursesButts.Length; i++)
        {
            coursesButts[i].interactable = false;
            coursesButts[i].GetComponent<Image>().color = Color.clear;
            coursesButts[i].GetComponentInChildren<Text>().text = "";
        }
    }

    // 
    void ResetButtons(Button[] bs, bool off)
    {
        int a = off ? 1 : 0;
        for (int i = 0; i < bs.Length-a; i++)
        {
            bs[i].onClick.RemoveAllListeners();
            if (i > 1)
            {
                bs[i].GetComponentInChildren<Text>().text = "NULL";
            }
        }
    }

    // 
    public static string ReturnLvlData(int index)
    {
        int mult = (Application.isEditor) ? 2 : 1;
        CheckForLvlDirectory();
        StreamReader sR = new StreamReader(Directory.GetFiles(ctrl.lvlsPath)[index * mult]);
        return sR.ReadToEnd();
        sR.Close();
    }

    // 
    public static string ReturnLvlData(string name)
    {
        int mult = (Application.isEditor) ? 2 : 1;
        CheckForLvlDirectory();
        StreamReader sR = new StreamReader(Path.Combine(ctrl.lvlsPath,name+".fcour"));
        return sR.ReadToEnd();
        sR.Close();
    }

    // 
    public string ReturnForrData(int index)
    {
        int mult = (Application.isEditor) ? 2 : 1;
        CheckForForrestDirectory();
        StreamReader sR = new StreamReader(Directory.GetFiles(ctrl.forrestPath)[index * mult]);
        return sR.ReadToEnd();
        sR.Close();
    }

    // 
    public string[] LoadAllLevelNames()
    {
        string[] ret = new string[0];

        CheckForLvlDirectory();

        string[] st = Directory.GetFiles(ctrl.lvlsPath).Select(Path.GetFileName)
                                     .ToArray();

        int counter = 0;

        if (Application.isEditor)
        {
            //
            for (int i = 0; i < st.Length; i++)
            {
                if (st[i].Contains(".meta"))
                {
                    st[i] = null;
                    counter--;
                }
            }

            string[] newSt = new string[st.Length + counter];

            for (int i = 0; i < newSt.Length; i++)
            {
                for (int j = 0; j < st.Length; j++)
                {
                    if (st[j] != null)
                    {
                        newSt[i] = st[j];
                        st[j] = null;
                        break;
                    }
                }

                newSt[i] = newSt[i].Replace(".fcour", string.Empty);
                newSt[i] = newSt[i].Replace("_", " ");
            }

            ret = newSt;
        }
        else
        {

            for (int i = 0; i < st.Length; i++)
            {
                st[i] = st[i].Replace(".fcour", string.Empty);
                st[i] = st[i].Replace("_", " ");
            }

            ret = st;
        }

        return ret;
    }

    // 
    public string[] LoadAllForrestNames()
    {
        string[] ret = new string[0];

        CheckForForrestDirectory();

        string[] st = Directory.GetFiles(ctrl.forrestPath).Select(Path.GetFileName)
                                     .ToArray();

        int counter = 0;

        if (Application.isEditor)
        {
            //
            for (int i = 0; i < st.Length; i++)
            {
                if (st[i].Contains(".meta"))
                {
                    st[i] = null;
                    counter--;
                }
            }

            string[] newSt = new string[st.Length + counter];

            for (int i = 0; i < newSt.Length; i++)
            {
                for (int j = 0; j < st.Length; j++)
                {
                    if (st[j] != null)
                    {
                        newSt[i] = st[j];
                        st[j] = null;
                        break;
                    }
                }

                newSt[i] = newSt[i].Replace(".frst", string.Empty);
            }

            ret = newSt;
        }
        else
        {
            for (int i = 0; i < st.Length; i++)
            {
                st[i] = st[i].Replace(".frst", string.Empty);
            }

            ret = st;
        }

        return ret;
    }

    // 
    public static void CheckForLvlDirectory()
    {
        if (!Directory.Exists(ctrl.lvlsPath))
        {
            Directory.CreateDirectory(ctrl.lvlsPath);
        }
    }

    // 
    void CheckForForrestDirectory()
    {
        if (!Directory.Exists(ctrl.forrestPath))
        {
            Directory.CreateDirectory(ctrl.forrestPath);
        }
    }

    //
    public void StartCampaign()
    {
            mP.laps = 2;
            mP.mode = ctrl.GameMode.Campaign;
            GoToRoom("course");
    }

    // 
    public void BuyMeAPizza()
    {
        Application.OpenURL("http://sefdstuff.com/buymepizza/");
    }
}
