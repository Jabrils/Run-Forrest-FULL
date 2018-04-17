using UnityEngine;
using System.Collections;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class takeScreencap : MonoBehaviour {

    public KeyCode whatKeyForScreens = KeyCode.Backspace;

    string savePath;
    string savedFile;
    string saveFolder;
    public int imgCount;

	// Use this for initialization
	void Awake () {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            savePath = "";
        }
        else
        {
            savePath = Application.dataPath + "/";
        }

        savedFile = savePath + "screencapdata.dat";
        saveFolder = savePath + "Screencaps";

        if (File.Exists(savedFile))
        {
            BinaryFormatter bF = new BinaryFormatter();
            FileStream theFile = File.Open(savedFile, FileMode.Open);
            int newImgCnt = (int)bF.Deserialize(theFile);
            imgCount = newImgCnt;
        }
        else
        {
            File.Create(savedFile);
        }
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyDown(whatKeyForScreens))
        {
            // Create the folder beforehand if not exists
            TakeScreenCap();
            PlayerPrefs.SetInt("imgCount", imgCount);
        }
	}

    public void TakeScreenCap()
    {
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        //
        ScreenCapture.CaptureScreenshot(saveFolder + "/screencap_" + imgCount + ".png");
            imgCount++;

        BinaryFormatter bF = new BinaryFormatter();
        FileStream theFile = File.Create(savedFile);

        bF.Serialize(theFile, imgCount);
        theFile.Close();
    }
}
