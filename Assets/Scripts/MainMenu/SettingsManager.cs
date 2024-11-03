using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public GameObject GraphicsSettingsPanel;
    public GameObject AudioSettingsPanel;
    public GameObject OthersSettingsPanel;

    private int currentPanelIndex = 0;
    private GameObject[] panels;
    private void Start()
    {
        panels = new GameObject[] 
        { 
            GraphicsSettingsPanel,
            AudioSettingsPanel,
            OthersSettingsPanel
        };
        ShowPanel(currentPanelIndex);
    }

    public void ShowPanel(int index)
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
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
