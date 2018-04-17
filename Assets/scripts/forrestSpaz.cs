using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class forrestSpaz : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponentInChildren<Animator>().speed = 0;
    }

    // Update is called once per frame
    void Update () {
        //transform.rotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * Random.Range(-1f,1) * 75f);

        if (Input.GetAxis("A") > 0)
        {
            GetComponentInChildren<Animator>().speed = 2f;
            GetComponentInChildren<Renderer>().material.mainTexture = Resources.Load<Texture>("talk_text");
        }
    }
}
