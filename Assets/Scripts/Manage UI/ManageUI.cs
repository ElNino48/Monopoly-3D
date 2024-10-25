using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel;//������/��������
    [SerializeField] Transform propertyUIGrid;//������������ ��������-�����
    [SerializeField] GameObject propertySetPrefab;//UI, ������� ����������� � propertyUIGrid ��� ����� 0-3 ��������
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText, yourMoneyTextBottomPanel;

    private void Awake()
    {
        instance = this;
    }

    //��������� ��
    private void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManagmentPanel()//��������� � ������ "����������"
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
        //�������� ��� �������� � �������� ����� ��������
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
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSets,playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
        //������� ��������� - ����� ������� ������
        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        //������� ���� �������� � ����������� ����� �� ��������� ���������
        for (int i = propertyPrefabs.Count-1; i >=0; i--)//FIX+ �������� ���� � i++/ ���� i--
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }//FIX �19

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? ""+playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyText.text = "����: " + showMoney + "BYN";
        yourMoneyTextBottomPanel.text = "����: " + showMoney + "BYN";
    }
    
    public void UpdateBottomMoneyText(int money)
    {

        Debug.Log(money + "= money 1TYT");
        //string showMoney = (playerReference.ReadMoney >= 0) ? "" + playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyTextBottomPanel.text = "����: " + money + "BYN";
    }

}
