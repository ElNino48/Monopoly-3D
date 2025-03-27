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
    [SerializeField] GameObject mortgageImage;//залог
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    //MESSAGE SYSTEM - СИСТЕМА ДИАЛОГОВЫХ ОКОН
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //ПОЛУЧИТЬ КАРТЫ COMMUNITY
    public delegate void DrawCommunityCard(Player player);
    public static DrawCommunityCard OnDrawCommunityCard;

    //ПОЛУЧИТЬ КАРТЫ CHANCE
    public delegate void DrawChanceCard(Player player);
    public static DrawChanceCard OnDrawChanceCard;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//каждый раз когда мы что-то делаем - происходит Invoke Human Panel 

    //BUY PROPERTY PANEL
    public delegate void ShowPropertyBuyPanel(MonopolyNode node, Player player);
    public static ShowPropertyBuyPanel OnShowPropertyBuyPanel;
    
    //BUY RAILROAD PANEL //ЖД
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
    }//собственно FIX mortgaging №18

    public void OnValidate()
    {
        if(nameText != null)
        {
            nameText.text = name;
        }

        //ВЫЧИСЛЕНИЯ
        if(calculateRentAuto)
        {
            if(type == MonopolyNodeType.Property)
            {
                if(baseRent>0)
                {
                    price = 3 * (baseRent * 10);
                    //цена залога
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
        //ОБНОВЛЕНИЕ ВЛАДЕЛЬЦА
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

    //MORTGAGE (ЗАЛОГ)==================== и всё о нём ниже====================
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

    //ОБНОВЛЕНИЕ ВЛАДЕЛЬЦА

    public void OnOwnerUpdated() //ОБНОВЛЕНИЕ ВЛАДЕЛЬЦА
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
        //ПРОВЕРКА НА ТИП КАРТОЧКИ И ДЕЙСТВИЕ
        //Debug.Log("Player Landed on node.");
        switch (type)
        {
            case MonopolyNodeType.Property:
                if (!isPlayerHuman)//AI
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //расчёт ренты
                        //Debug.Log("НУЖНО ЗАПЛАТИТЬ РЕНТУ. ВЛАДЕЛЕЦ - " + owner.nickname);
                        int rentToPay = CalculatePropertyRent();
                        
                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);
                    }
                    else if( owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI Покупает карточку
                        //Debug.Log("ВЫ МОЖЕТЕ ПРИОБРЕСТИ");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " приобретает " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        //OnOwnerUpdated();

                        //УВЕДОМЛЕНИЕ
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + МОЖЕМ ПОЗВОЛИТЬ

                    }
                }
                else//HUMAN
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if (owner!= null && owner != currentPlayer && !isMortgaged)
                    {

                        //расчёт ренты
                        //Debug.Log("НУЖНО ЗАПЛАТИТЬ РЕНТУ. ВЛАДЕЛЕЦ - " + owner.nickname);
                        int rentToPay = CalculatePropertyRent();

                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI для покупки и управления
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + НЕ МОЖЕМ ПОЗВОЛИТЬ
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer);
                    }
                }
                break;

            case MonopolyNodeType.Utility:
                if (!isPlayerHuman)//AI
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //расчёт ренты
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI Покупает карточку
                        //Debug.Log("ВЫ МОЖЕТЕ ПРИОБРЕСТИ");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " приобретает " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //УВЕДОМЛЕНИЕ
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + НЕ МОЖЕМ ПОЗВОЛИТЬ

                    }
                }
                else//HUMAN
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        //расчёт ренты
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI для покупки и управления
                        OnShowUtilityBuyPanel.Invoke(this,currentPlayer);
                        //УВЕДОМЛЕНИЕ
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + МОЖЕМ ПОЗВОЛИТЬ

                    }
                }
                break;

            case MonopolyNodeType.Railroad:
                if (!isPlayerHuman)//AI
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //расчёт ренты
                        //Debug.Log("НУЖНО ЗАПЛАТИТЬ РЕНТУ. ВЛАДЕЛЕЦ - " + owner.nickname);
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        //AI Покупает карточку
                        //Debug.Log("ВЫ МОЖЕТЕ ПРИОБРЕСТИ");
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " приобретает " + this.name);//DESIGN
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        //УВЕДОМЛЕНИЕ
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + МОЖЕМ ПОЗВОЛИТЬ

                    }
                }
                else//HUMAN
                {
                    //Если ЗАНЯТО + НЕ НАШЕ + НЕ ЗАЛОЖЕНО - плачу ренту кому-то
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {

                        //расчёт ренты
                        //Debug.Log("НУЖНО ЗАПЛАТИТЬ РЕНТУ. ВЛАДЕЛЕЦ - " + owner.nickname);
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        //оплата ренты
                        currentPlayer.PayRent(rentToPay, owner);

                        //Уведомление о транзакции (для всех игроков)
                        OnUpdateMessage.Invoke(currentPlayer.nickname + " платит " + rentToPay + " в качесте ренты игроку " + owner.nickname);

                    }
                    else if (owner == null)
                    {
                        //UI для покупки и управления
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer);
                        //УВЕДОМЛЕНИЕ
                    }
                    else
                    {
                        //ЕСЛИ НЕ ЗАНЯТО + НЕ МОЖЕМ ПОЗВОЛИТЬ

                    }
                }
                break;

            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //ПОКАЗАТЬ УВЕД.!!
                OnUpdateMessage.Invoke(currentPlayer.nickname + " оплачивает налог в размере <b><color=red>" + price+
                    "BYN</b></color>");//DESIGN + BYN
                break;

            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                //ПОКАЗАТЬ УВЕД.!!
                OnUpdateMessage.Invoke(currentPlayer.nickname + " производит инкассацию и получает <b><color=green>" + tax +
                    "BYN</b></color>");//DESIGN + BYN
                break;

            case MonopolyNodeType.GoToJail:
                //ИНДЕКС ПОЛЯ "ИДИ В ТУРМУ" - 30 на 21.10.24 13:20 ========== id 30 
                //ИЛИ МОЖНО ЗАПРОСИTЬ
                int indexOnBoard = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                OnUpdateMessage.Invoke(currentPlayer.nickname +
                    " <b><color=red>задержан</b></color> до выяснения обстоятельств.");//DESIGN
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

        //СТОП ЗДЕСЬ ЕСЛИ ЧТО (Посещение тюрмы)
        if(!continueTurn)
        {
            return;
        }
        //ПРОДОЛЖЕНИЕ
        if(!isPlayerHuman)
        {
            //Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
            currentPlayer.ChangeState(Player.AIStates.TRADING);
        }
        else
        {
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            //HUD/UI для игрока + проверка на дубль
            bool jailFreeChanceCard = currentPlayer.HasChanceJailFreeCard;
            bool jailFreeCommunityCard = currentPlayer.HasCommunityJailFreeCard;
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, jailFreeChanceCard, jailFreeCommunityCard);
        }
    }

    //void ContinueGame()
    //{
    //    //ЕСЛИ ПОСЛЕДНИЙ БРОСОК ДУБЛЬ -
    //    if (GameManager.instance.RolledADouble)
    //    {
    //        //-БРОСАЙ СНОВА
    //        GameManager.instance.RollDice();
    //    }
    //    else//ЕСЛИ НЕТ - ПЕРЕХОД ХОДА
    //    {
                        
    //        GameManager.instance.SwitchPlayer();
    //    }

    //}

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0://НЕТ домиков

                //Проверка, имеет ли OWNER полный сет карточек одного цвета!
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

            case 1://1 домик
                currentRent = rentWithHouses[0];//0+1=1
                break;

            case 2://2 домика
                currentRent = rentWithHouses[1];//1+1=2
                break;

            case 3://3 домика
                currentRent = rentWithHouses[2];//2+1=3
                break;

            case 4://4 домика
                currentRent = rentWithHouses[3];//3+1=4
                break;

            case 5://ОТЕЛЬ
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
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;//Цена утилити если у игрока в наличии все утилити на карте: (ролл1+ролл2)*10
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;//Цена утилити: (ролл1+ролл2)*4
        }

        return result;
    }

    int CalculateRailroadRent()
    {
        int result = 0;
        var (list, areAllSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(this);

        //вычисления ЖД:

        int amount = 0;
        foreach (var item in list)
        {
            /* 
             * if (item.owner == this.owner)
                 amount++; //То же самое: amount += (item.owner == this.owner)?1:0;
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
            case 0://БЕЗ ДОМИКОВ
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 1://1 ДОМИК
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 2://2 ДОМИКА
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 3://3 ДОМИКА
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
                break;
            case 4://4 ДОМИКА
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
                break;
            case 5://ОТЕЛЬ
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
            return houseCost / 2;//ФОРМУЛА ДЛЯ ПРОДАЖИ
        }
        return 0;
    }

    public void ResetNode()
    {
        if (isMortgaged)//ЕСЛИ ЗАЛОЖЕНА
        {
            propertyImage.SetActive(true);
            mortgageImage.SetActive(false);
            isMortgaged = false;
        }
        //RESET ДОМИКИ И ОТЕЛИ НА КАРТОЧКЕ
        if (type == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualiseHouses();
        }
        //RESET OWNER

        //RESET PROPERTY от OWNER
        owner.RemoveProperty(this);
        owner.ActivateArrowSelector(false);//DESIGN МОЖНО СДЕЛАТЬ ТУТ КАРТИНКУ - БАНКРОТ
        owner = null;
        //UPDATE UI/HUD
        OnOwnerUpdated();
    }

    //==============ТОРГОВЛЯ===============

    //------------ИЗМЕНИТЬ ВЛАДЕЛЬЦА КАРТОЧЕК--------------
    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddProperty(this);
        SetOwner(newOwner);
    }
}
