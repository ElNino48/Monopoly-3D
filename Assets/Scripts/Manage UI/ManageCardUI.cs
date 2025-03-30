using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ManageCardUI : MonoBehaviour
{
    [SerializeField] TMP_Text propertyNameText;

    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] TMP_Text unMortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;

    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    
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
        unMortgageValueText.text = "ВЫКУПИТЬ <color=red>-" + node.MortgageValue +"BYN";
        mortgageValueText.text = "ЗАЛОЖИТЬ <color=green>+" + node.MortgageValue + "BYN";
      
        //КНОПКІ + ТЕКСТ НА НИХ
        mortgageButton.gameObject.SetActive(!node.IsMortgaged);
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.gameObject.SetActive(node.IsMortgaged);
        unMortgageButton.interactable = node.IsMortgaged;

        //ЗАГРУЗИТЬ ИКОНКИ
        switch (nodeReference.type)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.blue;//ICON
            break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;//ICON
            break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;//ICON
            break;
        }
        //УСТАНОВИТЬ ИМЯ КАРТОЧКИ
        propertyNameText.text = nodeReference.name;
    }

    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed()) //if not allowed:
        {
            //ERROR MSG "Нельзя заложить потому что есть домики"
            string message = "Нельзя заложить потому что есть домики!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if(nodeReference.IsMortgaged)
        //ERROR сообщение об ошибке "Уже заложено" (на всякий, потому что кнопка должна вырубаться после юза"
        {
            string message = "Уже заложено!";
            ManageUI.instance.UpdateSystemMessage(message);
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
            string message = "Уже выкуплено!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if(playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            //ERROR "НЕТ ДЕНЕГ ЧТОБЫ ВЫКУПИТЬ КАРТОЧКУ"
            string message = "Недостаточно средств!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MortgageValue);
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
