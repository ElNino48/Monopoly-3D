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
        unMortgageValueText.text = "�������� <color=red>-" + node.MortgagedValue +"BYN";
        mortgageValueText.text = "�������� <color=green>+" + node.MortgagedValue + "BYN";
      
        //����ʲ + ����� �� ���
        mortgageButton.gameObject.SetActive(!node.IsMortgaged);
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.gameObject.SetActive(node.IsMortgaged);
        unMortgageButton.interactable = node.IsMortgaged;
    }

    public void MortgageButton()
    {
        if (!propertyReference.CheckIfMortgageAllowed()) //if not allowed:
        {
            //ERROR MSG "������ �������� ������ ��� ���� ������"
            return;
        }
        if(nodeReference.IsMortgaged)
        //ERROR ��������� �� ������ "��� ��������" (�� ������, ������ ��� ������ ������ ���������� ����� ���"
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
            return;
        }
        if(playerReference.ReadMoney < nodeReference.MortgagedValue)
        {
            //ERROR "��� ����� ����� �������� ��������"
            return;
        }
        playerReference.PayMoney(nodeReference.MortgagedValue);
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
