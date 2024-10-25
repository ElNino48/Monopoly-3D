using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ManageCardUI : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] TMP_Text unMortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;

    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUI propertyReference;

    //Color setColor, int numberOfBuildings, bool isMortgaged, int mortgageValue
    public void SetCard(MonopolyNode node, Player owner, ManagePropertyUI propertySet)
    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        //ЦВЕТ КАРТЫ
        if (node.propertyColorField != null)
        {
            colorField.color = node.propertyColorField.color;
        }
        else
        {
            colorField.color = Color.black;
        }

        //ПОКАЗАТЬ КАРТИНКУ "ЗАЛОЖЕНО"
        mortgageImage.SetActive(node.IsMortgaged);

        //ОБНОВЛЕНИЕ ТЕКСТА
        unMortgageValueText.text = "ВЫКУПИТЬ <color=red>-" + node.MortgagedValue +"BYN";
        mortgageValueText.text = "ЗАЛОЖИТЬ <color=green>+" + node.MortgagedValue + "BYN";
      
        //КНОПКІ + ТЕКСТ НА НИХ
        mortgageButton.gameObject.SetActive(!node.IsMortgaged);
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.gameObject.SetActive(node.IsMortgaged);
        unMortgageButton.interactable = node.IsMortgaged;
    }

    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed()) //if not allowed:
        {
            //ERROR MSG "Нельзя заложить потому что есть домики"
            return;
        }
        if(nodeReference.IsMortgaged)
        //ERROR сообщение об ошибке "Уже заложено" (на всякий, потому что кнопка должна вырубаться после юза"
        {
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());
        mortgageImage.SetActive(true);
        nodeReference.SetMortgaged(true);

        mortgageButton.gameObject.SetActive(false);//ANIMATION MORTGAGE //DESIGN
        mortgageButton.interactable = false;
        

        unMortgageButton.gameObject.SetActive(true);// ANIMATION MORTGAGE //
        unMortgageButton.interactable = true;
        ManageUI.instance.UpdateMoneyText();
        //FIX 22???
        //propertyReference.SetBuyHouseButton(false);

        //ИЛИ:??
        //propertyReference.GetComponent<ManagePropertyUI>().SetBuyHouseButton(false);
        //buyHouseButton Сделал неактивной потому что заложена карточка
        //но на одном ли СЕТе?
    }
    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        //ERROR сообщение об ошибке или чё то типо того
        {
            return;
        }
        if(playerReference.ReadMoney < nodeReference.MortgagedValue)
        {
            //ERROR "НЕТ ДЕНЕГ ЧТОБЫ ВЫКУПИТЬ КАРТОЧКУ"
            return;
        }
        playerReference.PayMoney(nodeReference.MortgagedValue);
        mortgageImage.SetActive(false);

        mortgageButton.gameObject.SetActive(true);//ANIMATION UNMORTGAGE //DESIGN
        mortgageButton.interactable = true;

        //ПОКАЗАТЬ КОЛ-ВО ЗДАНИЙ
        ShowBuildings();
        //FIX №18: показать картинку "Заложено"
        nodeReference.SetMortgaged(false);
        nodeReference.UnMortgageProperty();

        //FIX 22???
        //propertyReference.SetBuyHouseButton(true);

        unMortgageButton.gameObject.SetActive(false);// ANIMATION UNMORTGAGE //DESIGN
        unMortgageButton.interactable = false;
        ManageUI.instance.UpdateMoneyText();
    }

    public void ShowBuildings()
    {
        //СНАЧАЛА УБРАТЬ ВСЕ ДОМИКИ
        foreach (var houseIcon in buildings)
        {
            houseIcon.SetActive(false);
        }

        //ПОКАЗАТЬ КОЛ-ВО ЗДАНИЙ
        if (nodeReference.NumberOfHouses < 5)
        {
            for (int i = 0; i < nodeReference.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
            }
        }
        else
        {
            buildings[4].SetActive(true);
        }
    }
}
