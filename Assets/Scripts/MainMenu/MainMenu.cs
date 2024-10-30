using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : SoundEditor
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
    public void ContinueButton()
    {
        Debug.Log("1gere");
        insertedPlayerNameInput.text = rememberPlayerNameInput.text;
        PlaySound(1);
    }
    public void StartButton()
    {
        Debug.Log("2gere");
        foreach (var player in playerSelectionArray)
        {
            Debug.Log("3gere");
            if (player.activatePlayerToggle.isOn)
            {
                Debug.Log("4gere");
                Setting newSet = new Setting(player.nameInput.text, player.typeDropdown.value, player.colorDropdown.value);
                GameSettings.AddSetting(newSet);
            }
        }
        Debug.Log("5gere");
        SceneManager.LoadScene("Game");
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
