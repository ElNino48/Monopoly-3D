using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Calendar : MonoBehaviour
{
    public enum DayPhase
    {
        Day,
        Night
    }

    [SerializeField] TMP_Text dayText;
    [SerializeField] TMP_Text phaseText;
    [SerializeField] TMP_Text eventText;
    [SerializeField] Animator lightingChanger;
    [SerializeField] List<GameObject> lightpoles;
    public DayPhase currentPhase;
    StoryEvent currentEvent;
    [SerializeField] string defaultDayName = "Обычный";
    int currentTurn = 1;

    void Start()
    {
        currentPhase = DayPhase.Day;
    }

    public void StartTurn()
    {
        Debug.Log("current turn = " + currentTurn);
        currentTurn++;
        Debug.Log("current turn++ = " + currentTurn);

        switch (currentTurn % 2)
        {
            case 0:
                currentPhase = DayPhase.Night;
                ExecuteNightPhase();
                break;
            case 1:
                currentPhase = DayPhase.Day;
                ExecuteDayPhase();
                break;
        }
        CheckStoryEvents();
        UpdateCalendarUI();
    }

    void ExecuteDayPhase()
    {
        lightingChanger.SetTrigger("SetDay");
        foreach (GameObject pole in lightpoles)
        {
            pole.transform.GetChild(1).GetComponent<Animator>().SetTrigger("SetDayPoles");
        }
    }

    void ExecuteNightPhase()
    {
        lightingChanger.SetTrigger("SetNight");
        foreach (GameObject pole in lightpoles)
        {
            pole.transform.GetChild(1).GetComponent<Animator>().SetTrigger("SetNightPoles");
        }
    }

    void CheckStoryEvents()
    {
        if (currentEvent != null)
        {
            if(currentEvent.CurrentDuration >= currentEvent.EventDuration)
            {
                currentEvent.OnEventEnd();
                currentEvent = null;
            }
            else
            {
                currentEvent.CurrentDuration++;
            }
        }

        if (currentEvent == null)
        {
            if (currentTurn == 2)//1-я ночь
            {
                currentEvent = new ExhibitionEvent();
            }
            else if (currentTurn == 4)//2-я ночь
            {
                currentEvent = new BlackMarketEvent();
            }
            else if (currentTurn == 5)//3-е утро
            {
                currentEvent = new ColdTimesEvent();
            }

            if (currentEvent != null)
            {
                currentEvent.OnEventStart();
            }
        }
    }

    void UpdateCalendarUI()
    {
        dayText.text = $"ДЕНЬ : {currentTurn / 2}"; //День меняется когда завершилось 2 цикла ходов
        //phaseText.text = $"{ (currentPhase == DayPhase.Day ? phaseText.text = "Утро" : phaseText.text = "Ночь")}";
        phaseText.text = currentPhase == DayPhase.Day ? "Утро" : "Ночь";

        if (currentEvent != null)
        {
            eventText.text = currentEvent.EventName;
        }
        else
        {
            eventText.text = defaultDayName;
        }
    }
}
