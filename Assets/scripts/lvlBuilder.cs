using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class lvlBuilder : MonoBehaviour {
    public enum Type { builder, block };
    public Type type;
    public Button saveB;
    public InputField saveIF;
    public GameObject S;
    public Image Msg, rButt;

    public Vector2 size = Vector2.one*20;
    public Material[] mat;
    public List<GameObject> selHexx = new List<GameObject>();
    MeshFilter mesh;
    public string saveFile;
    public Vector4 startPt;
    public Vector3 stSave;
    int stLoc, dir;

    public int id;
    public bool perim, reverse, remove;
    bool spent;

    GameObject hex;
    ctrl C;

    // Use this for initialization
    void Start ()
    {
        C = Camera.main.GetComponent<ctrl>();

        if (type == Type.builder)
        {
            GameObject F = new GameObject("MAP");

            // 
            hex = Resources.Load<GameObject>("hex");

            bool final = (saveFile == "FINAL");

            if (!final) 
            {
                GenHex(F);
            }
            else
            {
                C.Ff.SetActive(true);
            }

            // Set the right demensions
            F.transform.localScale = (Vector3.one * 5) + (Vector3.up * 2);

            // 
            if (SceneManager.GetActiveScene().name == "course")
            {

                    GameObject stSp = GameObject.Find("StartSphere");
                if (!final)
                {
                    stSp.transform.position = new Vector3(startPt.x,0, startPt.z);
                    stSp.transform.eulerAngles = Vector3.up * (startPt.w + ((reverse) ? 180 : 0));
                }
                else
                {
                    stSp.transform.eulerAngles = Vector3.up * (((reverse) ? 180 : 0));
                }

                C.NewDay();
            }
            else
            {
                // 
                Msg.gameObject.SetActive(false);
            }
        }
    }

    void GenHex(GameObject F)
    {
        List<int> data = new List<int>();

        int[] t = new int[saveFile.Split(',').Length-1];

        bool loading = (t.Length > 5);

        string vectConv = saveFile.Split(',')[saveFile.Split(',').Length - 1];

        // 
        if (SceneManager.GetActiveScene().name == "course")
        {
            startPt = new Vector4(float.Parse(vectConv.Split('#')[0]), float.Parse(vectConv.Split('#')[1]), float.Parse(vectConv.Split('#')[2]), float.Parse(vectConv.Split('#')[3]));
        }

        // 
        if (loading)
        {
            for (int i = 0; i < t.Length; i++)
            {
                t[i] = int.Parse(saveFile.Split(',')[i]);
            }

            data.AddRange(t);
        }

        for (int i = 0; i < size.x; i++)
        {
            if (!data.Contains(id))
            {
                GameObject H = Instantiate(hex);
                H.tag = "Obstacle";
                H.AddComponent<MeshCollider>();
                H.transform.position = new Vector3(i - (size.x / 2), 0, -(size.y / 2));
                H.transform.SetParent(F.transform);

                lvlBuilder lb = H.AddComponent<lvlBuilder>();
                lb.mat = mat;
                lb.type = Type.block;
                lb.id = id;
                lb.perim = true;

            }
            id++;

            for (int j = 1; j < size.y; j++)
            {
            if (!data.Contains(id))
                {
                    if (size.y > 1)
                    {
                        GameObject I = Instantiate(hex);
                        I.tag = "Obstacle";
                        I.AddComponent<MeshCollider>();
                        I.transform.position = new Vector3(i - (size.x / 2) - ((j % 2 == 1) ? .5f : 0), 0, (j * .87f) - (size.y / 2));
                        I.transform.SetParent(F.transform);

                        lvlBuilder lb2 = I.AddComponent<lvlBuilder>();
                        lb2.mat = mat;
                        lb2.type = Type.block;
                        lb2.id = id;

                        if (i == 0 || i == size.x-1 || j == size.y-1)
                        {
                            lb2.perim = true;
                        }
                    }
                }
                id++;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (type == Type.builder)
        {
                // Will load the menu if power is pressed
                if (Input.GetAxis("Power") > 0)
            {
                GoToMenu();
            }

            if (SceneManager.GetActiveScene().name == "coursebuilder")
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // 
                if (Physics.Raycast(ray, out hit))
                {
                    lvlBuilder LB = hit.collider.GetComponent<lvlBuilder>();

                    // 
                    if (Input.GetMouseButton(0) && LB.perim == false)
                    {
                        if (!remove)
                        {
                            if (spent == false)
                            {
                                // change these to = first in selhexx array
                                stSave = hit.transform.position;
                                S.transform.position = stSave + (Vector3.up * 2.05f);
                                stLoc = LB.id;
                                spent = true;
                            }

                            SelectHex(hit.collider.gameObject);
                        }
                        else
                        {
                            RemoveHex(hit.collider.gameObject);

                            if (selHexx.Count > 0)
                            {
                                lvlBuilder LB2 = selHexx[0].GetComponent<lvlBuilder>();
                                int ch = LB2.id;

                                //
                                if (ch != stLoc)
                                {
                                    S.transform.position = LB2.transform.position + (Vector3.up * 2.05f);
                                    stSave = S.transform.position;
                                }

                            }
                            else
                            {
                                spent = false;
                                stSave = Vector3.zero;
                                S.transform.position = Vector3.one * 1000;
                            }
                        }
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        RemoveHex(hit.collider.gameObject);

                        if (selHexx.Count > 0)
                        {
                            lvlBuilder LB2 = selHexx[0].GetComponent<lvlBuilder>();
                            int ch = LB2.id;

                            //
                            if (ch != stLoc)
                            {
                                S.transform.position = LB2.transform.position + (Vector3.up * 2.05f);
                                stSave = S.transform.position;
                            }

                        }
                        else
                        {
                            spent = false;
                            stSave = Vector3.zero;
                            S.transform.position = Vector3.one * 1000;
                        }

                    }
                }

                // 
                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    saveB.interactable = (saveIF.text != "" && selHexx.Count > 5);
                }

                if (!Input.GetMouseButton(0))
                {
                    dir = 0;
                }
                // 
                if (Input.GetMouseButton(2))
                {
                    dir = (Input.GetKey(KeyCode.LeftShift)) ? -1 : 1;
                }

                S.transform.eulerAngles += Vector3.up * dir;
            }
        }
    }

    public void Remove()
    {

        Text rT = rButt.GetComponentInChildren<Text>();

        if (remove)
        {
            remove = false;
            rButt.color = Color.green;
            rT.text = "O";
        }
        else
        {
            remove = true;
            rButt.color = Color.red;
            rT.text = "X";
        }
    }

    public void RotateStart(int d)
    {
        dir = d;
    }

    // 
    public void GoToMenu()
    {
        SceneManager.LoadScene("menu");
    }

    // 
    public void SaveLvl()
    {
        saveFile = "";
        for (int i = 0; i < selHexx.Count; i++)
        {
            saveFile += selHexx[i].GetComponent<lvlBuilder>().id + ((i < selHexx.Count - 1) ? "," : string.Empty);
        }

        if (!Directory.Exists(ctrl.lvlsPath))
        {
            Directory.CreateDirectory(ctrl.lvlsPath);
        }

        string lvlFile = Path.Combine(ctrl.lvlsPath, CleanName(saveIF.text) + ".fcour");

        StreamWriter sW = new StreamWriter(lvlFile);
        sW.Write(saveFile+string.Format(",{0}#{1}#{2}#{3}", stSave.x, stSave.y, stSave.z,S.transform.eulerAngles.y));
        sW.Close();

        StartCoroutine(saveMessage());
    }

    /// <summary>
    /// shows a popup message that displays something
    /// </summary>
    /// <returns></returns>
    IEnumerator saveMessage()
    {
        Msg.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        Msg.gameObject.SetActive(false);
    }

    void Cave()
    {
        foreach (GameObject h in selHexx)
        {
            Destroy(h);
        }
    }

    // 
    string CleanName(string s)
    {
        return s.Replace(" ", "_");
    }

    // 
    public void Check4Saving()
    {
        saveB.interactable = (saveIF.text != "" && selHexx.Count>5);
    }

    /// <summary>
    /// This will appropriately select the hex
    /// </summary>
    void SelectHex(GameObject sel)
    {
        if (sel != null && !selHexx.Contains(sel))
        {
            selHexx.Add(sel);
            sel.GetComponent<Renderer>().material = mat[1];
        }
    }

    /// <summary>
    /// This will appropriately select the hex
    /// </summary>
    void RemoveHex(GameObject sel)
    {
        selHexx.Remove(sel);
        sel.GetComponent<Renderer>().material = mat[0];
    }
}
