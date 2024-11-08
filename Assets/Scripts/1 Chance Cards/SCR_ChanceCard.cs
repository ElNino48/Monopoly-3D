using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]

public class SCR_ChanceCard : ScriptableObject
{
    public Image eventImage;
    public string descriptionOnCard;//�������� (textOnCard)
    public int rewardMoney;//��������
    public int penaltyMoney;//��������
    public int moveToBoardIndex = -1; //�����������
    public bool payToPlayer;

    [Header("����������� �� �����")]

    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;

    [Header("�������� ���������")]

    public bool goToJail;
    public bool jailFreeCard;

    [Header("������������ �������")]

    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
