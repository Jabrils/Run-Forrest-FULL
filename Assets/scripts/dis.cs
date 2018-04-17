using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class dis : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Next());
	}
	
    IEnumerator Next()
    {
        while(true)
        {
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene("menu");
        }
    }

    public void Download()
    {
        Application.OpenURL("http://sefdstuff.com/runforrest");
    }
}
