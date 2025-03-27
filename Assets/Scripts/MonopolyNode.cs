using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    Community,
    Go,
    Jail,
    FreeParking,
    GoToJail
}

public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType type;
    public Image propertyColorField;

    [Header("Property Name")] 
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;

    [Header("Property Price")]
    public int price;
    public int houseCost;
    [SerializeField] TMP_Text priceText;

    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();

    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;
    [SerializeField] GameObject[] houses;
    [SerializeField] GameObject hotel;

    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;//�����
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    //MESSAGE SYSTEM - ������� ���������� ����
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //�������� ����� COMMUNITY
    public delegate void DrawCommunityCard(Player player);
    public static DrawCommunityCard OnDrawCommunityCard;

    //�������� ����� CHANCE
    public delegate void DrawChanceCard(Player player);
    public static DrawChanceCard OnDrawChanceCard;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//������ ��� ����� �� ���-�� ������ - ���������� Invoke Human Panel 

    //BUY PROPERTY PANEL
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;
    
    //BUY RAILROAD PANEL //��
    public delegate void ShowRailroadBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowRailroadBuyPanel;

    //BUY UTILITY PANEL 
    public delegate void ShowUtilityBuyPanel(MonopolyNode node, Player player);
    public static ShowUtilityBuyPanel OnShowUtilityBuyPanel;

    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdated();
    }

    public void SetMortgaged(bool _isMortgaged)
    {
        isMortgaged = _isMortgaged;
    }//���������� FIX mortgaging �18

    public void OnValidate()
    {
        if(nameText != null)
        {
            nameText.text = name;
        }

        //����������
        if(calculateRentAuto)
        {
            if(type == MonopolyNodeType.Property)
            {
                if(baseRent>0)
                {
                    price = 3 * (baseRent * 10);
                    //���� ������
                    mortgageValue = price / 2;

                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5 * 3);
                    rentWithHouses.Add(baseRent * 5 * 9);
                    rentWithHouses.Add(baseRent * 5 * 16);
                    rentWithHouses.Add(baseRent * 5 * 25);

                    
                }
                else if (baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                }
            }
            if(type == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }

            if (type == MonopolyNodeType.Railroad)
            {
                mortgageValue = price / 2;
            }
        }
        if (priceText != null)
        {
            priceText.text = price + " BYN";
        }
        //���������� ���������
        OnOwnerUpdated();
        UnMortgageProperty();
        isMortgaged = false;
    }

    public void UpdateColorField(Color color)
    {
        if (propertyColorField != null)
        {
            propertyColorField.color = color;
        }
    }

    //MORTGAGE (�����)==================== � �� � �� ����====================
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(true);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(false);
        }

        if (propertyImage != null)
        {
            propertyImage.SetActive(true);
        }
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgagedValue => mortgageValue;

    //���������� ���������

    public void OnOwnerUpdated() //���������� ���������
    {
        if(ownerBar != null)
        {
            if (owner != null)
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.nickname;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "???";
            }
        }
    }

    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool isPlayerHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;
        //�������� �� ��� �������� � ��������
        //Debug.Log("Player Landed on node.");
        switch (type)
        {
            case MonopolyNodeType.Property:
                if (!isPlayerHuman)//AI
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //������ �����
                        //Debug.Log("����� ��������� �����. �������� - " + owner.nickname);
                        int rentToPay = CalculatePropertyRent();
                        
                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);
                    }
                    else if( owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI �������� ��������
                        //Debug.Log("�� ������ ����������");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ����������� " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        //OnOwnerUpdated();

                        //�����������
                    }
                    else
                    {
                        //���� �� ������ + ����� ���������

                    }
                }
                else//HUMAN
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if (owner!= null && owner != currentPlayer && !isMortgaged)
                    {

                        //������ �����
                        //Debug.Log("����� ��������� �����. �������� - " + owner.nickname);
                        int rentToPay = CalculatePropertyRent();

                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI ��� ������� � ����������
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //���� �� ������ + �� ����� ���������
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                }
                break;

            case MonopolyNodeType.Utility:
                if (!isPlayerHuman)//AI
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //������ �����
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI �������� ��������
                        //Debug.Log("�� ������ ����������");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ����������� " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //�����������
                    }
                    else
                    {
                        //���� �� ������ + �� ����� ���������

                    }
                }
                else//HUMAN
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //������ �����
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI ��� ������� � ����������
                        OnShowUtilityBuyPanel.Invoke(this,currentPlayer);
                        //�����������
                    }
                    else
                    {
                        //���� �� ������ + ����� ���������

                    }
                }
                break;

            case MonopolyNodeType.Railroad:
                if (!isPlayerHuman)//AI
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //������ �����
                        //Debug.Log("����� ��������� �����. �������� - " + owner.nickname);
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI �������� ��������
                        //Debug.Log("�� ������ ����������");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ����������� " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //�����������
                    }
                    else
                    {
                        //���� �� ������ + ����� ���������

                    }
                }
                else//HUMAN
                {
                    //���� ������ + �� ���� + �� �������� - ����� ����� ����-��
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //������ �����
                        //Debug.Log("����� ��������� �����. �������� - " + owner.nickname);
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //������ �����
                        currentPlayer.PayRent(rentToPay, owner);

                        //����������� � ���������� (��� ���� �������)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " ������ " + rentToPay + " � ������� ����� ������ " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI ��� ������� � ����������
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                        //�����������
                    }
                    else
                    {
                        //���� �� ������ + �� ����� ���������

                    }
                }
                break;

            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //�������� ����.!!
                OnUpdateMessage.Invoke(currentPlayer.nickname + " ���������� ����� � ������� <b><color=red>" + price+
                    "BYN</b></color>");//DESIGN + BYN
                break;

            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                //�������� ����.!!
                OnUpdateMessage.Invoke(currentPlayer.nickname + " ���������� ���������� � �������� <b><color=green>" + tax +
                    "BYN</b></color>");//DESIGN + BYN
                break;

            case MonopolyNodeType.GoToJail:
                //������ ���� "��� � �����" - 30 �� 21.10.24 13:20 ========== id 30 
                //��� ����� �������T�
                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                OnUpdateMessage.Invoke(currentPlayer.nickname +
                    " <b><color=red>��������</b></color> �� ��������� �������������.");//DESIGN
                currentPlayer.GoToJail(indexOnBoard);
                continueTurn = false;
                break;

            case MonopolyNodeType.Chance:

                Debug.Log("Player Landed on chance node.");
                OnDrawChanceCard.Invoke(currentPlayer);
                continueTurn = false;
                break;

            case MonopolyNodeType.Community:
                Debug.Log("Player Landed on community node.");
                OnDrawCommunityCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }

        //���� ����� ���� ��� (��������� �����)
        if(!continueTurn)
        {
            return;
        }
        //�����������
        if(!isPlayerHuman)
        {
            //Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
            currentPlayer.ChangeState(Player.AIStates.TRADING);
        }
        else
        {
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            //HUD/UI ��� ������ + �������� �� �����
            bool jailFreeChanceCard = currentPlayer.HasChanceJailFreeCard;
            bool jailFreeCommunityCard = currentPlayer.HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, jailFreeChanceCard, jailFreeCommunityCard);
        }
    }

    //void ContinueGame()
    //{
    //    //���� ��������� ������ ����� -
    //    if (GameManager.instance.RolledADouble)
    //    {
    //        //-������ �����
    //        GameManager.instance.RollDice();
    //    }
    //    else//���� ��� - ������� ����
    //    {
                        
    //        GameManager.instance.SwitchPlayer();
    //    }

    //}

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0://��� �������

                //��������, ����� �� OWNER ������ ��� �������� ������ �����!
                var(list, areAllSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

                if (areAllSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }

                break;

            case 1://1 �����
                currentRent = rentWithHouses[0];//0+1=1
                break;

            case 2://2 ������
                currentRent = rentWithHouses[1];//1+1=2
                break;

            case 3://3 ������
                currentRent = rentWithHouses[2];//2+1=3
                break;

            case 4://4 ������
                currentRent = rentWithHouses[3];//3+1=4
                break;

            case 5://�����
                currentRent = rentWithHouses[4];//4+1=5
                break;
        }

        return currentRent;
    }

    int CalculateUtilityRent()
    {
        List<int> lastRolledDice = GameManager.instance.LastRolledDice();

        int result = 0;
        var (list, areAllSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);
        if (areAllSame)
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;//���� ������� ���� � ������ � ������� ��� ������� �� �����: (����1+����2)*10
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;//���� �������: (����1+����2)*4
        }

        return result;
    }

    int CalculateRailroadRent()
    {
        int result = 0;
        var (list, areAllSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

        //���������� ��:

        int amount = 0;
        foreach (var item in list)
        {
            /* 
             * if (item.owner == this.owner)
                 amount++; //�� �� �����: amount += (item.owner == this.owner)?1:0;
            */
            amount += (item.owner == this.owner)?1:0;
        }
        
        result = baseRent * (int)Mathf.Pow(2, amount-1);

        return result;
    }

    void VisualiseHouses()
    {
        switch (numberOfHouses)
        {
            case 0://��� �������
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1://1 �����
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2://2 ������
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3://3 ������
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4://4 ������
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5://�����
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(true);
                break;
        }
    }

    public void BuildHouseOrHotel()
    {
        if (type == MonopolyNodeType.Property)
        {
            numberOfHouses++;
            VisualiseHouses();
        }
    }

    public int SellHouseOrHotel()
    {
        if (numberOfHouses > 0 && type == MonopolyNodeType.Property && numberOfHouses > 0)
        {
            numberOfHouses--;
            VisualiseHouses();
            return houseCost / 2;//������� ��� �������
        }
        return 0;
    }

    public void ResetNode()
    {
        if (isMortgaged)//���� ��������
        {
            propertyImage.SetActive(true);
            mortgageImage.SetActive(false);
            isMortgaged = false;
        }
        //RESET ������ � ����� �� ��������
        if (type == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualiseHouses();
        }
        //RESET OWNER

        //RESET PROPERTY �� OWNER
        owner.RemoveProperty(this);
        owner.ActivateArrowSelector(false);//DESIGN ����� ������� ��� �������� - �������
        owner = null;
        //UPDATE UI/HUD
        OnOwnerUpdated();
    }

    //==============��������===============

    //------------�������� ��������� ��������--------------
    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        SetOwner(newOwner);
    }
}
