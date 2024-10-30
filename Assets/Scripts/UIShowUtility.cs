using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UIShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("UI ������� �������� �� RAILROAD")]

    [Header("TITLE BAR")]
    [SerializeField] GameObject utilityUIPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] Image colorField;
    [Space]
    [Header("INSIDE")]
    [SerializeField] TMP_Text mortgagePriceText;//������ �� �����
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;//��������� ������������
    [SerializeField] TMP_Text playerMoneyText;//����� ����� (DESIGN) ���� �����������'

    public void SetPlayerMoneyBalance(int currentCash)
    {
        playerMoneyText.text = "����: " + currentCash + "BYN";
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
        nodeReference = node;//��� ��������
        playerReference = currentPlayer;//��� ��������

        //TOP PANEL CONTENT:
        utilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color

        //��������� ��������� DESIGN
        mortgagePriceText.text = node.MortgagedValue + "BYN";//��������� ������

        //BOTTOM PANEL CONTENT:
        utilityPriceText.text = "���������� �� <color=green>" + node.price + "BYN";
        playerMoneyText.text = "����: " + currentPlayer.ReadMoney + "BYN";

        //BuyPropertyButton ��������� ������
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            utilityPriceText.text = "���������� �� <color=red>" + node.price + "BYN";
            buyUtilityButton.interactable = false;
        }
        //������� ����������� �������, ����� ��������� ������:
        utilityUIPanel.SetActive(true);
    }
    public void BuyUtilityButton()//���������� ������� "���������� �� ???BYN"
    {
        //��������� � ��������, ��� ��� ������������� �������
        playerReference.BuyProperty(nodeReference);
        //�� ������� ��������

        //��� ������� ������ ����������(����� �� ������ 29999 ��� �����������)
        buyUtilityButton.interactable = false;
    }
    public void CloseBuyUtilityButton()//���������� ������� "���������� �� ???BYN"
    {
        //������� ������ UI
        utilityUIPanel.SetActive(false);
        //NodeReference � PlayerReference ����� �������� - ������ � ��������� ���������, ����� �������
        nodeReference = null;
        playerReference = null;
    }
}
