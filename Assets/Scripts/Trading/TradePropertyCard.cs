using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class TradePropertyCard : MonoBehaviour
{
    MonopolyNode nodeReference;

    [SerializeField] Image colorField;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image typeImage; //УТИЛИТИ-ЖД-УЛИЦА
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] Toggle toggleButton;
    
    public void SetTradeCard(MonopolyNode node, ToggleGroup toggleGroup)
    {
        nodeReference = node;
        //Если цвет карточки есть, то заполняю, иначе - черный цвет
        colorField.color = (node.propertyColorField != null) ? node.propertyColorField.color : Color.black;
        propertyNameText.text = node.name;
        switch (node.type)
        {
            case MonopolyNodeType.Property:
                typeImage.sprite = houseSprite;
                typeImage.color = Color.white;//??DESIGN
                break;
            case MonopolyNodeType.Railroad:
                typeImage.sprite = railroadSprite;
                typeImage.color = Color.white;//??DESIGN
                break;
            case MonopolyNodeType.Utility:
                typeImage.sprite = utilitySprite;
                typeImage.color = Color.black;//DESIGN!!
                break;
        }
        mortgageImage.SetActive(node.IsMortgaged);
        propertyPriceText.text = node.price + "BYN";
        toggleButton.isOn = false;
        toggleButton.group = toggleGroup;
    }
    public MonopolyNode Node()//defолтный геттер
    {
        return nodeReference;
    } 
}
