using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ManagePropertyUI : MonoBehaviour
{
    [SerializeField] Transform cardHolder;//HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;//—¿Ã¿  ¿–“Œ◊ ¿
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
    //—≈“  ¿–“Œ◊≈  ŒƒÕŒ√Œ ÷¬≈“¿
    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        //nodeInSet.AddRange(nodes); == //nodesInSet = nodes;
        nodesInSet.AddRange(nodes);

        for (int i = 0; i < nodesInSet.Count; i++)//„ÂÌÂ‡ˆËˇ Í‡ÚÓ˜ÍË ‚ ÏÂÌÂ‰ÊÏÂÌÚÂ Ë Ú.‰.
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder,false);//—Œ«ƒ¿Õ»≈  ¿–“Œ◊ », œ–»¬«ﬂ ¿   cardHolder Ë Local Space (‰Îˇ Scaling'‡)
            ManageCardUI manageCardUI = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);//ƒŒ¡¿¬À≈Õ»≈  ¿–“Œ◊ » ¬ —≈“
            manageCardUI.SetCard(nodesInSet[i],owner,this);

        }
        var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);


        buyHouseButton.interactable = allSame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHouseButtonText.text = ("œŒ—“–Œ»“‹ <color=red>-" + nodesInSet[0].houseCost + "BYN");//DESIGN 
        sellHouseButtonText.text = ("œ–Œƒ¿“‹ <color=green>+" + nodesInSet[0].houseCost + "BYN");//DESIGN 
    }

    public void BuyHouseButton()
    {
        if (!CheckIfBuyAllowed())
        {
            //ERROR MSG "ÕÂÎ¸Áˇ ÍÛÔËÚ¸ ‰ÓÏ ÔÓÚÓÏÛ ˜ÚÓ Ó‰Ì‡ ËÁ Í‡ÚÓ˜ÂÍ Á‡ÎÓÊÂÌ‡"
            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHousesOrHotelEvenly(nodesInSet);
            UpdateHouseVisuals();
        }
        else
        {
            //Õ≈ ÃŒ∆≈“  ”œ»“‹ - Õ≈“ ƒ≈Õ≈√
            Debug.Log("Õ≈ ’¬¿“¿≈“ ƒ≈Õ≈√");
        }
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    void SellHouseButton()
    {
        //Ã¡ œ–Œ¬≈–»“‹, ≈—À» ≈—“‹ ’Œ“‹ 1 ƒŒÃ Õ¿ œ–Œƒ¿∆”
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE ÍÓÎ-‚Ó ‰ÂÌ¸„Ë ‚ ÒÍËÔÚÂ ManageUI
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
        if (nodesInSet.Any(n => n.IsMortgaged == true)) //≈—À» Œƒ»Õ »« Õ»’ «¿ÀŒ∆≈Õ
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
