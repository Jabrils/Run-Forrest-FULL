using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeuralNet;

[RequireComponent(typeof(Rigidbody))]
public class ForrestCTRL : MonoBehaviour {
    ctrl C;
    Rigidbody rb;
    Collider col;
    public NN nn = new NN(5,4);
    public string myName;
    public float movement;
    public float fitness;
    float[] ini;
    public bool ended, stLap;
    public Vector2 lap;
    public float[] inp;
    public string brain;

    bool menu;

    // Use this for initialization
    void Start() {
        menu = SceneManager.GetActiveScene().name == "menu";

        if (!menu)
        {
            // Grab reference to the controller script
            C = Camera.main.GetComponent<ctrl>();

            // Sets the max lap according to the controller
            lap.y = C.mLaps;
        }
        else
        {
            nn.IniWeights(new float[] { 3.472079f, 1.762525f, -2.266208f, 0.8920379f, -3.915989f, -1.762377f, -2.844904f, 3.381477f, 1.12464f, -3.086241f, 3.320154f, 0.1941123f, 0.1791953f, -3.122393f, 0.8971314f, 0.1158746f, 3.512217f, 1.440832f, 3.3429f, -3.377463f, -2.171291f, 1.523072f, -2.242229f, -2.650826f, 3.01321f, -3.341551f, 3.746894f, -1.755286f, -0.3875917f });
        }

        // Grab ref to ridgid body component
        rb = GetComponent<Rigidbody>();

        // Set rotation constraints so Forrest doesn't fall over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Grab ref to collider
        col = GetComponent<Collider>();

        // 
        tag = "Active";

        // Set the laps to 1 instead of 0 at every start no matter what
        lap.x = 1;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        brain = nn.ReadBrain();

        // Rotate the charater based on Horizonal Input & later NN Output
        transform.rotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * movement * 2.5f);

        // If attempt has ended
        if (!ended)
        {
            // Auto move Forrest forward
            rb.MovePosition(transform.position + transform.forward * (Time.deltaTime * 10));
        }

        // Set up a raycast hit for knowing what we hit
        RaycastHit hit;

        // Set up out 5 feelers for undertanding the world
        Vector3[] feeler = new Vector3[]
        {
            // 0 = L
            transform.TransformDirection(Vector3.left),
            // 1 - FL
            transform.TransformDirection(Vector3.left+Vector3.forward),
            // 2 - F
            transform.TransformDirection(Vector3.forward),
            // 3 = FR
            transform.TransformDirection(Vector3.right + Vector3.forward),
            // 4 = R
            transform.TransformDirection(Vector3.right),
        };

        // Use this to collect all feeler distances, then well pass them through our NN for an output
        inp = new float[feeler.Length];

        // Loop through all feelers
        for (int i = 0; i < feeler.Length; i++)
        {
            // See what all feelers feel
            if (Physics.Raycast(transform.position, feeler[i], out hit))
            {
                // If feelers feel something other than Forrest & nothing
                if (hit.collider != null && hit.collider != col)
                {
                    // Set the input[i] to be the distance of feeler[i]
                    inp[i] = hit.distance;
                }

            }

            // Draw the feelers in the Scene mode
            Debug.DrawRay(transform.position, feeler[i] * 10, Color.red);
        }

        // Add to our fitness every frame
        fitness += (ended) ? 0 : inp2fit(inp);

        // This sets the output text display to be the output of our NN
        if (!menu)
        {
            movement = ended ? 0 : ((C.intelli == ctrl.IntelMode.Human) ? Input.GetAxis("Horizontal") : nn.CalculateNN(inp));
        }
        else
        {
            movement = ended ? 0 : (nn.CalculateNN(inp));
        }

        // 
        if (!menu && !ended && lap.x>lap.y)
        {
            Freeze();
            CheckIfLast();
        }
    }

    /// <summary>
    /// This converts your inputs to a fitness value
    /// </summary>
    /// <param name="inps"></param>
    /// <returns></returns>
    float inp2fit(float[] inps)
    {
        float ret = 0;

        // 
        for (int i = 0; i < inps.Length; i++)
        {
            ret += inps[i];
        }

        return ((ret/inps.Length)/100)*lap.x;
    }

    // For when we collide with the walls
    void OnTriggerEnter(Collider col)
    {
        // 
        if (!menu && col.tag == "Obstacle")
        {
            Freeze();
        }

        // 
        if (col.tag == "Point")
        {
         if (!stLap && col.name == "startPoint")
            {
                stLap = true;
            }
         else if (stLap && col.name == "endPoint")
            {
                lap.x++;
                stLap = false;
            }
        }
    }

    public void Reset()
    {
        fitness = 0;
        lap.x = 1;
        nn.SetFitness(fitness);
        tag = "Active";
        GetComponent<CapsuleCollider>().enabled = true;
        ended = false;
        GetComponentInChildren<Animator>().speed = 1;
        Renderer[] children = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in children)
        {
            r.material = Resources.Load<Material>("forrest_full");
        }
    }

    void CheckIfLast()
    {
        // Grab a ref to every Active Forrest
        GameObject[] f = GameObject.FindGameObjectsWithTag("Active");

        // Update the active running badges is here for a reason I can't remember & are too lazy to figure out why it's here
        C.UpdateActiveRunBadges();

        // If there are no more then start new day!
        if (f.Length == 0)
        {
            C.NewDay();
        }
    }

    /// <summary>
    /// This will freeze Forrest in his tracks & save his current state;
    /// </summary>
    public void Freeze()
    {
        ended = true;
        tag = "Passive";
        GetComponentInChildren<Animator>().speed = 0;
        GetComponent<CapsuleCollider>().enabled = false;
        Renderer[] children = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in children)
        {
            r.material = Resources.Load<Material>("forrest_full 1");
        }

        nn.SetFitness(fitness);

        // Remove self from All Forrest Attempts Controller Listings
        C.allFatt.Remove(this);
            CheckIfLast();
    }

    /// <summary>
    /// Set the Genetic Code for the attempt
    /// </summary>
    /// <param name="i"></param>
    public void SetBrain(float[] i)
    {
        ini = i;
        nn.IniWeights(ini);
    }

    /// <summary>
    /// Set the Genetic Code for the attempt with name
    /// </summary>
    /// <param name="i"></param>
    public void SetBrain(float[] i, string n)
    {
        ini = i;
        nn.IniWeights(ini);
        nn.SetName(n);
        myName = nn.name;
        gameObject.name = myName;
    }
}
