using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class forVid : MonoBehaviour {
    public enum doing { inputVar, NNDemo };
    public doing scene;
    public float speed = 1;
    public RectTransform[] nodesI;
    public RectTransform[] nodesH;
    public RectTransform outp;
    public RectTransform forrest;
    Vector3[] stampI;
    Vector3[] stampH;
    Vector3 stampO;
    float lerp;

	// Use this for initialization
	void Start () {
        StampStart();
	}
	
	// Update is called once per frame
	void Update () {
        Text tt = GameObject.Find("IText").GetComponent<Text>();
        tt.enabled = scene == doing.inputVar;

        GameObject NND = GameObject.Find("NN Demo");

        lerp = (lerp<1) ? lerp+(Time.deltaTime*speed) : 0;

        // 
        if (scene == doing.inputVar)
        {
            tt.text = ""+Mathf.Round(Input.GetAxis("Horizontal")*100)/100;
        }
        else
        {

            // Inputs
            for (int j = 0; j < nodesI.Length; j++)
            {
                    for (int i = 0; i < nodesI[j].childCount - 1; i++)
                {
                    nodesI[j].GetChild(i + 1).transform.position = Vector3.Lerp(stampI[j], nodesH[i].transform.position, lerp);
                }
            }

            // Hidden
            for (int j = 0; j < nodesH.Length; j++)
            {
                for (int i = 0; i < nodesH[j].childCount - 1; i++)
                {
                    nodesH[j].GetChild(i + 1).transform.position = Vector3.Lerp(stampH[j], outp.transform.position, lerp);
                }
            }

            // Out
            outp.GetChild(1).transform.position = Vector3.Lerp(stampO, forrest.transform.position, lerp);

        }
    }

    void StampStart()
    {
        stampI = new Vector3[nodesI.Length];
        stampH = new Vector3[nodesH.Length];

        for (int i = 0; i < stampI.Length; i++)
        {
            stampI[i] = nodesI[i].GetChild(0).transform.position;
        }

        for (int i = 0; i < stampH.Length; i++)
        {
            stampH[i] = nodesH[i].GetChild(0).transform.position;
        }

        stampO = outp.GetChild(0).transform.position;

    }
}











