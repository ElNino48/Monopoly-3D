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
    [SerializeField] string defaultDayName = "�������";
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
            if (currentTurn == 2)//1-� ����
            {
                currentEvent = new ExhibitionEvent();
            }
            else if (currentTurn == 4)//2-� ����
            {
                currentEvent = new BlackMarketEvent();
            }
            else if (currentTurn == 5)//3-� ����
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
        dayText.text = $"���� : {currentTurn / 2}"; //���� �������� ����� ����������� 2 ����� �����
        //phaseText.text = $"{ (currentPhase == DayPhase.Day ? phaseText.text = "����" : phaseText.text = "����")}";
        phaseText.text = currentPhase == DayPhase.Day ? "����" : "����";

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
