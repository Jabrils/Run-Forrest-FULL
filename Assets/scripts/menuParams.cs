using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using NeuralNet;

public class menuParams : MonoBehaviour {
    public ctrl.COMode CO;
    public ctrl.SelMode SEL;
    public ctrl.GameMode mode;
    public int laps, attempts, maxDays;
    public float campaignProgress;
    public bool randomizeWeights, reverse, sesh, slowMode;
    public string lvlData, lvlName, storedName;
    public List<string> brains = new List<string>();
    public Vector2 seshMark, adCount = new Vector2(3,6);
    public string campNN;
    public double campScore;
    public string[] campaign { get { return _campaign; } }

    string[] _campaign = new string[]
    {
        "105,86,66,87,67,48,69,50,70,91,90,111,92,112,113,114,115,116,136,157,177,197,217,218,237,238,257,258,277,297,296,295,274,293,313,312,332,351,350,349,348,347,326,325,345,324,323,302,282,281,261,241,221,202,201,182,162,142,143,122,123,103,104,84,-27.5#0#-28.25#331.9999",
        "86,87,88,89,90,91,92,93,94,115,135,136,155,156,175,195,215,194,214,213,192,191,170,189,188,208,228,249,250,271,252,273,274,295,315,335,314,334,353,352,351,350,349,348,328,327,306,286,266,246,226,225,204,224,243,244,263,264,283,284,303,282,301,281,261,241,221,181,161,141,121,102,103,84,105,85,107,201,-30#0#-23.9#0",
        "43,45,47,48,49,30,51,32,53,34,55,56,57,117,96,94,93,92,72,91,70,89,87,64,24,26,88,66,85,83,123,143,163,144,165,146,147,148,168,189,209,210,211,191,172,152,151,131,132,133,135,136,156,177,197,198,218,238,257,277,256,275,254,234,214,213,233,232,252,272,293,294,315,316,335,334,353,352,351,330,329,309,289,248,325,345,344,343,322,302,301,281,261,241,221,201,181,161,141,61,81,121,101,103,134,95,78,58,98,42,22,23,41,305,247,227,206,225,205,204,224,244,264,285,268,365,184,104,124,251,-42.5#0#-36.95#339.9999",
        "187,166,147,127,128,170,234,255,275,276,254,150,95,137,296,291,242,274,292,309,263,221,161,101,63,157,196,244,183,102,24,325,243,122,295,185,145,109,131,174,318,326,175,177,317,216,116,257,215,132,48,130,315,345,323,303,282,304,346,342,262,124,26,114,298,294,308,322,341,324,184,103,74,115,155,237,357,25,89,195,355,333,311,289,224,223,202,85,28,217,214,363,82,270,312,222,156,336,353,310,327,347,373,337,68,136,94,113,93,88,106,144,343,281,181,141,81,62,236,235,64,61,188,189,133,163,125,108,354,201,44,47,302,83,-7.5#0#-19.55#19.00001",
    };

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Reset()
    {
        laps = 0;
        attempts = 0;
        maxDays = 0;
        randomizeWeights = false;
        reverse = false;
        lvlData = "";
        lvlName = "";
        brains.Clear();
        seshMark = new Vector2(1, 0);
        storedName = string.Empty;
        campNN = "";
        campScore = 0;
        campaignProgress = 0;
    }

    public static void Check4MenuParam()
    {
        if (!GameObject.Find("menuParams"))
        {
            SceneManager.LoadScene("splash");
        }
    }


}
