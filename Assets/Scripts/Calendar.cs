using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Calendar : MonoBehaviour
{
    public TMP_Text dayText;
    public TMP_Text phaseText;

    public enum DayPhase
    {
        Day,
        Night
    }
    public DayPhase currentPhase;
    private int currentTurn = 0;

    private void Start()
    {
        StartTurn();
    }

    void StartTurn()
    {
        switch (currentTurn % 2)
        {
            case 0:
                currentPhase = DayPhase.Day;
                ExecuteDayPhase();
                break;
            case 1:
                currentPhase = DayPhase.Night;
                ExecuteNightPhase();
                break;
        }
        currentTurn++;
    }

    void ExecuteDayPhase()
    {
        CheckStoryEvents();
    }

    void ExecuteNightPhase()
    {
        IncreasePropertyCost(1f);
    }
    void IncreasePropertyCost(float percentage)
    {

    }

    void UpdateCalendarUI()
    {
        dayText.text = $"демэ : { currentTurn / 2 + 1}";
        dayText.text = $"{ currentPhase}";

    }
    void CheckStoryEvents()
    {
        if (currentTurn == 2)
        {
            TriggerExhibition();
        }
        else if (currentTurn == 4)
        {
            TriggerBlackMarket();
        }
        else if (currentTurn == 5)
        {
            TriggerColdTimes();
        }
    }

    void TriggerExhibition()
    {

    }
    void TriggerBlackMarket()
    {

    }
    void TriggerColdTimes()
    {
            
    }
}
