using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Credits : MonoBehaviour
{
    public static Credits instance;
    public float moveSpeed = 1.0f;
    public int yAxisStopCoordinate;
    public Vector2 startPos;
    public int speedMultiplier;
    private int speedMultiplierScript;
    public float buttonAppearTime;
    [SerializeField] Button speedUpButton;
    [SerializeField] Button skipButton;
    [SerializeField] Image fillSkipButtonBackgroundImage;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] Image fadeImage;
    bool isFilling = false;
    public float maxWidth = 400;
    float width = 0f;
    float fillTime = 2f;
    float step;
    public float fadeSpeed = 1f;
    IEnumerator StartFading()
    {
        Color color = fadeImage.color;

        Debug.Log("fadeIN");
        Debug.Log("1" + color.a);
        color.a = 0;
        
        while (color.a < 1f)
        {
            fadeImage.gameObject.SetActive(true);
            color.a += fadeSpeed * Time.deltaTime;
            fadeImage.color = color;
            yield return null;
        }
    }
    //IEnumerator StartFadingOut()
    //{
    //    speedUpButton.gameObject.SetActive(false);
    //    skipButton.gameObject.SetActive(false);
    //    Color color = fadeImage.color;
    //    Debug.Log("fadeOUT");
    //    Debug.Log("2" + color.a);
    //    color.a = 1;
    //    blackOverallBackground.gameObject.SetActive(true);
    //    Color colorBG = blackOverallBackground.color;
    //    colorBG.a = 1;
    //    while (color.a > 0f)
    //    {
    //        fadeImage.gameObject.SetActive(true);
    //        color.a -= fadeSpeed * Time.deltaTime;
    //        colorBG.a -= fadeSpeed * Time.deltaTime;
    //        fadeImage.color = color;
    //        blackOverallBackground.color = color;
    //        yield return null;
    //    }
    //    blackOverallBackground.gameObject.SetActive(false);
    //    creditsPanel.gameObject.SetActive(false);
    //}
    private void Awake()
    {
        instance = this;
        speedUpButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        fillSkipButtonBackgroundImage.gameObject.SetActive(true);   
        fillSkipButtonBackgroundImage.rectTransform.sizeDelta = new Vector2(0, fillSkipButtonBackgroundImage.rectTransform.sizeDelta.y);
        step = maxWidth / (fillTime*60);
        speedMultiplierScript = 1;
        ResetPosition();
    }
    private void FixedUpdate()
    {
        if (this.transform.localPosition.y <= yAxisStopCoordinate)
        {
            this.transform.Translate(Vector2.up * moveSpeed * speedMultiplierScript);
        }
        else ResetPosition(true);
        if (isFilling && width<400f)
        {
            fillSkipButtonBackgroundImage.rectTransform.sizeDelta = 
                new Vector2(fillSkipButtonBackgroundImage.rectTransform.sizeDelta.x+step,
                fillSkipButtonBackgroundImage.rectTransform.sizeDelta.y);
            width = fillSkipButtonBackgroundImage.rectTransform.sizeDelta.x;
        }
        else if(width >= 400f)
        {
            StopButtonBackgroundFill();
            creditsPanel.gameObject.SetActive(false);
        }
    }
    public void CreditScreenAppear()
    {
        float opacity = 0f;
        creditsPanel.SetActive(false);
        opacity += creditsPanel.GetComponent<Image>().color.a;
    }
    public void SpeedUpCredits()
    {
        speedMultiplierScript += speedMultiplier;
    }
    public void ResetSpeed()
    {
        speedMultiplierScript = 1;
    }
    public void ResetPosition()
    {
        StartCoroutine(StartFading());
        speedUpButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        StartCoroutine(showButtons());
        this.transform.localPosition = startPos;
    }
    public void ResetPosition(bool isEnded)
    {
        speedMultiplier =3;
        speedMultiplierScript = 1;
        speedUpButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        StartCoroutine(showButtons());
        this.transform.localPosition = startPos;
        creditsPanel.SetActive(false);
    }

    IEnumerator showButtons() {
        yield return new WaitForSeconds(buttonAppearTime);
        speedUpButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);
    }
    public void ButtonBackgroundFill()
    {
        //buttonWidth 31.10.24 18:48 = 400
        isFilling = true;
        fillSkipButtonBackgroundImage.gameObject.SetActive(true);
    }
    public void StopButtonBackgroundFill()
    {
        isFilling = false;
        fillSkipButtonBackgroundImage.gameObject.SetActive(false);
        width = 0;
        fillSkipButtonBackgroundImage.rectTransform.sizeDelta = new Vector2(0, fillSkipButtonBackgroundImage.rectTransform.sizeDelta.y);
    }
}
