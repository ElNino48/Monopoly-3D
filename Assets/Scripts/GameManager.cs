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

    [SerializeField] int maxTurnsInJail = 3;//НАСТРОЙКИ ЧТОБЫ ЧАЛИТЬСЯ НА НАРАХ
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;
    [SerializeField] float secondsBetweenTurns = 3;

    [Header("Player Info")]

    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerUI;//ДЛЯ ТОГО ЧТОБЫ playerInfoPrefab'ы СТАЛИ РОДИТЕЛЯМИ 
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    //[SerializeField] TMP_Text winnerName;

    [Header("Dice")]
    [SerializeField] Dice physicalDice1;
    [SerializeField] Dice physicalDice2;
    //БРОСОК КОСТЕЙ

    List<int> rolledDice = new List<int>();
    bool isDoubleRolled;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;
    public bool RolledADouble => isDoubleRolled;
    public void ResetRolledADouble() => isDoubleRolled = false;
    int doubleRollCount;
    int taxPool = 0;
    
    //ПРОШЕЛ ЧЕРЕЗ GO - ПОЛУЧИ 500
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];//понадобилось для ManageUI

    //MESSAGE SYSTEM - СИСТЕМА ДИАЛОГОВЫХ ОКОН
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//каждый раз когда мы что-то делаем - происходит Invoke Human Panel 

    //public delegate void UpdateBottomBalance(int money);
    //public static UpdateBottomBalance OnUpdateBottomBalance;//каждый раз когда покупка - смена баланса

    //DEBUG
    public bool alwaysDoubleRoll = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentPlayer = Random.Range(0, playerList.Count);//Рандомный игрок начинает игру.
        gameOverPanel.SetActive(false);
        Initialize(); 
        CameraSwitcher.instance.SwitchToTopDown();
        StartCoroutine(StartGame());
        OnUpdateMessage.Invoke("Да начнется <b><color=black>битва!");
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
            //UI для живого игрока
            OnShowHumanPanel.Invoke(true, true, false, false, false);
        }
    }
    private void Initialize()
    {
        if (GameSettings.settingsList.Count == 0) 
        {
            //Debug.LogError("Старт игры ТОЛЬКО из Главного меню!");
            return;
        }
        //СОЗДАНИЕ ИГРОКОВ(основываясь на данных меню)
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

        //    //Рандомный токен:
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

    //СОЕДИНИТЬ ФИЗИЧЕСКИЙ ДАЙС К СИСТЕМЕ
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
            
            //ПОКАЗАТЬ/СКРЫТЬ HUD/UI
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

    private void RollDice()//РЕАКЦИЯ НА НАЖАТИЕ КНОПКИ ИГРОКОМ ИЛИ ИИ //rolldice = private + //rollphysical = public
    {
        bool isAllowedToMove = true;
        hasRolledDice = true;
        //УЖЕ В ТУРМЕ?
        
        //    //RESET ПОСЛЕДНИЙ РОЛЛ
        //    rolledDice = new int[2];

        ////СОХРАНИТЬ РОЛЛ
        //rolledDice[0] = Random.Range(1, 7);
        //rolledDice[1] = Random.Range(1, 7);
        //Debug.Log("Результат броска: " + rolledDice[0] + "+" + rolledDice[1]);

        ////DEBUG
        //if (alwaysDoubleRoll)
        //{
        //    rolledDice[0] = 2;
        //    rolledDice[1] = 2;
        //}

        //ПРОВЕРКА НА ДУБЛЬ
        isDoubleRolled = rolledDice[0] == rolledDice[1];

        //УЖЕ В ТУРМЕ?
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();//+ход в турме
            
            if (isDoubleRolled)
            {
                
                OnUpdateMessage.Invoke("Заключенный " + playerList[currentPlayer].nickname +
                    " выбросил <b>одинаковые</b> кости и был <b><color=green>освобожден</b></color> из-под стражи.");//DESIGN
                playerList[currentPlayer].SetFreeOfJail();

                doubleRollCount++;
                //ТЕПЕРЬ МОЖНО СДЕЛАТЬ ОБЫЧНЫЙ ХОД
            }
            else if(playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
               
                //ИГРОК УЖЕ ОТСИДЕЛ 3 ХОДА (maxTurnsInJail) И МОЖЕТ ВЫЙТИ НА СВОБОДУ
                OnUpdateMessage.Invoke("Срок заключения <b>истёк</b> и " + playerList[currentPlayer].nickname +
                    " был <b><color=green>освобожден</b></color> из-под стражи.");//DESIGN
                playerList[currentPlayer].SetFreeOfJail();
            }
            else
            {
                isAllowedToMove = false;
            }
        }
        else//НЕ В ТУРМЕ ПОКА ЧТО
        //ВЫКИНУЛ 3 - В ТУРМУ
        {
            //Debug.Log("дублей: "+(doubleRollCount+1));
            //ОБНУЛЕНИЕ СЧЕТЧИКА ДУБЛЕЙ
            if (!isDoubleRolled)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if (doubleRollCount>=3)
                {
                    //В ТУРМУ ЗА 3 ПОДРЯД ДУБЛЯ
                    
                    //ПОЛУЧЕНИЕ ИНДЕКСА ПОЛЯ ГДЕ СЕЙЧАС ИГРОК
                 
                    int indexOnBoard = MonopolyBoard.instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage.Invoke(playerList[currentPlayer].nickname + 
                        " был <b><color=red>арестован</b></color> за <b><color=red>МОШЕННИЧЕСТВО</b></color>.");
                    //HTML нотация можно редактировать текстовые поля! //DESIGN
                    isDoubleRolled = false;//ОБНУЛЕНИЕ
                    return;
                }
             }
        }


        //МОЖЕШЬ СБЕЖАТЬ ИЗ ТУРМЫ

        //ХОДИТЬ КАК ХОЧЕШЬ ЕСЛИ МОЖНО
        if (isAllowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].nickname +" выбросил " +
                +rolledDice[0] + " & " + rolledDice[1]+ ".");//DESIGN
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            //ПЕРЕХОД ХОДА?
            //DEBUG
            OnUpdateMessage.Invoke(playerList[currentPlayer].nickname + " арестован и пропускает ход.");//DESIGN
            StartCoroutine(DelayBetweenSwitchPlayer());//К СМЕНЕ ИГРОКА ДОБАВЛЕН ДИЛЕЙ В 3 СЕКУНДЫ
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayerFollow(playerList[currentPlayer].MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns);
        //ЕСЛИ МОЖЕМ ХОДИТЬ - ХОДИМ
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        //НЕ МОЖЕМ - ПЕРЕХОД ХОДА
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
        //БРОСИЛ ДУБЛЬ?
        doubleRollCount = 0;
        //ОБНУЛИТЬ СТАТУС:
        hasRolledDice = false;
        //ПРОВЕРКА НА OVERFLOW
        if (currentPlayer>=playerList.Count)
        {
            currentPlayer = 0;
        }

        DeactivateArrows();
        playerList[currentPlayer].ActivateArrowSelector(true);

        ManageUI.instance.GetBottomTextPanel.gameObject.SetActive(false);
        //В ТУРМЕ?
        //Игрок - ИИ?
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            //Debug.Log(playerList[currentPlayer].nickname + " = nickname");
            //Debug.Log(playerList[currentPlayer].playerType + " = playerType");
            //Debug.Log("SWITCH PLAYER ROLL:");
            RollPhysicalDices();
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(false, false, false,false, false);//deactivate panel когда ход AI 
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

    //ЭТО:
    public List<int> LastRolledDice() => rolledDice;
    //То же самое что и: 

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
        //Временное сохранение количества денег пула налогов, обнуление его, когда собираются деньги с поля FreeParking
        int currentTaxCollected = taxPool;
        taxPool = 0;
        return currentTaxCollected;
    }
    
    //---------------------------------------GAME OVER--------------------------------------
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        //ПРОВЕРИТЬ НА ПРОИГРЫШ (GAME OVER)
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            //ЕСТЬ ПОБЕДИТЕЛЬ:
            //Debug.Log("ПОБЕДИТЕЛЬ - " + playerList[0].nickname);
            OnUpdateMessage("ПОБЕДИТЕЛЬ - " + playerList[0].nickname);
            //ОСТАНОВИТЬ ИГРУ
            gameOverPanel.SetActive(true);
            //winnerName.text = playerList[0].nickname +" - МОНОПОЛИСТ. (L.)";
            //ПОКАЗАТЬ UI //DESIGN
        }
    }

    //-----------------------------------UI------------------------
    void DeactivateArrows()
    {
        foreach (var player in playerList)
        {
            player.ActivateArrowSelector(false);//вырубание стрелочек или "Активный игрок"
        }
    }

    //----------------------ПРОДОЛЖИТЬ ИГРУ CONTINUE() ФУНКЦИЯ-------------(нужно invoke в playere но он не monobehaviour)
    public void Continue()
    {
        if (playerList.Count > 1)//fix#166-2+ (164) проблемы когда можно было удалить предпоследнего игрока, но игра продолжалась
        {
            SwitchPlayer();
        }
        Invoke("ContinueGame", SecondsBetweenTurns);
    }
    public void ContinueGame()
    {
        //ЕСЛИ ПОСЛЕДНИЙ БРОСОК ДУБЛЬ -
        if (RolledADouble || !hasRolledDice)
        {
            //-БРОСАЙ СНОВА
            //RollDice();
            //Debug.Log("ContinueGame Roll");
            RollPhysicalDices();
            hasRolledDice = true;
        }
        else//ЕСЛИ НЕТ - ПЕРЕХОД ХОДА
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

