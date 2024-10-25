using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder;//HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;//���� ��������
    [SerializeField] Button buyHouseButton,sellHouseButton;
    [SerializeField] TMP_Text buyHouseButtonText, sellHouseButtonText;
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();
    public Button GetBuyHouseButton => buyHouseButton;
    public void SetBuyHouseButton(bool isInteractive)
    {
        buyHouseButton.interactable = isInteractive;
    }
    //��� �������� ������ �����
    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        //nodeInSet.AddRange(nodes); == //nodesInSet = nodes;
        nodesInSet.AddRange(nodes);

        for (int i = 0; i < nodesInSet.Count; i++)//��������� �������� � ����������� � �.�.
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder,false);//�������� ��������, �������� � cardHolder � Local Space (��� Scaling'�)
            ManageCardUI manageCardUI = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);//���������� �������� � ���
            manageCardUI.SetCard(nodesInSet[i],owner,this);

        }
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);


        buyHouseButton.interactable = allSame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHouseButtonText.text = ("��������� <color=red>-" + nodesInSet[0].houseCost + "BYN");//DESIGN 
        sellHouseButtonText.text = ("������� <color=green>+" + nodesInSet[0].houseCost + "BYN");//DESIGN 
    }

    public void BuyHouseButton()
    {
        if (!CheckIfBuyAllowed())
        {
            //ERROR MSG "������ ������ ��� ������ ��� ���� �� �������� ��������"
            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHousesOrHotelEvenly(nodesInSet);
            UpdateHouseVisuals();
        }
        else
        {
            //�� ����� ������ - ��� �����
            Debug.Log("�� ������� �����");
        }
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    void SellHouseButton()
    {
        //�� ���������, ���� ���� ���� 1 ��� �� �������
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE ���-�� ������ � ������� ManageUI
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
        if (nodesInSet.Any(n => n.IsMortgaged == true)) //���� ���� �� ��� �������
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
