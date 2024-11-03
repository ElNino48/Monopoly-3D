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
            endGameScreen.SetActive(false);
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
      }

    private IEnumerator FadeToBlack(GameObject blackoutPanel, float fadeDuration)
    {
        endGameScreen.SetActive(false);
        blackoutPanel.SetActive(true);
        Image image = blackoutPanel.GetComponent<Image>();
        Color color = image.color;
        color.a = 0;
        image.color = color;
        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            color.a = Mathf.Clamp01(time / fadeDuration);
            image.color = color;
            yield return null;
        }
        color.a = 1;
        image.color = color;
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
