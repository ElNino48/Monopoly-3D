using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ������� ��������")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject propertyUIPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text rentPriceText;//0 �������
    [SerializeField] TMP_Text oneHouseRentPriceText;//1 ����� 
    [SerializeField] TMP_Text twoHouseRentPriceText;//2 ������
    [SerializeField] TMP_Text threeHouseRentPriceText;//3 ������
    [SerializeField] TMP_Text fourRentPriceText;//4 ������
    [SerializeField] TMP_Text hotelRentPriceText;//�����
    [Space]
    [SerializeField] TMP_Text housePriceText;//������ �� 1 ���
    [SerializeField] TMP_Text mortgagePriceText;//������ �� �����
    [Space]
    [SerializeField] Button buyPropertyButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;//��������� ������������
    [SerializeField] TMP_Text playerMoneyText;//����� ����� (DESIGN) ���� �����������

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "����: " + currentCash + "BYN";
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
        nodeReference = node;//��� ��������
        playerReference = currentPlayer;//��� ��������

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

        //��������� ��������� 
        housePriceText.text = node.houseCost + "BYN";//��������� ����� � ������ ���������� DESIGN
        mortgagePriceText.text = node.MortgageValue + "BYN";//��������� ������
       
        //BOTTOM PANEL CONTENT:
        propertyPriceText.text = "���������� �� <color=green>" + node.Price + "BYN";
        playerMoneyText.text = "����: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton ��������� ������
        if (currentPlayer.CanAffordNode(node.Price))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            propertyPriceText.text = "���������� �� <color=red>" + node.Price + "BYN";
            buyPropertyButton.interactable = false;
        }
        //������� ����������� �������, ����� ��������� ������:
        propertyUIPanel.SetActive(true);
    }

    public void BuyPropertyButton()//���������� ������� "���������� �� ???BYN"
    {
        //��������� � ��������, ��� ��� ������������� �������
        playerReference.BuyProperty(nodeReference);
        //�� ������� ��������

        //��� ������� ������ ����������(����� �� ������ 29999 ��� �����������)
        buyPropertyButton.interactable = false;
    }
    public void CloseBuyPropertyButton()//���������� ������� "���������� �� ???BYN"
    {
        //������� ������ UI
        propertyUIPanel.SetActive(false);
        //NodeReference � PlayerReference ����� �������� - ������ � ��������� ���������, ����� �������
        nodeReference = null;
        playerReference = null;
    }
}
