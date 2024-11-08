using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Community Card", menuName = "Monopoly/Cards/Community")] 

public class SCR_CommunityCard : ScriptableObject
{
    public Image eventImage;
    public string descriptionOnCard;//Описание (textOnCard)
    public int rewardMoney;//ПОЛУЧИТЬ
    public int penaltyMoney;//ПОТЕРЯТЬ
    public int moveToBoardIndex = -1; //ПЕРЕМЕЩЕНИЕ
    public bool collectFromPlayer;

    [Header("Тюремные настройки")]

    public bool goToJail;
    public bool jailFreeCard;

    [Header("СанИнспекция бизнеса")]

    public bool streetRepairs;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
