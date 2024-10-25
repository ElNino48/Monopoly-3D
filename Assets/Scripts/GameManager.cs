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

    [SerializeField] int maxTurnsInJail = 3;//НАСТРОЙКИ ЧТОБЫ ЧАЛИТЬСЯ НА НАРАХ
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;
    [SerializeField] float secondsBetweenTurns = 3;

    [Header("Player Info")]

    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerUI;//ДЛЯ ТОГО ЧТОБЫ playerInfoPrefab'ы СТАЛИ РОДИТЕЛЯМИ 
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();
    
    //БРОСОК КОСТЕЙ

    int[] rolledDice;
    bool isDoubleRolled;
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
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
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
        Initialize();
        if(playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }
        else
        {
            //UI для живого игрока
        }
    }

    private void Initialize()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerUI, false);//false - Local
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            //Рандомный токен:
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

    public void RollDice()//РЕАКЦИЯ НА НАЖАТИЕ КНОПКИ ИГРОКОМ ИЛИ ИИ
    {

        bool isAllowedToMove = true;

        //RESET ПОСЛЕДНИЙ РОЛЛ
        rolledDice = new int[2];

        //СОХРАНИТЬ РОЛЛ
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);
        Debug.Log("Результат броска: " + rolledDice[0] + "+" + rolledDice[1]);

        //DEBUG
        if (alwaysDoubleRoll)
        {
            rolledDice[0] = 0;
            rolledDice[1] = 1;
        }

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
            Debug.Log("дублей: "+(doubleRollCount+1));
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

        //ПОКАЗАТЬ/СКРЫТЬ HUD/UI
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(true, false, false);
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
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
        currentPlayer++;

        //БРОСИЛ ДУБЛЬ?
        doubleRollCount = 0;

        //ПРОВЕРКА НА OVERFLOW
        if (currentPlayer>=playerList.Count)
        {
            currentPlayer = 0;
        }

        DeactivateArrows();
        playerList[currentPlayer].ActivateArrowSelector(true);

        //В ТУРМЕ?

        //Игрок - ИИ?
        if(playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(false, false, false);//deactivate panel когда ход AI 
        }
        else
        {
            //OnUpdateBottomBalance.Invoke(playerList[currentPlayer].ReadMoney);
            OnShowHumanPanel.Invoke(true, true, false);//panel+ roll+ end-
        }
    }

    //ЭТО:
    public int[] LastRolledDice() => rolledDice;
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
            Debug.Log("ПОБЕДИТЕЛЬ - " + playerList[0].nickname);
            OnUpdateMessage("ПОБЕДИТЕЛЬ - " + playerList[0].nickname);
            //ОСТАНОВИТЬ ИГРУ

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
}

