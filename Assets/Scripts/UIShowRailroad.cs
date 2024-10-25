using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ПОКУПКИ КАРТОЧКИ ЖД RAILROAD")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject railroadUIPanel;
    [SerializeField] TMP_Text railroadNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text oneRailroadRentPriceText;//1 ЖД
    [SerializeField] TMP_Text twoRailroadRentPriceText;//2 ЖД
    [SerializeField] TMP_Text threeRailroadRentPriceText;//3 ЖД
    [SerializeField] TMP_Text fourRailroadPriceText;//4 ЖД
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;//ценник за отель
    [Space]
    [SerializeField] Button buyRailroadButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;//стоимость приобретения
    [SerializeField] TMP_Text playerMoneyText;//всего денег (DESIGN) если понадобится

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "БАНК: " + currentCash + "BYN";
    }
    private void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadUI;
    }

    private void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadUI;
    }
    private void Start()
    {
        railroadUIPanel.SetActive(false);
    }
    void ShowBuyRailroadUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;//что покупает
        playerReference = currentPlayer;//кто покупает

        //TOP PANEL CONTENT:
        railroadNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;
        //INSIDE 
        //result = baseRent * (int)Mathf.Pow(2, amount-1);
        oneRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 1 - 1) + "BYN";
        twoRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 2 - 1) + "BYN";
        threeRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 3 - 1) + "BYN";
        fourRailroadPriceText.text = node.baseRent * (int)Mathf.Pow(2, 4 - 1) + "BYN";

        //СТОИМОСТЬ ПОСТРОЙКИ DESIGN
        mortgagePriceText.text = node.MortgagedValue + "BYN";//стоимость залога

        //BOTTOM PANEL CONTENT:
        propertyPriceText.text = "ПРИОБРЕСТИ ЗА <color=green>" + node.price + "BYN";
        playerMoneyText.text = "БАНК: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton НАСТРОЙКИ КНОПКИ
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            propertyPriceText.text = "ПРИОБРЕСТИ ЗА <color=red>" + node.price + "BYN";
            buyRailroadButton.interactable = true;
        }
        //СНАЧАЛА ЗАПОЛНЯЕТСЯ КОНТЕНТ, ЗАТЕМ ПОКАЗЫВАЮ ПАНЕЛЬ:
        railroadUIPanel.SetActive(true);
    }

    public void BuyRailroadButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //СООБЩЕНИЕ К КАРТОЧКЕ, ЧТО ОНА ПРИОБРЕТАЕТСЯ ИГРОКОМ
        playerReference.BuyProperty(nodeReference);
        //мб ЗАКРЫТЬ карточку

        //или сделать кнопку неактивной(чтобы не купить 29999 раз мисскликами)
        buyRailroadButton.interactable = false;
    }
    public void CloseBuyRailroadButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //ЗАКРЫТЬ ПАНЕЛЬ UI
        railroadUIPanel.SetActive(false);
        //NodeReference и PlayerReference нужно ОБНУЛИТЬ - работа с карточкой закончена, МОЖНО ЧИСТИТЬ
        nodeReference = null;
        playerReference = null;
    }
}
