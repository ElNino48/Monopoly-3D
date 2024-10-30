using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel;//������/��������
    [SerializeField] Transform propertyUIGrid;//������������ ��������-�����
    [SerializeField] GameObject propertySetPrefab;//UI, ������� ����������� � propertyUIGrid ��� ����� 0-3 ��������
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText, yourMoneyTextBottomPanel;
    [SerializeField] TMP_Text systemMessageText;
    [SerializeField] Button autoHandleFundsButton;
    [SerializeField] Button bankruptButton;

    public TMP_Text GetBottomTextPanel => yourMoneyTextBottomPanel;

    private void Awake()
    {
        instance = this;
    }

    //��������� ��
    private void Start()
    {
        autoHandleFundsButton.interactable = false;
        bankruptButton.interactable = true;
        managePanel.SetActive(false);
    }

    public void OpenManagmentPanel()//��������� � ������ "����������"
    {
        yourMoneyTextBottomPanel.gameObject.SetActive(true);
        playerReference = GameManager.instance.GetCurrentPlayer;
        CreateProperties();
        
        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        ClearProperties();
    }//FIX �19+

    void ClearProperties()
    {
        //������� ���� �������� � ����������� ����� �� ��������� ���������
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)//FIX+ �������� ���� � i++/ ���� i--
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }//FIX �19+

    void CreateProperties()
    {
        List<MonopolyNode> processedSet = null;

        //PLAN=
        //��������: �������� = playerReference (current)
        //��������� PROPERTY SETS � ������� �� ���

        //����������� foreach ��� � � Player CheckIfPlayerHasASet � �.�. 
        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //���� List<MonopolyNode> nodeSets = list;
            //FIX ����� ����������� nodeSets, MonopolyBoard ������ ����� � ���������� � ��������� ���� � ������
            List<MonopolyNode> nodeSets = new List<MonopolyNode>();
            nodeSets.AddRange(list);//������ ��� �����

            if (nodeSets != null && list != processedSet)
            {

                //������ - ������� ��� �� ����� ����� OWNED ������ �� ��� OWNED ������� PLAYER'�� 
                //������� - �������� processedSet
                processedSet = list;
                nodeSets.RemoveAll(node => node.Owner != playerReference);//node => node.Owner ���� ������

                //������� ������ �� ����� ���������� ������� � ������� � ������(current)

                //propertySETprefab!!! *�����*: +
                // + Instantiate as GameObject � �� Object ����� ������� ���� (������ �� �������� �����)!!!
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyUIGrid, false);
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSets, playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? ""+playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        if (playerReference.ReadMoney < 0)
        {
            autoHandleFundsButton.interactable = true;
            bankruptButton.interactable = true;
        }
        else
        {
            autoHandleFundsButton.interactable = false;
            //bankruptButton.interactable = false;
        }
        yourMoneyText.text = "����: " + showMoney + "BYN";
        yourMoneyTextBottomPanel.text = "����: " + showMoney + "BYN";
    }
    
    public void UpdateBottomMoneyText(int money)
    {
        //string showMoney = (playerReference.ReadMoney >= 0) ? "" + playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyTextBottomPanel.text = "����: " + money + "BYN";
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }

    public void AutoHandleFunds()//���������� ������� "���������."
    {
        if (playerReference.ReadMoney > 0)
        {
            string message = "<br><br><br><color=red> ������ �� ���������. �� ����� ������������� ������.</color>";
            UpdateSystemMessage(message);
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));//���� �� ������ ��������� ����� � ������� 
        //�������� UI
        ClearProperties();
        CreateProperties();
        UpdateMoneyText();
        //�������� SYSTEM MSG

    }
}
