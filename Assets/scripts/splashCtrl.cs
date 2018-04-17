using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class splashCtrl : MonoBehaviour
{

    public enum version { HD, portrait }
    public version resolution;

    public float speed = 5;
    public float timeTilBite = 1;
    public float timeFromBiteToEnd = 1;
    public Sprite sprCookie;
    public Sprite sprCookieBite;
    public Sprite sprSEFDStuffTXT;
    public Sprite txrBackdrop;
    private Transform sprinkles;
    private Image theCookie;
    private Image theText;
    private Image Backdrop1;
    private Image Backdrop2;
    private float moveY;
    private float timer;

    // Use this for initialization
    void Start()
    {
        sprinkles = GameObject.Find("Backdrop").GetComponent<Transform>();
        theCookie = GameObject.Find("Cookie").GetComponent<Image>();
        theCookie.sprite = sprCookie;
        theText = GameObject.Find("SEFDStuff Text").GetComponent<Image>();
        theText.sprite = sprSEFDStuffTXT;
        Backdrop1 = GameObject.Find("Backdrop1").GetComponent<Image>();
        Backdrop2 = GameObject.Find("Backdrop2").GetComponent<Image>();
        Backdrop1.sprite = txrBackdrop;
        Backdrop2.sprite = Backdrop1.sprite;

        // Check the platform
        if (Application.isMobilePlatform)
        {
            resolution = version.portrait;
        }
        else
        {
            resolution = version.HD;
        }

        if (resolution == version.portrait)
        {
            Backdrop1.rectTransform.localScale = Vector3.one * 2;
            Backdrop2.rectTransform.localScale = Vector3.one * 2;
            theCookie.rectTransform.localScale = Vector3.one * .95f;
            theText.rectTransform.localScale = Vector3.one * .70f;
            theText.rectTransform.localPosition += Vector3.down * 256;
        }

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (moveY > -1080)
        {
            moveY -= speed;
        }
        else
        {
            moveY = 0;
        }
        sprinkles.localPosition = new Vector3(sprinkles.localPosition.x, moveY, sprinkles.localPosition.z);
        if (timer > timeTilBite)
        {
            theCookie.sprite = sprCookieBite;
        }
        if (timer > (timeTilBite + timeFromBiteToEnd))
        {
            if (!Application.isMobilePlatform)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                SceneManager.LoadScene("disclaimer");
            }
        }
    }
}
