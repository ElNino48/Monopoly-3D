using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;

    [Header("General Settings")]

    [SerializeField] int maxTurnsInJail = 3;//��������� ����� �������� �� �����
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;
    [SerializeField] float secondsBetweenTurns = 3;

    [Header("Player Info")]

    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerUI;//��� ���� ����� playerInfoPrefab'� ����� ���������� 
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();
    
    //������ ������

    int[] rolledDice;
    bool isDoubleRolled;
    public bool RolledADouble => isDoubleRolled;
    public void ResetRolledADouble() => isDoubleRolled = false;
    int doubleRollCount;
    int taxPool = 0;
    //������ ����� GO - ������ 500
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];//������������ ��� ManageUI

    //MESSAGE SYSTEM - ������� ���������� ����
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;//������ ��� ����� �� ���-�� ������ - ���������� Invoke Human Panel 

    //public delegate void UpdateBottomBalance(int money);
    //public static UpdateBottomBalance OnUpdateBottomBalance;//������ ��� ����� ������� - ����� �������

    //DEBUG
    public bool alwaysDoubleRoll = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Initialize();
        if(playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }
        else
        {
            //UI ��� ������ ������
        }
    }

    private void Initialize()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerUI, false);//false - Local
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            //��������� �����:
            int randomIndex = Random.Range(0, playerTokenList.Count);
            GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);

            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
        playerList[currentPlayer].ActivateArrowSelector(true);

        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            //OnUpdateBottomBalance.Invoke(startMoney);
            OnShowHumanPanel.Invoke(true, true, false);
        }
        else
        {
            //OnUpdateBottomBalance.Invoke(startMoney);
            OnShowHumanPanel.Invoke(false, false, false);
        }
    }

    public void RollDice()//������� �� ������� ������ ������� ��� ��
    {

        bool isAllowedToMove = true;

        //RESET ��������� ����
        rolledDice = new int[2];

        //��������� ����
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);
        Debug.Log("��������� ������: " + rolledDice[0] + "+" + rolledDice[1]);

        //DEBUG
        if (alwaysDoubleRoll)
        {
            rolledDice[0] = 0;
            rolledDice[1] = 1;
        }

        //�������� �� �����
        isDoubleRolled = rolledDice[0] == rolledDice[1];

        //��� � �����?
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();//+��� � �����
            
            if (isDoubleRolled)
            {
                
                OnUpdateMessage.Invoke("����������� " + playerList[currentPlayer].nickname +
                    " �������� <b>����������</b> ����� � ��� <b><color=green>����������</b></color> ��-��� ������.");//DESIGN
                playerList[currentPlayer].SetFreeOfJail();

                doubleRollCount++;
                //������ ����� ������� ������� ���
            }
            else if(playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
               
                //����� ��� ������� 3 ���� (maxTurnsInJail) � ����� ����� �� �������
                OnUpdateMessage.Invoke("���� ���������� <b>����</b> � " + playerList[currentPlayer].nickname +
                    " ��� <b><color=green>����������</b></color> ��-��� ������.");//DESIGN
                playerList[currentPlayer].SetFreeOfJail();
            }
            else
            {
                isAllowedToMove = false;
            }
        }
        else//�� � ����� ���� ���
        //������� 3 - � �����
        {
            Debug.Log("������: "+(doubleRollCount+1));
            //��������� �������� ������
            if (!isDoubleRolled)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if (doubleRollCount>=3)
                {
                    //� ����� �� 3 ������ �����
                    
                    //��������� ������� ���� ��� ������ �����
                 
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage.Invoke(playerList[currentPlayer].nickname + 
                        " ��� <b><color=red>���������</b></color> �� <b><color=red>�������������</b></color>.");
                    //HTML ������� ����� ������������� ��������� ����! //DESIGN
                    isDoubleRolled = false;//���������
                    return;
                }
             }
        }


        //������ ������� �� �����

        //������ ��� ������ ���� �����
        if (isAllowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].nickname +" �������� " +
                +rolledDice[0] + " & " + rolledDice[1]+ ".");//DESIGN
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            //������� ����?
            //DEBUG
            OnUpdateMessage.Invoke(playerList[currentPlayer].nickname + " ��������� � ���������� ���.");//DESIGN
            StartCoroutine(DelayBetweenSwitchPlayer());//� ����� ������ �������� ����� � 3 �������
        }

        //��������/������ HUD/UI
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(true, false, false);
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        //���� ����� ������ - �����
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        //�� ����� - ������� ����
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        currentPlayer++;

        //������ �����?
        doubleRollCount = 0;

        //�������� �� OVERFLOW
        if (currentPlayer>=playerList.Count)
        {
            currentPlayer = 0;
        }

        DeactivateArrows();
        playerList[currentPlayer].ActivateArrowSelector(true);

        //� �����?

        //����� - ��?
        if(playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(false, false, false);//deactivate panel ����� ��� AI 
        }
        else
        {
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(true, true, false);//panel+ roll+ end-
        }
    }

    //���:
    public int[] LastRolledDice() => rolledDice;
    //�� �� ����� ��� �: 

    /*    public int[] LastRolledDice()
        {
            return rolledDice;
        }
    ==========================
    public int[] LastRolledDice() => rolledDice;
    */

    public void AddTaxToPool(int amount)
    {
        taxPool += amount;
    }

    public int GetTaxPool()
    {
        //��������� ���������� ���������� ����� ���� �������, ��������� ���, ����� ���������� ������ � ���� FreeParking
        int currentTaxCollected = taxPool;
        taxPool = 0;
        return currentTaxCollected;
    }
    
    //---------------------------------------GAME OVER--------------------------------------
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        //��������� �� �������� (GAME OVER)
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            //���� ����������:
            Debug.Log("���������� - " + playerList[0].nickname);
            OnUpdateMessage("���������� - " + playerList[0].nickname);
            //���������� ����

            //�������� UI //DESIGN
        }
    }

    //-----------------------------------UI------------------------
    void DeactivateArrows()
    {
        foreach (var player in playerList)
        {
            player.ActivateArrowSelector(false);//��������� ��������� ��� "�������� �����"
        }
    }
}

