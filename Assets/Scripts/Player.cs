using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Player
{
    public enum PlayerType 
    {
        HUMAN,
        AI
    }

    //HUMAN
    public PlayerType playerType;
    public string nickname;
    int money;
    MonopolyNode currentnode;
    bool isInJail;
    int numTurnsInJail;
    [SerializeField] GameObject myToken;
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard;
    public bool HasChanceJailFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityJailFreeCard => hasCommunityJailFreeCard;

    //---------------------------�������������� � �������� -----------------------------------
    //SKILLS
    public List<Skill> Skills { get; } = new List<Skill>();
    public float RentBonus { get; set; }

    //PLAYER INFO
    PlayerInfo myInfo;
    
    //UI Show Panel
    UIShowProperty uiShowPropertyInfo;

    //MESSAGE SYSTEM - ������� ���������� ����
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//������ ��� ����� �� ���-�� ������ - ���������� Invoke Human Panel 

    //AI
    int aiMoneySavity = 200;

    //AI ���������
    public enum AIStates
    {
        IDLE,
        TRADING
    }

    public AIStates aiState;

    //return ��������� �������� � ����������
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode;
    public int ReadMoney => money;

    public void Initialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode;
        money = startMoney;
        myInfo = info;

        UIShowPanel.instance.UpdateBalance(money);
        myInfo.SetPlayerNameAndCash(nickname, money);
        myToken = token;
        myInfo.ActivateArrow(false);
    }

    public void SetNewNode(MonopolyNode newNode)
    {
        
        currentnode = newNode;
        //�������� ��:
        //����� �������� �� �����, ��� ����� ������ �������� �������� 
        newNode.PlayerLandedOnNode(this);

            //���� ��� ��:
            if(playerType == PlayerType.AI)
        {
            //����� ������� ������?
            CheckIfPlayerHasASet();
            //���� � �� ���� ���������� ��������?
            UnMortgageProperties();
            //����� �� ���������, ����� �������� ����������� �������� (����� ��� ������ ����� ����� ����� ������������ � ������� ������)
            //TradingSystem.instance.FindMissingProperty(this);
        }
        
    }

    public void CollectMoney(int amount)
    {
        money += amount;

        UIShowPanel.instance.UpdateBalance(money);
       
        myInfo.SetPlayerCash(money);
        //�������� (��� � � MonopolyNode):
        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            //HUD/UI ��� ������ + �������� �� �����
            ManageUI.instance.UpdateBottomMoneyText(money);

            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }

    internal bool CanAffordNode(int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.SetOwner(this);
        //���������� �ר�� �������
        //Debug.Log(money + "= money 1TYT");
        ManageUI.instance.UpdateBottomMoneyText(money);
        myInfo.SetPlayerCash(money);
        //uiShowPropertyInfo.SetPlayerMoneyBalance(money);
        //���������� ��������
        myMonopolyNodes.Add(node);

        //���������� �������� �� ����
        SortPropertiesByPrice();
    }
    void SortPropertiesByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        float oldRentAmount = rentAmount;
        rentAmount = (int)(rentAmount * (1f + owner.RentBonus));
        if(oldRentAmount != rentAmount)
        {
            Debug.Log($"oldRentAmount = {oldRentAmount}; rentBonus = {owner.RentBonus}; newRentAmount = {rentAmount}");
        }

        //(RENT) ���� ��� ���������� ����� - ������ ������� � ������ �����:
        if (money < rentAmount)
        {
            if (playerType == PlayerType.AI)
            {
                //AI
                HandleInsufficientFunds(rentAmount);
            }
            else
            {
                //��������� ����������� ��������� ��� ��� ������� ����
                OnShowHumanPanel.Invoke(true, false, false, hasChanceJailFreeCard, hasCommunityJailFreeCard);
            }
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        //���������� UI ��� ������ � ��������� � ������

        UIShowPanel.instance.UpdateBalance(money);
        myInfo.SetPlayerCash(money);
    }
   
    internal void PayMoney(int taxAmount)
    {
        //(TAX) ���� ��� ���������� ����� - ������ ������� � ������ �����:
        if (money < taxAmount)
        {
            if (playerType == PlayerType.AI)
            {
                //AI
                HandleInsufficientFunds(taxAmount);
            }
            //else
            //{
            //    //��������� ����������� ��������� ��� ��� ������� ����
            //    OnShowHumanPanel.Invoke(true, false, false);
            //}
        }
        money -= taxAmount;
        //���������� UI ��� ������ � ��������� � ������

        UIShowPanel.instance.UpdateBalance(money);
        ManageUI.instance.UpdateBottomMoneyText(money);
        myInfo.SetPlayerCash(money);

        //�������� (��� � � MonopolyNode):
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);
            //HUD/UI ��� ������ + �������� �� �����
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasChanceJailFreeCard, hasCommunityJailFreeCard);//
        }
    }

    //-----------------------------------------------======--------------------------------------------------------------------------------------
    //-----------------------------------------------=JAIL=--------------------------------------------------------------------------------------
    //-----------------------------------------------======--------------------------------------------------------------------------------------
   
    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        //����������� ������:
        //myToken.transform.position = MonopolyBoard.instance.route[10].transform.position;
        //currentnode = MonopolyBoard.instance.route[10];

        MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);//���� ����� ������ 20 ������ ����� � �����
        GameManager.instance.ResetRolledADouble();//�������� �������� �� ����� ��������� ��� ������ � ������ � �� ����� ������ ��� ���
    }

    public void SetFreeOfJail()
    {
        isInJail = false;
        ResetNumTurnsInJail();
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;//������ ���� "�����" �� ����� - 10 �� 21.10.24 13:16
        if (indexOnBoard > indexOfJail)
        {
            result = -(indexOnBoard - indexOfJail);// - ������������� �������� ���� �� ������� ����� �����
        }
        else
        {
            result = (indexOfJail - indexOnBoard);// + ��� 0 ���� ����� ��� �� ���� �����
        }
        return result;
    }

    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseNumTurnsInJail()
    {
        numTurnsInJail++;
    }
    void ResetNumTurnsInJail()
    {
        numTurnsInJail = 0;
    }

    //---------------------------------------STREET-REPAIRS----------------------------------
    //---------------------------------------������ � �����----------------------------------
    public int[] CountHousesAndHotels()
    {
        int houses = 0;//INDEX 0
        int hotels = 0;//INDEX 1

        foreach (var node in myMonopolyNodes)
        {
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;//����� ����� ������� ������ ����� �� ��� 4, ����� - 5
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    public void HandleInsufficientFunds(int amountToPay)
    {
        //����� �������� �������:
        int housesToSell = 0;//��������� ������ �� �������
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        //����ר� ���� �������
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        //������ ������ �� ��������� � ���������� ������� ������� ������� �����
        while (money<amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    //����� ������ �����?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        //������:

        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (node.IsMortgaged) ? 0 : 1;// ��� (!node.IsMortgaged) ? 1 : 0;
        }
        //������ ������ �� ��������� � ���������� �������� ������� ������� �����
        while(money<amountToPay && propertiesToMortgage >0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (node.IsMortgaged) ? 0 : 1;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortgageProperty());//������ DESIGN
                    allPropertiesToMortgage--;
                    //����� ������ �����?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        //������� ���� ����� ����:
        if (playerType == PlayerType.AI)//fix166-3+
                                        //������ ������ �� ����� ������������ ��������� ������� "����������������� ��������"
        {
            Bankrupt();
        }
    }

    internal void Bankrupt()
    {
        //������ ������ �� ����

        //�����������
        OnUpdateMessage.Invoke(nickname + " <b><color=red>������������</b></color>.");//TMP (text mesh pro) HELP FILES DESIGN 

        //�������� �Ѩ ��� ���� ������ �������
        for (int i = myMonopolyNodes.Count-1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }

        if (hasChanceJailFreeCard)//fix#166+ ����� �� ����������� � ������ ����� ���� ��� �����(� ��) ������� "�������"
        {
            ChanceField.instance.AddBackJailFreeCard();
        }
        if (hasCommunityJailFreeCard)//fix#166+ ����� �� ����������� � ������ ����� ���� ��� �����(� ��) ������� "�������"
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }
        //������ ����� �������� � ������ ����� ���� ��� ���-�� ������ ���������.

        //������� ������
        GameManager.instance.RemovePlayer(this);
        
    }


    void UnMortgageProperties()//��� AI �� ������ ������ 22.10 20:55
    {
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                int cost = node.MortgagedValue + (int)(node.MortgagedValue * 0.1f);//���� ������ �� ������, ������� (10%�������)
                //����� �� ��������� �������� �� ������?
                if (money >= aiMoneySavity + cost)
                {
                    PayMoney(cost);
                    node.UnMortgageProperty();
                }
            }
        }
    }
    public void CheckIfPlayerHasASet()
    {
        List<MonopolyNode> processedSet = null;// == List<MonopolyNode> processedSet = new List<MonopolyNode>;
        foreach (var node in myMonopolyNodes)
        {
            var (list, areAllSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            if(!areAllSame)
            {
                continue;
            }
            List<MonopolyNode> nodeSets = list;//������ 1 ����� �� ���
            if(nodeSets != null && nodeSets != processedSet)
            {
                bool hasMortgagedNode = nodeSets.Any(node => node.IsMortgaged)?true:false;//����� �� ���������� ��������
                if (!hasMortgagedNode)
                {
                    if(nodeSets[0].type == MonopolyNodeType.Property)//����� ��������� ��� ���
                    {
                        BuildHousesOrHotelEvenly(nodeSets);
                        //���������� 
                        processedSet = nodeSets;
                    }
                }
            }
        }
    }
    
    internal void BuildHousesOrHotelEvenly(List<MonopolyNode> nodesToBuidOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;
        //��������� ��� � ���� ���������� ������� ������ �� �������� �� ��������
        foreach (var node in nodesToBuidOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if(numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }
            if (numOfHouses > maxHouses && numOfHouses < 5) 
            {
                maxHouses = numOfHouses;
            }
        }
        foreach (var node in nodesToBuidOn)
        {
            //������ � ������ ���� �����
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses< 5 && CanAffordHouse(node.houseCost)){
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                //��������� ����� ���� ����� ��������� ������ ���� �����
                break;
            }
        }
    }

    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool isHouseSold = false;//FIX ������, ����� �� ����������� ������
        foreach (var node in nodesToSellFrom)
        {//������� ������� ���������� � ��������, �� ������� ���� ������� ������ �����
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses);
            break;
        }
        //������� SELL HOUSE ���
        for (int i = nodesToSellFrom.Count - 1; i >= 0; i--)
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)
            {//��� � Player'e(class Player)
                CollectMoney(nodesToSellFrom[i].SellHouseOrHotel());
                isHouseSold = true;
                break;
            }
        }
        if (!isHouseSold)
        {
            CollectMoney(nodesToSellFrom[nodesToSellFrom.Count-1].SellHouseOrHotel());
        }
    }

    internal bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)//AI
        {

            return (money - aiMoneySavity) >= price;
        }
        return money >= price;//HUMAN
    }

    //-----------------SELECTOR ������� ---------------------
    public void ActivateArrowSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }

    //TRADING SYSTEM ��������


    //�������� ���� � ������ ������ �������� �� ��������
    public void AddProperty(MonopolyNode node)
    {
        myMonopolyNodes.Add(node);
        SortPropertiesByPrice();
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
        SortPropertiesByPrice();
    }

    //------��������� ��������-------
    public void ChangeState(AIStates state)
    {
        if(playerType == PlayerType.HUMAN)
        {
            return;
        }

        aiState = state;
        switch (aiState)
        {
            case AIStates.IDLE:
            {
                //����������
                GameManager.instance.ContinueGame();
                }
            break;

            case AIStates.TRADING:
            {
                //����� ���� �� ���������� ������
                //Debug.Log("TRADING status");
                TradingSystem.instance.FindMissingProperty(this);
            }
            break;
        }
    }
    
    public void AddChanceJailFreeCard()
    {
        hasChanceJailFreeCard = true;
    }
    public void AddCommunityJailFreeCard()
    {
        hasCommunityJailFreeCard = true;
    }
    public void UseChanceJailFreeCard()//jailFreeChanceCard
    {
        if (!isInJail)
        {
            return;
        }
        SetFreeOfJail();
        hasChanceJailFreeCard = false;
        ChanceField.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(nickname + " �������� ��������� �������� �������� � ��� <color=green>����������</color> ��-��� ������.");
    }
    public void UseCommunityJailFreeCard()//jailFreeCommunityCard
    {
        if (!isInJail)
        {
            return;
        }
        SetFreeOfJail();
        hasCommunityJailFreeCard = false;
        CommunityChest.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(nickname + "������������ ������� ��� <color=green>����������</color> ��-��� ������. (L.)");
    }
}
