using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSettingsManager : MonoBehaviour
{
    public GameObject GameSceneGraphicsSettingsPanel;
    public GameObject GameSceneAudioSettingsPanel;
    public GameObject GameSceneOthersSettingsPanel;

    private int currentPanelIndex = 0;
    private GameObject[] panels;
    private void Start()
    {
        panels = new GameObject[]
        {
            GameSceneGraphicsSettingsPanel,
            GameSceneAudioSettingsPanel,
            GameSceneOthersSettingsPanel
        };
        ShowPanel(currentPanelIndex);
    }

    public void ShowPanel(int index)
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);

            Debug.Log(index + "=index");
        }
        Debug.Log(index + "=index");
        panels[index].SetActive(true);
    }
    public void NextPanel()
    {
        currentPanelIndex++;
        if (currentPanelIndex >= panels.Length)
        {
            currentPanelIndex = 0;
        }
        ShowPanel(currentPanelIndex);
    }
    public void PreviousPanel()
    {
        currentPanelIndex--;
        if (currentPanelIndex < 0)
        {
            currentPanelIndex = panels.Length - 1;
        }
        ShowPanel(currentPanelIndex);
    }
}
