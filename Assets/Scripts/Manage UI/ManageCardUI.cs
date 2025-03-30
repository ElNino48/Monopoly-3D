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
        //���� �����
        if (node.propertyColorField != null)
        {
            colorField.color = node.propertyColorField.color;
        }
        else
        {
            colorField.color = Color.black;
        }

        //�������� �������� "��������"
        mortgageImage.SetActive(node.IsMortgaged);

        //���������� ������
        unMortgageValueText.text = "�������� <color=red>-" + node.MortgageValue +"BYN";
        mortgageValueText.text = "�������� <color=green>+" + node.MortgageValue + "BYN";
      
        //����ʲ + ����� �� ���
        mortgageButton.gameObject.SetActive(!node.IsMortgaged);
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.gameObject.SetActive(node.IsMortgaged);
        unMortgageButton.interactable = node.IsMortgaged;

        //��������� ������
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
        //���������� ��� ��������
        propertyNameText.text = nodeReference.name;
    }

    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed()) //if not allowed:
        {
            //ERROR MSG "������ �������� ������ ��� ���� ������"
            string message = "������ �������� ������ ��� ���� ������!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if(nodeReference.IsMortgaged)
        //ERROR ��������� �� ������ "��� ��������" (�� ������, ������ ��� ������ ������ ���������� ����� ���"
        {
            string message = "��� ��������!";
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

        //���:??
        //propertyReference.GetComponent<ManagePropertyUI>().SetBuyHouseButton(false);
        //buyHouseButton ������ ���������� ������ ��� �������� ��������
        //�� �� ����� �� ����?
    }
    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        //ERROR ��������� �� ������ ��� �� �� ���� ����
        {
            string message = "��� ���������!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if(playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            //ERROR "��� ����� ����� �������� ��������"
            string message = "������������ �������!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MortgageValue);
        mortgageImage.SetActive(false);

        mortgageButton.gameObject.SetActive(true);//ANIMATION UNMORTGAGE //DESIGN
        mortgageButton.interactable = true;

        //�������� ���-�� ������
        ShowBuildings();
        //FIX �18: �������� �������� "��������"
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
        //������� ������ ��� ������
        foreach (var houseIcon in buildings)
        {
            houseIcon.SetActive(false);
        }

        //�������� ���-�� ������
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
