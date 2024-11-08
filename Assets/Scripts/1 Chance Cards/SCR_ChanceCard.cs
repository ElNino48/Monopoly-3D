using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]

public class SCR_ChanceCard : ScriptableObject
{
    public Image eventImage;
    public string descriptionOnCard;//Описание (textOnCard)
    public int rewardMoney;//ПОЛУЧИТЬ
    public int penaltyMoney;//ПОТЕРЯТЬ
    public int moveToBoardIndex = -1; //ПЕРЕМЕЩЕНИЕ
    public bool payToPlayer;

    [Header("Перемещения по карте")]

    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;

    [Header("Тюремные настройки")]

    public bool goToJail;
    public bool jailFreeCard;

    [Header("СанИнспекция бизнеса")]

    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
