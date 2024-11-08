using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Community Card", menuName = "Monopoly/Cards/Community")] 

public class SCR_CommunityCard : ScriptableObject
{
    public Image eventImage;
    public string descriptionOnCard;//�������� (textOnCard)
    public int rewardMoney;//��������
    public int penaltyMoney;//��������
    public int moveToBoardIndex = -1; //�����������
    public bool collectFromPlayer;

    [Header("�������� ���������")]

    public bool goToJail;
    public bool jailFreeCard;

    [Header("������������ �������")]

    public bool streetRepairs;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
