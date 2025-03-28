using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Calendar : MonoBehaviour
{
    public TMP_Text dayText;
    public TMP_Text phaseText;
    public TMP_Text eventText;
    [SerializeField] Animator lightingChanger;
    bool isDaytime;
    [SerializeField] List<GameObject> lightpoles;
    string eventName = "�������";
    int playerCount = 0;
    public enum DayPhase
    {
        Day,
        Night
    }
    public DayPhase currentPhase;
    int currentTurn = 0;

    void Start()
    {
        isDaytime = true;
        //StartTurn(playersCount);
    }

    public void StartTurn(int playersCount)
    {
        Debug.Log("current turn = " + currentTurn);
        this.playerCount = playersCount;
        ++currentTurn;       
        
        Debug.Log("++current turn / playersCount = " + currentTurn / playersCount);
        switch ((currentTurn / playersCount) % 2)
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
        CheckStoryEvents();
        UpdateCalendarUI();
    }

    void ExecuteDayPhase()
    {
        if (!isDaytime)
        {
            lightingChanger.SetTrigger("SetDay");
            isDaytime = true;
            foreach(GameObject pole in lightpoles)
            {
                pole.transform.GetChild(1).GetComponent<Animator>().SetTrigger("SetDayPoles");
            }
        }
    }

    void ExecuteNightPhase()
    {
        if (isDaytime)
        {
            lightingChanger.SetTrigger("SetNight");
            isDaytime = false;
            foreach (GameObject pole in lightpoles)
            {
                pole.transform.GetChild(1).GetComponent<Animator>().SetTrigger("SetNightPoles");
            }
        }
        IncreasePropertyCost(1f);
    }
    void IncreasePropertyCost(float percentage)
    {

    }

    void UpdateCalendarUI()
    {
        dayText.text = $"���� : { currentTurn / (playerCount * 2) + 1}"; //���� �������� ����� ����������� 2 ����� ����� + 1 ���������
        phaseText.text = $"{ (currentPhase == DayPhase.Day? phaseText.text = "����" : phaseText.text = "����")}";
        eventText.text = $"{ eventName}";
    }
    void CheckStoryEvents()
    {
        Debug.Log("check story current turn = " + currentTurn);
        Debug.Log("playerCount = " + playerCount);
        Debug.Log(currentTurn / playerCount);
        if (currentTurn / playerCount == 1)//1-� ����
        {
            TriggerExhibition();
        }
        else if (currentTurn / playerCount == 3)//2-� ����
        {
            TriggerBlackMarket();
        }
        else if (currentTurn / playerCount == 4)//3-� ����
        {
            TriggerColdTimes();
        }
        else eventName = "�������";
    }

    void TriggerExhibition()
    {
        eventName = "��������";
    }
    void TriggerBlackMarket()
    {
        eventName = "������ �����";
    }
    void TriggerColdTimes()
    {
        eventName = "���������";

    }
}
