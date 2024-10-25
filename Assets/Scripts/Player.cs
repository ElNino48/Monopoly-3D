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

    //PLAYER INFO
    PlayerInfo myInfo;

    //UI Show Panel
    UIShowProperty uiShowPropertyInfo;

    //MESSAGE SYSTEM - СИСТЕМА ДИАЛОГОВЫХ ОКОН
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;//каждый раз когда мы что-то делаем - происходит Invoke Human Panel 

    //AI
    int aiMoneySavity = 200;

    //return некоторых статусов и информации
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
        //ПРОВЕРКИ НА:
        //игрок наступил на место, где можно делать активные действия 
        newNode.PlayerLandedOnNode(this);

            //если это ИИ:
            if(playerType == PlayerType.AI)
        {
            //Может строить домики?
            CheckIfPlayerHasASet();
            //Если у ИИ есть заложенные карточки?
            UnMortgageProperties();
            //Может ли торговать, чтобы получить недостающие карточки (нужны все одного цвета чтобы стать монополистом и строить домики)

        }
        
    }

    public void CollectMoney(int amount)
    {
        money += amount;

        UIShowPanel.instance.UpdateBalance(money);
        myInfo.SetPlayerCash(money);
        //ПРОВЕРКА (как и в MonopolyNode):
        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0;
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0;
            //HUD/UI для игрока + проверка на дубль
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn);//
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
        //ОБНОВЛЕНИЕ СЧЁТА БАЛАНСА
        Debug.Log(money + "= money 1TYT");
        ManageUI.instance.UpdateBottomMoneyText(money);
        myInfo.SetPlayerCash(money);
        //uiShowPropertyInfo.SetPlayerMoneyBalance(money);
        //УСТАНОВИТЬ ВЛАДЕНИЕ
        myMonopolyNodes.Add(node);

        //СОРТИРОВКА КАРТОЧКИ ПО ЦЕНЕ
        SortPropertiesByPrice();
    }
    void SortPropertiesByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        //(RENT) ЕСЛИ НЕТ ДОСТАТОЧНО ДЕНЕГ - залоги кредиты и прочая шляпа:
        if(money < rentAmount)
        {
            if (playerType == PlayerType.AI)
            {
                //AI
                HandleInsufficientFunds(rentAmount);
            }
            else
            {
                //ВЫКЛЮЧИТЬ ВОЗМОЖНОСТЬ ЗАВЕРШИТЬ ХОД ИЛИ БРОСИТЬ ДАЙС
                OnShowHumanPanel.Invoke(true, false, false);
            }
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        //Обновление UI для игрока и владельца и вообще

        UIShowPanel.instance.UpdateBalance(money);
        myInfo.SetPlayerCash(money);
    }

    internal void PayMoney(int taxAmount)
    {
        //(TAX) ЕСЛИ НЕТ ДОСТАТОЧНО ДЕНЕГ - залоги кредиты и прочая шляпа:
        if (money < taxAmount)
        {
            if (playerType == PlayerType.AI)
            {
                //AI
                HandleInsufficientFunds(taxAmount);
            }
            //else
            //{
            //    //ВЫКЛЮЧИТЬ ВОЗМОЖНОСТЬ ЗАВЕРШИТЬ ХОД ИЛИ БРОСИТЬ ДАЙС
            //    OnShowHumanPanel.Invoke(true, false, false);
            //}
        }
        money -= taxAmount;
        //Обновление UI для игрока и владельца и вообще

        UIShowPanel.instance.UpdateBalance(money);
        myInfo.SetPlayerCash(money);

        //ПРОВЕРКА (как и в MonopolyNode):
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0;
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0;
            //HUD/UI для игрока + проверка на дубль
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn);//
        }
    }

    //-----------------------------------------------======--------------------------------------------------------------------------------------
    //-----------------------------------------------=JAIL=--------------------------------------------------------------------------------------
    //-----------------------------------------------======--------------------------------------------------------------------------------------
   
    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        //ПЕРЕМЕЩЕНИЕ ИГРОКА:
        //myToken.transform.position = MonopolyBoard.instance.route[10].transform.position;
        //currentnode = MonopolyBoard.instance.route[10];

        MonopolyBoard.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);//ЭТОТ ИГРОК ШУРУЕТ 20 КЛЕТОК НАЗАД В ТУРМУ
        GameManager.instance.ResetRolledADouble();//обнуляем проверку на дубль поскольку уже попали в тюрьму и не можем ходить ещё раз
    }

    public void SetFreeOfJail()
    {
        isInJail = false;
        ResetNumTurnsInJail();
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;//ИНДЕКС ПОЛЯ "ТУРМА" НА КАРТЕ - 10 на 21.10.24 13:16
        if (indexOnBoard > indexOfJail)
        {
            result = -(indexOnBoard - indexOfJail);// - отрицательное значение если на позиции ПОСЛЕ турмы
        }
        else
        {
            result = (indexOfJail - indexOnBoard);// + или 0 если ПЕРЕД или НА поле турмы
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
    //---------------------------------------ДОМИКИ И ОТЕЛИ----------------------------------
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
                hotels += 1;//отели можно ставить только когда их уже 4, отель - 5
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0;//Доступные домики на продажу
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        //ПОДСЧЁТ ВСЕХ ДОМИКОВ
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        //ПРОЙТИ ЦИКЛОМ ПО КАРТОЧКАМ И ПОПЫТАТЬСЯ ПРОДАТЬ СТОЛЬКО СКОЛЬКО НУЖНО
        while (money<amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    //НУЖНО БОЛЬШЕ ДЕНЕГ?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        //ЗАЛОГИ:

        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (node.IsMortgaged) ? 0 : 1;// или (!node.IsMortgaged) ? 1 : 0;
        }
        //ПРОЙТИ ЦИКЛОМ ПО КАРТОЧКАМ И ПОПЫТАТЬСЯ ЗАЛОЖИТЬ СТОЛЬКО СКОЛЬКО НУЖНО
        while(money<amountToPay && propertiesToMortgage >0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (node.IsMortgaged) ? 0 : 1;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortgageProperty());//ЗАЛОГИ DESIGN
                    allPropertiesToMortgage--;
                    //НУЖНО БОЛЬШЕ ДЕНЕГ?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }
        //БАНКРОТ ЕСЛИ ПОПАЛ СЮДА:
        Bankrupt();
    }

    void Bankrupt()
    {
        //УБРАТЬ ИГРОКА ИЗ ИГРЫ

        //УВЕДОМЛЕНИЕ
        OnUpdateMessage.Invoke(nickname + " <b><color=red>ОБАНКРОТИЛСЯ</b></color>.");//TMP (text mesh pro) HELP FILES DESIGN 

        //ОЧИСТИТЬ ВСЁ ЧТО БЫЛО ЗАНЯТО ИГРОКОМ
        for (int i = myMonopolyNodes.Count-1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }

        //УДАЛИТЬ ИГРОКА
        GameManager.instance.RemovePlayer(this);
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
    }

    void UnMortgageProperties()//Для AI на данный момент 22.10 20:55
    {
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                int cost = node.MortgagedValue + (int)(node.MortgagedValue * 0.1f);//цена выкупа из залога, формула (10%наценка)
                //Можем ли позволить выкупить из залога?
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
            List<MonopolyNode> nodeSets = list;//ТОЛЬКО 1 ДОМИК ЗА ХОД
            if(nodeSets != null && nodeSets != processedSet)
            {
                bool hasMortgagedNode = nodeSets.Any(node => node.IsMortgaged)?true:false;//Имеет ли ЗАЛОЖЕННУЮ карточку
                if (!hasMortgagedNode)
                {
                    if(nodeSets[0].type == MonopolyNodeType.Property)//МОЖЕМ ПОСТРОИТЬ ДОМ ТУТ
                    {
                        BuildHousesOrHotelEvenly(nodeSets);
                        //ОБНОВЛЕНИЕ 
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
        //ПОЛУЧЕНИЕ МИН и МАКС КОЛИЧЕСТВА ДОМИКОВ СЕЙЧАС ВО ВЛАДЕНИИ НА КАРТОЧКЕ
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
            //ЗНАЧИТ У ИГРОКА ЕСТЬ МЕСТО
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses< 5 && CanAffordHouse(node.houseCost)){
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                //ОСТАНОВКА ЦИКЛА ЕСЛИ НУЖНО ПОСТРОИТЬ ТОЛЬКО ОДИН ДОМИК
                break;
            }
        }
    }

    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool isHouseSold = false;//FIX ошибки, когда не продавались домики
        foreach (var node in nodesToSellFrom)
        {//ПРОДАЖА ДОМИКОВ НАЧИНАЕТСЯ С КАРТОЧКИ, НА КОТОРОЙ ЭТИХ ДОМИКОВ БОЛЬШЕ ВСЕГО
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses);
            break;
        }
        //ПРОДАТЬ SELL HOUSE ДОМ
        for (int i = nodesToSellFrom.Count - 1; i >= 0; i--)
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)
            {//уже в Player'e(class Player)
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

    //public/internal?
    internal bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)//AI
        {

            return (money - aiMoneySavity) >= price;
        }
        return money >= price;//HUMAN
    }

    public void ActivateArrowSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }
}
