using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shimmy : MonoBehaviour {
    RectTransform rT;
    float speed;
    int shake;
    bool tfAhead, tfBehind;
    float bounds = 50;

    // Use this for initialization
    void Start ()
    {
        shake = RandomShake();
        rT = GetComponent<RectTransform>();
    }

    // 
    int RandomShake()
    {
        return (Random.Range(0, 2) == 0) ? 1 : -1;
    }

    // Update is called once per frame
    void Update () {
        // 
        if (rT.transform.localPosition.x > bounds)
        {
            tfAhead = true;
        }
        else if (rT.transform.localPosition.x < -bounds)
        {
            tfBehind = true;
        }

        // 
        if (tfAhead)
        {
            shake = -1;
            if (rT.transform.localPosition.x <= 0)
            {
                tfAhead = false;
            }
        }
        else if (tfBehind)
        {
            shake = 1;
            if (rT.transform.localPosition.x >= 0)
            {
                tfBehind = false;
            }
        }
        else
        {
            shake = RandomShake();
        }

        // 
        speed = 1f * shake;
        rT.transform.localPosition = rT.transform.localPosition + Vector3.right * speed;

    }
}
