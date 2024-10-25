using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ������� �������� �� RAILROAD")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject railroadUIPanel;
    [SerializeField] TMP_Text railroadNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text oneRailroadRentPriceText;//1 ��
    [SerializeField] TMP_Text twoRailroadRentPriceText;//2 ��
    [SerializeField] TMP_Text threeRailroadRentPriceText;//3 ��
    [SerializeField] TMP_Text fourRailroadPriceText;//4 ��
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;//������ �� �����
    [Space]
    [SerializeField] Button buyRailroadButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;//��������� ������������
    [SerializeField] TMP_Text playerMoneyText;//����� ����� (DESIGN) ���� �����������

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "����: " + currentCash + "BYN";
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
        nodeReference = node;//��� ��������
        playerReference = currentPlayer;//��� ��������

        //TOP PANEL CONTENT:
        railroadNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;
        //INSIDE 
        //result = baseRent * (int)Mathf.Pow(2, amount-1);
        oneRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 1 - 1) + "BYN";
        twoRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 2 - 1) + "BYN";
        threeRailroadRentPriceText.text = node.baseRent * (int)Mathf.Pow(2, 3 - 1) + "BYN";
        fourRailroadPriceText.text = node.baseRent * (int)Mathf.Pow(2, 4 - 1) + "BYN";

        //��������� ��������� DESIGN
        mortgagePriceText.text = node.MortgagedValue + "BYN";//��������� ������

        //BOTTOM PANEL CONTENT:
        propertyPriceText.text = "���������� �� <color=green>" + node.price + "BYN";
        playerMoneyText.text = "����: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton ��������� ������
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            propertyPriceText.text = "���������� �� <color=red>" + node.price + "BYN";
            buyRailroadButton.interactable = true;
        }
        //������� ����������� �������, ����� ��������� ������:
        railroadUIPanel.SetActive(true);
    }

    public void BuyRailroadButton()//���������� ������� "���������� �� ???BYN"
    {
        //��������� � ��������, ��� ��� ������������� �������
        playerReference.BuyProperty(nodeReference);
        //�� ������� ��������

        //��� ������� ������ ����������(����� �� ������ 29999 ��� �����������)
        buyRailroadButton.interactable = false;
    }
    public void CloseBuyRailroadButton()//���������� ������� "���������� �� ???BYN"
    {
        //������� ������ UI
        railroadUIPanel.SetActive(false);
        //NodeReference � PlayerReference ����� �������� - ������ � ��������� ���������, ����� �������
        nodeReference = null;
        playerReference = null;
    }
}
