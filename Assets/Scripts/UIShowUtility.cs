using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ПОКУПКИ КАРТОЧКИ ЖД RAILROAD")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject utilityUIPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text mortgagePriceText;//ценник за отель
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;//стоимость приобретения
    [SerializeField] TMP_Text playerMoneyText;//всего денег (DESIGN) если понадобится'

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "БАНК: " + currentCash + "BYN";
    }
    private void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityUI;
    }

    private void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityUI;
    }
    private void Start()
    {
        utilityUIPanel.SetActive(false);
    }

    void ShowBuyUtilityUI(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;//что покупает
        playerReference = currentPlayer;//кто покупает

        //TOP PANEL CONTENT:
        utilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color

        //СТОИМОСТЬ ПОСТРОЙКИ DESIGN
        mortgagePriceText.text = node.MortgagedValue + "BYN";//стоимость залога

        //BOTTOM PANEL CONTENT:
        utilityPriceText.text = "ПРИОБРЕСТИ ЗА <color=green>" + node.price + "BYN";
        playerMoneyText.text = "БАНК: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton НАСТРОЙКИ КНОПКИ
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            utilityPriceText.text = "ПРИОБРЕСТИ ЗА <color=red>" + node.price + "BYN";
            buyUtilityButton.interactable = false;
        }
        //СНАЧАЛА ЗАПОЛНЯЕТСЯ КОНТЕНТ, ЗАТЕМ ПОКАЗЫВАЮ ПАНЕЛЬ:
        utilityUIPanel.SetActive(true);
    }
    public void BuyUtilityButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //СООБЩЕНИЕ К КАРТОЧКЕ, ЧТО ОНА ПРИОБРЕТАЕТСЯ ИГРОКОМ
        playerReference.BuyProperty(nodeReference);
        //мб ЗАКРЫТЬ карточку

        //или сделать кнопку неактивной(чтобы не купить 29999 раз мисскликами)
        buyUtilityButton.interactable = false;
    }
    public void CloseBuyUtilityButton()//ВЫЗЫВАЕТСЯ КНОПКОЙ "ПРИОБРЕСТИ ЗА ???BYN"
    {
        //ЗАКРЫТЬ ПАНЕЛЬ UI
        utilityUIPanel.SetActive(false);
        //NodeReference и PlayerReference нужно ОБНУЛИТЬ - работа с карточкой закончена, МОЖНО ЧИСТИТЬ
        nodeReference = null;
        playerReference = null;
    }
}
