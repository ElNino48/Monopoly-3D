using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseMusicChoicePanelButton : MonoBehaviour
{
    [SerializeField] GameObject musicMenuPanel;
    bool isMenuOpened = false;
    public void toggleOpen()
    {
        isMenuOpened = !isMenuOpened;
        musicMenuPanel.SetActive(isMenuOpened);
    }
}
