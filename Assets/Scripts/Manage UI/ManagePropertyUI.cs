using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder;//HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;//САМА КАРТОЧКА
    [SerializeField] Button buyHouseButton,sellHouseButton;
    [SerializeField] TMP_Text buyHouseButtonText, sellHouseButtonText;
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();
    [SerializeField] GameObject scrollableObjectInPrefab;
    ScrollRect scrollAbility;
    public Button GetBuyHouseButton => buyHouseButton;
    public void SetBuyHouseButton(bool isInteractive)
    {
        buyHouseButton.interactable = isInteractive;
    }
    //СЕТ КАРТОЧЕК ОДНОГО ЦВЕТА
    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {

        scrollAbility = scrollableObjectInPrefab.GetComponent<ScrollRect>();
        scrollAbility.enabled = false;
        playerReference = owner;
        //nodeInSet.AddRange(nodes); == //nodesInSet = nodes;
        nodesInSet.AddRange(nodes);
        if (nodesInSet.Count > 3)
        {
            scrollAbility.enabled = true;
        }
        for (int i = 0; i < nodesInSet.Count; i++)//генерация карточки в менеджменте и т.д.
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder,false);//СОЗДАНИЕ КАРТОЧКИ, ПРИВЗЯКА К cardHolder и Local Space (для Scaling'а)
            ManageCardUI manageCardUI = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);//ДОБАВЛЕНИЕ КАРТОЧКИ В СЕТ
            manageCardUI.SetCard(nodesInSet[i],owner,this);

        }
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);


        buyHouseButton.interactable = allSame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHouseButtonText.text = ("ПОСТРОИТЬ <color=red>-" + nodesInSet[0].houseCost + "BYN");//DESIGN 
        sellHouseButtonText.text = ("ПРОДАТЬ <color=green>+" + nodesInSet[0].houseCost/2 + "BYN");//DESIGN 
        if (nodes[0].type != MonopolyNodeType.Property)
        {
            buyHouseButton.interactable = false;
            sellHouseButton.interactable = false;
        }

    }

    public void BuyHouseButton()
    {
        if (!CheckIfBuyAllowed())
        {
            //ERROR MSG
            string message = "Нельзя купить дом потому что одна из карточек <color=red>заложена";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHousesOrHotelEvenly(nodesInSet);
            string message = "Дом построен.";
            ManageUI.instance.UpdateSystemMessage(message);
            UpdateHouseVisuals();
        }
        else
        {
            //НЕ МОЖЕТ КУПИТЬ - НЕТ ДЕНЕГ
            string message = "Недостаточно средств!";
            ManageUI.instance.UpdateSystemMessage(message);
        }
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    void SellHouseButton()
    {
        //МБ ПРОВЕРИТЬ, ЕСЛИ ЕСТЬ ХОТЬ 1 ДОМ НА ПРОДАЖУ
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE кол-во деньги в скрипте ManageUI
        UpdateHouseVisuals();
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    bool CheckIfSellAllowed()
    {//LINQ
        if (nodesInSet.Any(n => n.NumberOfHouses > 0)) 
        {
            return true;
        }  
        return false; 
    }

    bool CheckIfBuyAllowed()
    {
        if (nodesInSet.Any(n => n.IsMortgaged == true)) //ЕСЛИ ОДИН ИЗ НИХ ЗАЛОЖЕН
        {
            return false;
        }
        return true;
    }

    public bool CheckIfMortgageAllowed()
    {//LINQ
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return false;
        }
        return true;
    }
    void UpdateHouseVisuals()
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUI>().ShowBuildings();
        }    
    }
}
