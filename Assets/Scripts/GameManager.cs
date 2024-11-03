using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    //[SerializeField] TMP_Text winnerName;

    [Header("Dice")]
    [SerializeField] Dice physicalDice1;
    [SerializeField] Dice physicalDice2;
    //������ ������

    List<int> rolledDice = new List<int>();
    bool isDoubleRolled;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;
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
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
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
        currentPlayer = Random.Range(0, playerList.Count);//��������� ����� �������� ����.
        gameOverPanel.SetActive(false);
        Initialize(); 
        CameraSwitcher.instance.SwitchToTopDown();
        StartCoroutine(StartGame());
        OnUpdateMessage.Invoke("�� �������� <b><color=black>�����!");
    }
    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3f);
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDices();
        }
        else
        {
            //UI ��� ������ ������
            OnShowHumanPanel.Invoke(true, true, false, false, false);
        }
    }
    private void Initialize()
    {
        if (GameSettings.settingsList.Count == 0) 
        {
            //Debug.LogError("����� ���� ������ �� �������� ����!");
            return;
        }
        //�������� �������(����������� �� ������ ����)
        foreach (var setting in GameSettings.settingsList)
        {
            Player player = new Player();
            player.nickname = setting.playerName;
            player.playerType = (Player.PlayerType)setting.selectedType;
            playerList.Add(player);

            GameObject infoObject = Instantiate(playerInfoPrefab, playerUI, false);//false - Local
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            GameObject newToken = Instantiate(playerTokenList[setting.selectedColor],
                gameBoard.route[0].transform.position, Quaternion.identity);
            player.Initialize(gameBoard.route[0], startMoney, info, newToken);
        }

        //for (int i = 0; i < playerList.Count; i++)
        //{
        //    GameObject infoObject = Instantiate(playerInfoPrefab, playerUI, false);//false - Local
        //    PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

        //    //��������� �����:
        //    int randomIndex = Random.Range(0, playerTokenList.Count);
        //    GameObject newToken = Instantiate(playerTokenList[randomIndex], gameBoard.route[0].transform.position, Quaternion.identity);

        //    playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        //}


        playerList[currentPlayer].ActivateArrowSelector(true);

        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jailFreeChanceCard = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jailFreeCommunityCard = playerList[currentPlayer].HasCommunityJailFreeCard;
            //OnUpdateBottomBalance.Invoke(startMoney);
            OnShowHumanPanel.Invoke(true, true, false, jailFreeChanceCard, jailFreeCommunityCard);
        }
        else
        {
            bool jailFreeChanceCard = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jailFreeCommunityCard = playerList[currentPlayer].HasCommunityJailFreeCard;
            //OnUpdateBottomBalance.Invoke(startMoney);
            ManageUI.instance.GetBottomTextPanel.gameObject.SetActive(false);
            OnShowHumanPanel.Invoke(false, false, false, jailFreeChanceCard, jailFreeCommunityCard);
        }
    }

    //��������� ���������� ���� � �������
    public void RollPhysicalDices()
    {
        //if (!hasRolledDice)
        //{
        CheckForGameOver();
        CheckForJailFreeCards();
            rolledDice.Clear();
            physicalDice1.RollPhysicalDice();
            physicalDice2.RollPhysicalDice();
            hasRolledDice = true;
            CameraSwitcher.instance.SwitchToDice();
            
            //��������/������ HUD/UI
            if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            {
                //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
                bool jailFreeChanceCard = playerList[currentPlayer].HasChanceJailFreeCard;
                bool jailFreeCommunityCard = playerList[currentPlayer].HasCommunityJailFreeCard;
                OnShowHumanPanel.Invoke(true, false, false, jailFreeChanceCard, jailFreeCommunityCard);
            }
        //}
    }

    void CheckForJailFreeCards()
    {
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceJailFreeCard)
            {
                playerList[currentPlayer].UseChanceJailFreeCard();
            }
            else if (playerList[currentPlayer].HasCommunityJailFreeCard)
            {
                playerList[currentPlayer].UseCommunityJailFreeCard();
            }
        }
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count == 2)
        {
            RollDice();//2D
        }
    }

    private void RollDice()//������� �� ������� ������ ������� ��� �� //rolldice = private + //rollphysical = public
    {
        bool isAllowedToMove = true;
        hasRolledDice = true;
        //��� � �����?
        
        //    //RESET ��������� ����
        //    rolledDice = new int[2];

        ////��������� ����
        //rolledDice[0] = Random.Range(1, 7);
        //rolledDice[1] = Random.Range(1, 7);
        //Debug.Log("��������� ������: " + rolledDice[0] + "+" + rolledDice[1]);

        ////DEBUG
        //if (alwaysDoubleRoll)
        //{
        //    rolledDice[0] = 2;
        //    rolledDice[1] = 2;
        //}

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
            //Debug.Log("������: "+(doubleRollCount+1));
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
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayerFollow(playerList[currentPlayer].MyToken.transform);
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
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer++;
        //Debug.Log(currentPlayer + "= currentPlayer switchplayer");
        //������ �����?
        doubleRollCount = 0;
        //�������� ������:
        hasRolledDice = false;
        //�������� �� OVERFLOW
        if (currentPlayer>=playerList.Count)
        {
            currentPlayer = 0;
        }

        DeactivateArrows();
        playerList[currentPlayer].ActivateArrowSelector(true);

        ManageUI.instance.GetBottomTextPanel.gameObject.SetActive(false);
        //� �����?
        //����� - ��?
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            //Debug.Log(playerList[currentPlayer].nickname + " = nickname");
            //Debug.Log(playerList[currentPlayer].playerType + " = playerType");
            //Debug.Log("SWITCH PLAYER ROLL:");
            RollPhysicalDices();
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(false, false, false,false, false);//deactivate panel ����� ��� AI 
        }
        else
        {
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            ManageUI.instance.UpdateBottomMoneyText(playerList[currentPlayer].ReadMoney);
            ManageUI.instance.GetBottomTextPanel.gameObject.SetActive(true);
            bool jailFreeChanceCard = playerList[currentPlayer].HasChanceJailFreeCard;
            bool jailFreeCommunityCard = playerList[currentPlayer].HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jailFreeChanceCard,jailFreeCommunityCard);//panel+ roll+ end-
        }
    }

    //���:
    public List<int> LastRolledDice() => rolledDice;
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
            //Debug.Log("���������� - " + playerList[0].nickname);
            OnUpdateMessage("���������� - " + playerList[0].nickname);
            //���������� ����
            gameOverPanel.SetActive(true);
            //winnerName.text = playerList[0].nickname +" - ����������. (L.)";
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

    //----------------------���������� ���� CONTINUE() �������-------------(����� invoke � playere �� �� �� monobehaviour)
    public void Continue()
    {
        if (playerList.Count > 1)//fix#166-2+ (164) �������� ����� ����� ���� ������� �������������� ������, �� ���� ������������
        {
            SwitchPlayer();
        }
        Invoke("ContinueGame", SecondsBetweenTurns);
    }
    public void ContinueGame()
    {
        //���� ��������� ������ ����� -
        if (RolledADouble || !hasRolledDice)
        {
            //-������ �����
            //RollDice();
            //Debug.Log("ContinueGame Roll");
            RollPhysicalDices();
            hasRolledDice = true;
        }
        else//���� ��� - ������� ����
        {
            SwitchPlayer();
        }
    }

    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
    }

    public void UseJailFreeChanceCard()
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }
    public void UseJailFreeCommunityCard()
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }
}

