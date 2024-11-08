using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndGameScreen : MonoBehaviour
{
    public GameObject endGameScreen;
    public GameObject blackoutPanel;
    public float fadeDuration = 2f;
    public Button toMainMenuButton;
    public static EndGameScreen instance;
    //public Image end1Lose1Face;
    //public Image end1Lose1Leave;
    //public Image end1Win1Face;
    //public Image end1Win1Leave;

    //Image endGameImage;

    //public bool isLose1FaceButtonPressed = false;
    //public bool isLose1LeaveButtonPressed = false;
    //public bool isWin1FaceButtonPressed = false;
    //public bool isWin1LeaveButtonPressed = false;

    //public void pressLose1FaceButton()
    //{
    //    isLose1FaceButtonPressed = true;
    //}
    //public void pressLose1LeaveButton()
    //{
    //    isLose1LeaveButtonPressed = true;
    //}
    //public void pressWin1FaceButton()
    //{
    //    isWin1FaceButtonPressed = true;
    //}
    //public void pressWin1LeaveButton()
    //{
    //    isWin1LeaveButtonPressed = true;
    //}
    private void Awake()
      {
          instance = this;
      }

    public void LoadEndGameScene(string sceneName)
    {        
        if (sceneName == "MainMenu")
        {
            //isGameEndedGameScene = true;
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        if(sceneName == "ToBeContinued")
        {
            //endGameScreen.SetActive(false);
            //end1Lose1Face.enabled = false;
            //end1Lose1Leave.enabled = false;
            //end1Win1Face.enabled = false;
            //end1Win1Leave.enabled = false;
            blackoutPanel.SetActive(true);
            StartCoroutine(FadeToBlack(blackoutPanel, fadeDuration));
        }
    }
      private IEnumerator LoadSceneAsync(string sceneName)
      {
            yield return StartCoroutine(FadeFromBlack(blackoutPanel, fadeDuration));
            endGameScreen.SetActive(true);
          
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            Debug.Log("LOADING MAIN MENU FROM GAME SCENE");
            yield return new WaitForSeconds(5f);
        }
        blackoutPanel.SetActive(false);
        endGameScreen.SetActive(false);
        //endGameImage.enabled = false;
        //endGameImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeToBlack(GameObject blackoutPanel, float fadeDuration)
    {
        endGameScreen.SetActive(false);
        blackoutPanel.SetActive(true);
        Image image = blackoutPanel.GetComponent<Image>();
        Color color = image.color;
        color.a = 0;
        image.color = color;
        //if (isLose1FaceButtonPressed)
        //{
        //    endGameImage = end1Lose1Face;
        //}
        //if (isLose1LeaveButtonPressed)
        //{
        //    endGameImage = end1Lose1Leave;
        //}
        //if (isWin1FaceButtonPressed)
        //{
        //    endGameImage = end1Win1Face;
        //}
        //if (isWin1LeaveButtonPressed)
        //{
        //    endGameImage = end1Win1Leave;
        //}
        //Color endConditionColor = endGameImage.color;
        //endConditionColor.a = 0;
        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            //endConditionColor.a = Mathf.Clamp01(time / fadeDuration);
            color.a = Mathf.Clamp01(time / fadeDuration);
            image.color = color;
            //endGameImage.color = endConditionColor;
            yield return null;
        }
        color.a = 1;
        image.color = color;
        //endGameImage.gameObject.SetActive(true);
        //endGameImage.enabled = true;
        endGameScreen.SetActive(true);
        color.a = 0;
        image.color = color;
        blackoutPanel.SetActive(false);
    }
    private IEnumerator FadeFromBlack(GameObject blackoutPanel, float fadeDuration)
    {
        blackoutPanel.SetActive(true);
        Image image = blackoutPanel.GetComponent<Image>();
        Color color = image.color;
        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            color.a =  Mathf.Clamp01(time / fadeDuration);
            image.color = color;
            yield return null;
        }
        endGameScreen.SetActive(false);
        //color.a = 0;
        //image.color = color;
    }

    public void LoseGameButton()
    {
        LoadEndGameScene("ToBeContinued");
    }
    public void FinishGameButton()
    {
        LoadEndGameScene("MainMenu");
    }
}
