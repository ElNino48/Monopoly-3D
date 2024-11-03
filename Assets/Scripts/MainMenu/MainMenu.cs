using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSelect
    {
        public TMP_InputField nameInput;
        public TMP_Dropdown typeDropdown;
        public TMP_Dropdown colorDropdown;
        public Toggle activatePlayerToggle;

    }
    [SerializeField] PlayerSelect[] playerSelectionArray;
    public TMP_InputField rememberPlayerNameInput;
    public TMP_InputField insertedPlayerNameInput;
    public string webURL;

    public Button startNewGameButton;
    public GameObject CreditsGameObject;
    //
    public GameObject loadingScreen;
    public GameObject blackoutPanel;
    public float fadeDuration = 2f;
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return StartCoroutine(FadeToBlack(blackoutPanel, fadeDuration));
        loadingScreen.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return new WaitForSeconds(2f);
        }

        blackoutPanel.SetActive(false);
        loadingScreen.SetActive(false);
    }

    private IEnumerator FadeToBlack(GameObject blackoutPanel, float fadeDuration)
    {
        blackoutPanel.SetActive(true);
        Debug.Log("Loading Game Scene from Main Menu Scene");
        Image image = blackoutPanel.GetComponent<Image>();
        Color color = image.color;
        for (float time = 0; time < fadeDuration; time += Time.deltaTime)
        {
            color.a = Mathf.Clamp01(time / fadeDuration);
            image.color = color;
            yield return null;
        }
        color.a = 1;
        image.color = color;
    }
    //
    public void ContinueButton()
    {
        insertedPlayerNameInput.text = rememberPlayerNameInput.text;
        //PlaySound(1);
    }
    public void StartButton()
    {
        foreach (var player in playerSelectionArray)
        {
            if (player.nameInput.text == "")
            {
                player.nameInput.text = "«уев";
            }
            if (player.activatePlayerToggle.isOn)
            { 
                
                Setting newSet = new Setting(player.nameInput.text, player.typeDropdown.value, player.colorDropdown.value);
                GameSettings.AddSetting(newSet);
            }
        }
        LoadScene("Game");
    }
    public void VisitDeveloper()
    {
        Application.OpenURL(webURL);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
