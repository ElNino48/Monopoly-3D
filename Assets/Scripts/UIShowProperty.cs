using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ПОКУПКИ КАРТОЧКИ")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject propertyUIPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text rentPriceText;//0 домиков
    [SerializeField] TMP_Text oneHouseRentPriceText;//1 домик 
    [SerializeField] TMP_Text twoHouseRentPriceText;//2 домика
    [SerializeField] TMP_Text threeHouseRentPriceText;//3 домика
    [SerializeField] TMP_Text fourRentPriceText;//4 домика
    [SerializeField] TMP_Text hotelRentPriceText;//отель
    [Space]
    [SerializeField] TMP_Text housePriceText;//ценник за 1 дом
    [SerializeField] TMP_Text mortgagePriceText;//ценник за отель
    [Space]
    [SerializeField] Button buyPropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;//стоимость приобретения
    [SerializeField] TMP_Text playerMoneyText;//всего денег (DESIGN) если понадобится

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "БАНК: " + currentCash + "BYN";
    }
    private void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyUI;
    }

    private void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyUI;
    }

    private void Start()
    {
        propertyUIPanel.SetActive(false); 
    }

    void ShowBuyPropertyUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;//что покупает
        playerReference = currentPlayer;//кто покупает

        //TOP PANEL CONTENT:
        propertyNameText.text = node.name;
        colorField.color = node.propertyColorField.color;
        //INSIDE 
        rentPriceText.text = node.baseRent + "BYN";
        oneHouseRentPriceText.text = node.rentWithHouses[0] + "BYN";
        twoHouseRentPriceText.text = node.rentWithHouses[1] + "BYN";
        threeHouseRentPriceText.text = node.rentWithHouses[2] + "BYN";
        fourRentPriceText.text = node.rentWithHouses[3] + "BYN";
        hotelRentPriceText.text = node.rentWithHouses[4] + "BYN";

        //СТОИМОСТЬ ПОСТРОЙКИ 
        housePriceText.text = node.houseCost + "BYN";//стоимость домов и отелей одинаковая DESIGN
        mortgagePriceText.text = node.MortgageValue + "BYN";//стоимость залога
       
        //BOTTOM PANEL CONTENT:
        propertyPriceText.text = "ПРИОБРЕСТИ ЗА <color=green>" + node.Price + "BYN";
        playerMoneyText.text = "БАНК: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton НАСТРОЙКИ КНОПКИ
        if (currentPlayer.CanAffordNode(node.Price))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            propertyPriceText.text = "ПРИОБРЕСТИ ЗА <color=red>" + node.Price + "BYN";
            buyPropertyButton.interactable = false;
        }
        //СНАЧАЛА ЗАПОЛНЯЕТСЯ КОНТЕНТ, ЗАТЕМ ПОКАЗЫВАЮ ПАНЕЛЬ:
        propertyUIPanel.SetActive(true);
    }

    public void BuyPropertyButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //СООБЩЕНИЕ К КАРТОЧКЕ, ЧТО ОНА ПРИОБРЕТАЕТСЯ ИГРОКОМ
        playerReference.BuyProperty(nodeReference);
        //мб ЗАКРЫТЬ карточку

        //или сделать кнопку неактивной(чтобы не купить 29999 раз мисскликами)
        buyPropertyButton.interactable = false;
    }
    public void CloseBuyPropertyButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //ЗАКРЫТЬ ПАНЕЛЬ UI
        propertyUIPanel.SetActive(false);
        //NodeReference и PlayerReference нужно ОБНУЛИТЬ - работа с карточкой закончена, МОЖНО ЧИСТИТЬ
        nodeReference = null;
        playerReference = null;
    }
}
