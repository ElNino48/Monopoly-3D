using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    //синглтон:
    public static TradingSystem instance;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] Image failTradeIcon;//DESIGN
    [SerializeField] Image successTradeIcon;//DESIGN
    [SerializeField] TMP_Text resultMessageText;
    [SerializeField] TMP_Text fromWhoText;
    [SerializeField] TMP_Text titleText;

    [Header("ЛЕВАЯ СТОРОНА - OFFER")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup; //АКТИВАТОР ВЫБОРА КАРТОЧКИ
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoneyText;
    [SerializeField] Slider leftOfferMoneySlider;

    List<GameObject> leftCardPrefabList = new List<GameObject>();
    Player leftPlayerReference;

    [Header("ЦЕНТР")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject choosePlayerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();
    [SerializeField] Button submitTradeOfferButton;

    [Header("ПРАВАЯ СТОРОНА - REQUEST")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup; //АКТИВАТОР ВЫБОРА КАРТОЧКИ
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoneyText;
    [SerializeField] Slider rightOfferMoneySlider;

    List<GameObject> rightCardPrefabList = new List<GameObject>();
    Player rightPlayerReference;

    [Header("ОКНО ПРЕДЛОЖЕННОЙ СДЕЛКИ(от ИИ к ИГРОКУ)")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text fromWhoOfferText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text leftOfferedMoneyText;
    [SerializeField] TMP_Text rightRequestedMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] Image leftPropertyImage, rightPropertyImage;
    [SerializeField] Sprite propertySprite, railroadSprite, utilitySprite;
    [SerializeField] TMP_Text leftCardNameText, rightCardNameText, leftCardPriceText, rightCardPriceText;
    [SerializeField] Image tradeICON;

    //ДАННЫЕ ДЛЯ ПРЕДЛОЖЕНИЙ ТРЕЙДА ОТ ИИ ИГРОКУ
    Player currentPlayer, nodeOwner;
    MonopolyNode offeredNode, requestedNode;
    int offeredMoney, requestedMoney;

    public bool isCorrupted = false;

    //MESSAGE SYSTEM - СИСТЕМА ДИАЛОГОВЫХ ОКОН
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //SKILLS
    public float SpeculantBonus { get; set; } = 0.05f;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
    }

    //НАХОЖДЕНИЕ НЕДОСТАЮЩИХ КАРТОЧЕК ДЛЯ СЕТА (ИИ)
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;//НЕ ИЗМЕНЯТЬ processedSet!!!
        MonopolyNode requestedNode = null;

        //Debug.Log("TRADING+  FIND");
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSets = new List<MonopolyNode>();
            nodeSets.AddRange(list);//Теперь это копия (аналогично ManageUI)

            //ПРОВЕРИТЬ, КУПЛЕНЫ ЛИ ВСЕ
            //если хоть один - null, то куплены не все (notAllPurchased = true)
            bool notAllPurchased = list.Any(n => n.Owner == null);

            //У ИИ уже есть этот сет?
            if (allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            //НАЙТИ ТУ, ЧТО ЗАНЯТА ДРУГИМ ИГРОКОМ
            //НО ПРОВЕРИТЬ, ЧТО У ИГРОКА КОЛИЧЕСТВО КАРТОЧЕК БОЛЬШЕ СРЕДНЕГО
            if (list.Count == 2)//сет из 2 карточек + !!! (ЖД - 4, УТИЛИТИ - 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);//Лямбда
                if (requestedNode != null)
                {
                    //СДЕЛАТЬ ПРЕДЛОЖЕНИЕ ВЛАДЕЛЬЦУ КАРТОЧКИ
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);//ОФФЕР ВНУТРИ Decision
                    break;//или return;??
                }
            }
            if (list.Count >= 3)//сет из 3 карточек и ЖД
            {//LINQ
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)//+жд утил
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);//Лямбда
                    //СДЕЛАТЬ ПРЕДЛОЖЕНИЕ ВЛАДЕЛЬЦУ КАРТОЧКИ
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;//или return;??
                }
            }
        }
        //ПРОДОЛЖЕНИЕ ИГРЫ ЕСЛИ НИЧЕГО НЕ НАЙДЕНО (ТРЕЙД НЕВОЗМОЖЕН)   
        if (requestedNode == null)
        {
            //Debug.Log("TRADING+  null");
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //ПРИНЯТИЕ РЕШЕНИЯ О ЦЕЛЕСООБРАЗНОСТИ ОФФЕРА
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        Debug.Log("7"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
        //ДЕНЬГАМИ, ЕСЛИ МОЖНО (ФОРМУЛА ТОРГОВЛИ ДЕНЬГАМИ)
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode, currentPlayer))
        {
            //ТОРГОВЛЯ ТОЛЬКО ТОЛЬКО ДЕНЬГАМИ:
            //СОЗДАНИЕ ОФФЕРА
            Debug.Log("ДЕНЕЖНЫЙ ТРЕЙД:");
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode, currentPlayer), 0);
            //offeredNode=null для торговли ДЕНЬГАМИ
            return;
        }
        bool foundDecision = false;//ПРОВЕРКА + фикс бага (не выходил из цикла если не находил как поторговать)

        //НАЙТИ ВСЕ НЕПОЛНЫЕ СЕТЫ И ВЫЧЕСТЬ СЕТ С ЗАПРОШЕННОЙ КАРТОЧКОЙ
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            //Contains:
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            if (checkedSet.Contains(requestedNode))
            {
                //НАЙДЕН, ОСТАНОВИТЬ ПОИСК
                continue;
            }
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)//КАЖДАЯ КАРТОЧКА, ГДЕ ВЛАДЕЛЕЦ - currentPLayer
            {
                if (CalculateValueOfNode(node, currentPlayer) + currentPlayer.ReadMoney >= requestedNode.Price)
                {
                    //ФОРМУЛА рассчёта РАЗНИЦЫ МЕЖДУ ТОРГУЕМЫМИ КАРТОЧКАМИ
                    int difference = CalculateValueOfNode(requestedNode, currentPlayer) - CalculateValueOfNode(node, currentPlayer);
                    Debug.Log(difference + "= difference");
                    if (difference >= 0)
                    {
                        //УСЛОВИЯ ВЫПОЛНЯЮТСЯ = ВОЗМОЖНАЯ СДЕЛКА
                        //СДЕЛАТЬ САМ ОФФЕР
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0);
                    }
                    else
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference));
                    }
                    //без ЗАПРОСА ДЕНЕГ + 
                    //offeredNode = node через который шёл ЦИКЛ
                    //requestedMoney = 0;
                    foundDecision = true;
                    break;
                }
            }
        }
        if (!foundDecision)//FIX#162++
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //------------------------------СДЕЛАТЬ ОФФЕР
    void MakeTradeOffer(Player currentPlayer,Player nodeOwner, MonopolyNode requestedNode, 
        MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        Debug.Log("1"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
        Debug.Log("2"+currentPlayer.playerType + " = current. \n" + nodeOwner.playerType + " = nodeOwner");

        if (nodeOwner.playerType == Player.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else if(nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            //UI РАССМОТРЕНИЯ ОФФЕРА для игрока (HUMAN)
            ShowTradeOfferPanel(currentPlayer, nodeOwner,requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }

    //РАССМОТРЕТЬ ОФФЕР(ИИ)(AI)
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode,
        MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        Debug.Log("3"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
        //(600+req0) - (300+off300) 
        //(300+req300) - (600+off0)
        Debug.Log(requestedMoney + "= reqMoney + ");
        Debug.Log(CalculateValueOfNode(requestedNode, currentPlayer) + "= CalcReq");
        Debug.Log(offeredMoney + "= offeredMoney + ");
        Debug.Log(CalculateValueOfNode(offeredNode, currentPlayer) + "= CalcOff");

        int valueOfTrade = (CalculateValueOfNode(requestedNode, currentPlayer) + requestedMoney) - (CalculateValueOfNode(offeredNode, currentPlayer) + offeredMoney);

        Debug.Log((CalculateValueOfNode(requestedNode, currentPlayer) + requestedMoney) + " = лево(но Req) -");
        Debug.Log((CalculateValueOfNode(offeredNode, currentPlayer) + offeredMoney) + " = право(ноOffer).");
        Debug.Log(valueOfTrade + " = valueOfTrade");
        //ИИ ПРЕДЛОЖИТ ИГРОКУ ТРЕЙД, ГДЕ ЗАХОЧЕТ ПРОДАТЬ КАРТОЧКУ ЗА $$$-ДЕНЬГИ-BYN (скорее всего работать не будет канешно...
        //ДЛЯ ЭТОГО: 1) не должно быть запроса карточки
        //           2) не должно быть запроса денег у игрока
        //           3) должна быть предлагаемая карточка
        //           4) ФОРМУЛА: запрошенных денег < кол-во денег игрока / 2
        //           5) ИИ не должен соглашаться, если у него есть сет
        Debug.Log(requestedNode + " = requestedNode");
        Debug.Log(offeredNode + " = offeredNode");
        Debug.Log(requestedMoney+ " = reqMoney <= nodeowner money = " + nodeOwner.ReadMoney/3);
        Debug.Log(!MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).areAllSame + "= playerHasAllNodesOfset");
        if (requestedNode == null && offeredNode!=null &&  requestedMoney <= nodeOwner.ReadMoney / 3
            && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).areAllSame)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                TradeResult(true);
            }
            return;
        }
        
        //ОБЫЧНЫЙ ТРЕЙД, ГДЕ МОЖНО ОБМЕНЯТЬ КАРТОЧКИ С ДОПЛАТОЙ(или без)
        if (valueOfTrade <= 0 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).areAllSame)
        {
            Debug.Log("4"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
            //ТОРГОВЛЯ ДОСТУПНА
            Trade(currentPlayer,nodeOwner,requestedNode,offeredNode,offeredMoney,requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                Debug.Log("5" + currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
                if (!isCorrupted)
                {
                    TradeResult(true);
                    Debug.Log("ИИ понравился оффер");
                }
                else
                {
                    TradeResult(false);
                    Debug.Log("ИИ Не понравился оффер");
                }
            }
        }
        else
        {
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                Debug.Log("6"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
                TradeResult(false);
            }
            //DEBUG или ERROR "ИИ Отказал Вам";
            Debug.Log("ИИ Не понравился оффер");
        }
    }

    //РАССЧИТАТЬ СТОИМОСТЬ КАРТОЧКИ(ИИ)
    int CalculateValueOfNode(MonopolyNode requestedNode, Player player)
    {
        int value = 0;
        if (requestedNode != null)
        {
            if (requestedNode.type == MonopolyNodeType.Property)
            {
                value = requestedNode.Price * 2 + requestedNode.NumberOfHouses * player.GetHouseCostForPlayer(requestedNode);
            }
            else
            {
                value = requestedNode.Price * 2;
            }
            return value;
        }
        return value;
    }

    //ТОРГОВАЯ ФУНКЦИЯ:
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode,
        MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        Debug.Log("Trade FUNCTION.");
        if (requestedNode != null)
        {
            Debug.Log(requestedMoney + " requestedMoney");
            Debug.Log(requestedNode.name + " requested");
            //currentPlayer ДОЛЖЕН:
            currentPlayer.PayMoney(offeredMoney); //заплатить ТОМУ, КОМУ предлагает
            requestedNode.ChangeOwner(currentPlayer); //Владельцем запрошенной карточки становится ТОТ, КТО предложил

            //nodeOwner ДОЛЖЕН:
            nodeOwner.CollectMoney(offeredMoney); //ТОТ, КОМУ предлагает ПОЛУЧАЕТ ДЕНЬГИ С ОФФЕРА
            nodeOwner.PayMoney(requestedMoney);//ТОТ, КОМУ предложил - платит
            if (offeredNode != null)
            {
                Debug.Log(offeredNode.name + " offered node");
                offeredNode.ChangeOwner(nodeOwner);//Владельцем предложенной карточки становится ТОТ, КОМУ предложил
            }
            //ОБНОВИТЬ UI(общая история событий)
            //UI ОБНОВЛЕН:
            string offeredNodeName = (offeredNode != null) ? " взамен " + offeredNode.name : "";
            if (offeredMoney != 0)
            {
                Debug.Log(offeredMoney + " offeredMoney");
                OnUpdateMessage.Invoke("<color=red>СДЕЛКА!</color> " + currentPlayer.nickname + " получил " + requestedNode.name +
                    offeredNodeName + " от  " + nodeOwner.nickname + "и " + offeredMoney + "BYN сверх этого.");
            }
            else
            {
                OnUpdateMessage.Invoke("<color=red>СДЕЛКА!</color> " + currentPlayer.nickname + " получил " + requestedNode.name +
                    offeredNodeName + " от " + nodeOwner.nickname + ".");
            }
        }
        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);//ТОТ, КТО предложил - получает
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);//Владельцем предложенной карточки становится ТОТ, КОМУ предложил
            //UI ОБНОВЛЕН:
            OnUpdateMessage.Invoke("<color=red>СДЕЛКА!</color> " + currentPlayer.nickname + " продал " + offeredNode.name + " " +
                nodeOwner.nickname + " за " + requestedMoney + ".");
        }
        else if(offeredNode==null && requestedNode == null && (requestedMoney!=0 || offeredMoney!=0))
        {
            Debug.Log("Попытка взятки.");
            isCorrupted = true;
        }

        //SKILL--Speculant
        List<Player> playerList = GameManager.instance.GetPlayers;
        if(offeredNode != null) Debug.Log("offered node price = " + offeredNode.Price);
        if(requestedNode != null) Debug.Log("requested node price = " + requestedNode.Price);
        Debug.Log("offered money = " + offeredMoney + " ;requested money " + requestedMoney);

        int totalTradeValue = offeredMoney + requestedMoney;
        if (offeredNode != null)
        {
            totalTradeValue += offeredNode.Price;
        }
        if (requestedNode != null)
        {
            totalTradeValue += requestedNode.Price;
        }
        int calculatedSpeculantBonus = (int)Mathf.Ceil(totalTradeValue * SpeculantBonus);

        foreach (Player player in playerList)
        {
            if (player.Skills.Any((playerSkill) => playerSkill.SkillType == SkillManager.SkillType.Speculant))
            {
                player.CollectMoney(calculatedSpeculantBonus);
                Debug.Log("Speculant trait granted player \"" + player.nickname +"\" " + calculatedSpeculantBonus + " BYN as bonus");
            }
        }
        ManageUI.instance.UpdateBottomMoneyText(currentPlayer.ReadMoney);

        //СПРЯТАТЬ UI
        Debug.Log("Сделка совершена!");
        CloseTradePanel();
        if(currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //UI---------------------------- ИНТЕРФЕЙС

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "кому?";
        //submitTradeOfferButton.interactable = false;
        CreateLeftPanel();
        CreateMiddleButtons();
    }
    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        ClearAll();
    }

    //CURRENT PLAYER---------------- ЛЕВО
    void CreateLeftPanel()
    {
        leftOffererNameText.text = leftPlayerReference.nickname + " предлагает ";

        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);//leftCardGrid->cardPrefab local WS
            //НАСТРОЙКА СОДЕРЖАНИЯ КАРТОЧКИ(prefab'a)
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], leftToggleGroup);

            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = "БАНК: " + leftPlayerReference.ReadMoney + "BYN";
        //НАСТРОЙКА СЛАЙДЕРА И ТЕКСТА
    leftOfferMoneySlider.maxValue = leftPlayerReference.ReadMoney;
    leftOfferMoneySlider.value = 0;
    UpdateLeftSlider(leftOfferMoneySlider.value);//ОБНОВЛЕНИЕ СЛАЙДЕРА В РЕАЛЬНОМ ВРЕМЕНИ
        //RESET ПОСЛЕ ЗАКРЫТИЯ ВКЛАДКИ

        tradePanel.SetActive(true);
    }
    
    public void UpdateLeftSlider(float value)
    {
        leftOfferMoneyText.text = "Добавить сверх предложенного: " + leftOfferMoneySlider.value + "BYN"; 
        if (leftOfferMoneySlider.value > 0) //FIX добавить проверку: выбрана ли карточка + UpdateRightSlider 
        {
            submitTradeOfferButton.interactable = true;
        }
        else
        {
            submitTradeOfferButton.interactable = false;
        }
    }


    //SELECTED PLAYER--------------- ПРАВО

    public void ShowRightPlayer(Player player)
    {
        rightPlayerReference = player;

        //!!!ОБНОВИТЬ ТЕКУЩИЙ КОНТЕНТ (от предыдущего выбора)
        ClearRightPanel();
        //ПОКАЗАТЬ ПРАВУЮ ЧАСТЬ (REQUEST) ДЛЯ ВЫБРАННОГО ИГРОКА

        rightOffererNameText.text = "обменяться с " + rightPlayerReference.nickname;
        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);//rightCardGrid->cardPrefab +local WS
            //НАСТРОЙКА СОДЕРЖАНИЯ КАРТОЧКИ(prefab'a)
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);
            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = "БАНК: " + rightPlayerReference.ReadMoney + "BYN";
        //ОБНОВИТЬ ПРАВЫЙ СЛАЙДЕР И ДЕНЬГИ
        rightOfferMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightOfferMoneySlider.value = 0;
        UpdateLeftSlider(rightOfferMoneySlider.value);//ОБНОВЛЕНИЕ СЛАЙДЕРА В РЕАЛЬНОМ ВРЕМЕНИ
      
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoneyText.text = "Запросить сверх предложенного: " + rightOfferMoneySlider.value + "BYN";
        if (rightOfferMoneySlider.value > 0)
        {
            submitTradeOfferButton.interactable = true;
        }
        else
        {
            submitTradeOfferButton.interactable = false;
        }
    }

    //MIDDLE----------------------- ЦЕНТР
    void CreateMiddleButtons()
    {
        //CLEAR
        for (int i = playerButtonList.Count - 1; i >= 0; i--) 
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();
        List<Player> allPlayers = new List<Player>();
        allPlayers.AddRange(GameManager.instance.GetPlayers);
        allPlayers.Remove(leftPlayerReference);
        foreach (var player in allPlayers)
        {
            GameObject newPlayerButton = Instantiate(choosePlayerButtonPrefab, buttonGrid, false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);

            playerButtonList.Add(newPlayerButton);
        }

        //ЦИКЛ ВСЕХ ИГРОКОВ
    }
    
    //ОЧИСТКА---------------------- ОЧИСТКА
    void ClearAll()
    {
        rightOffererNameText.text = "кому?";
        rightYourMoneyText.text = "Банк: ";
        rightOfferMoneySlider.maxValue = 0;
        rightOfferMoneySlider.value = 0;
        UpdateRightSlider(rightOfferMoneySlider.value);
        //CLEAR КНОПКИ ВЫБОРА ИГРОКА
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //CLEAR ЛЕВО - КАРТОЧКИ
        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        //CLEAR ПРАВО - КАРТОЧКИ
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }
    void ClearRightPanel()
    {
        //CLEAR ПРАВО - КАРТОЧКИ
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
        //НАСТРОЙКА СЛАЙДЕРА И ТЕКСТА
        rightOfferMoneySlider.maxValue = 0;//полностью ОБНУЛЕНИЕ слайдерп
        rightOfferMoneySlider.value = 0;
        UpdateRightSlider(rightOfferMoneySlider.value);//ОБНОВЛЕНИЕ СЛАЙДЕРА В РЕАЛЬНОМ ВРЕМЕНИ(правого)
    }

    //ОФФЕР---------------ОФФЕР---------------ОФФЕР
    public void MakeOfferButton() //ПОДТВЕРЖДЕНИЕ СДЕЛКИ (КНОПКА)
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (rightPlayerReference == null)
        {
            //ERROR ("игрока для торговли не было выбрано")//FIX кнопки актив
            //Debug.Log("игрока для торговли не было выбрано");
            return;
        }
        //ЛЕВАЯ ВЫБРАННАЯ КАРТОЧКА
        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();//НАХОЖДЕНИЕ ПЕРВОГО АКТИВНОГО ТОГГЛА (ВСЕГО ОДИН АКТИВЕН)
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();//(ВОЗМОЖНО НУЖЕН //FIX)
        }
        //ПРАВАЯ ВЫБРАННАЯ КАРТОЧКА

        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();//НАХОЖДЕНИЕ ПЕРВОГО АКТИВНОГО ТОГГЛА (ВСЕГО ОДИН АКТИВЕН)
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();//(ВОЗМОЖНО НУЖЕН //FIX)№143
        }
        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode,
            (int)leftOfferMoneySlider.value, (int)rightOfferMoneySlider.value);//ПРЯМОЕ ПРЕОБРАЗОВАНИЕ ТИПА
    }

    //------------------РЕЗУЛЬТАТ СДЕЛКИ----------------------
    void TradeResult(bool isAccepted)
    {
        
        //DESIGN
        if (isAccepted)
        {
            titleText.text = "Подтверждение";
            fromWhoText.text = "От: " +rightPlayerReference.nickname;
            resultMessageText.text = "\"Звучит выгодно\". Сказал - как отрезал.<br>" +
                " Сделка состоялась и была <color=green>подтверждена</color>.";
        }
        else
        {
            titleText.text = "Сделка не состоялась";
            fromWhoText.text = "От: " + rightPlayerReference.nickname;
            resultMessageText.text = "\"Ты кого хочешь на.. обмануть, а?\" <br>К сожалению, " + 
                " сделка не состоялась и была <color=red>отклонена</color>.";
        }
        resultPanel.SetActive(true);
    }

    //------------------------ПРЕДЛОЖЕНИЕ-----------------------(ОТ ИИ ИГРОКУ)
    void ShowTradeOfferPanel(Player _currentPlayer, Player _nodeOwner, MonopolyNode _requestedNode,
        MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        //[SerializeField] GameObject tradeOfferPanel;
        //[SerializeField] TMP_Text fromWhoOfferText;
        //[SerializeField] TMP_Text descriptionText;
        //[SerializeField] TMP_Text leftOfferedMoneyText;
        //[SerializeField] TMP_Text rightRequestedMoneyText;
        //[SerializeField] GameObject leftCard, rightCard;
        //[SerializeField] Image leftColorField, rightColorField;
        //[SerializeField] Image leftPropertyImage, rightPropertyImage;
        //[SerializeField] Sprite propertySprite, railroadSprite, utilitySprite;
        //[SerializeField] TMP_Text leftCardNameText, rightCardNameText, leftCardPriceText, rightCardPriceText;

        //ЗАПОЛНИТЬ КОНТЕНТОМ:
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        offeredMoney = _offeredMoney;
        requestedMoney = _requestedMoney;
        offeredNode = _offeredNode;
        requestedNode = _requestedNode;
        
        //ПОКАЗАТЬ ПАНЕЛЬ:
        leftCard.SetActive(false);
        rightCard.SetActive(false);
        tradeOfferPanel.SetActive(true);
        fromWhoOfferText.text = "От: " + currentPlayer.nickname;
        descriptionText.text = nodeOwner.nickname + ", я - состоятельный человек, которому не безразлична судьба народа." +
            " Я хочу предложить вам сделку. По рукам?";
        rightRequestedMoneyText.text = "+ " + requestedMoney + "BYN";
        rightCard.SetActive(requestedNode != null ? true : false);
        leftOfferedMoneyText.text = "+ " + offeredMoney + "BYN";
        leftCard.SetActive(offeredNode != null ? true : false);
        if (leftCard.activeInHierarchy && offeredNode!=null)
        {
            leftCardPriceText.text = offeredNode.Price + "BYN";
            leftCardNameText.text = offeredNode.name;
            leftColorField.color = (offeredNode.propertyColorField!=null)?offeredNode.propertyColorField.color:Color.black;//DESIGN + ДЛЯ ICON
            switch (offeredNode.type)
            {
                case MonopolyNodeType.Property:
                    leftPropertyImage.sprite = propertySprite;
                    leftPropertyImage.color = Color.white;//??DESIGN
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropertyImage.sprite = railroadSprite;
                    leftPropertyImage.color = Color.white;//??DESIGN
                    break;
                case MonopolyNodeType.Utility:
                    leftPropertyImage.sprite = utilitySprite;
                    leftPropertyImage.color = Color.black;//DESIGN!!
                    break;
            }
        }
        if (rightCard.activeInHierarchy)
        {
            rightCardNameText.text = requestedNode.name;
            rightCardPriceText.text = requestedNode.Price + "BYN";
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;//DESIGN + ДЛЯ ICON
            switch (requestedNode.type)
            {
                case MonopolyNodeType.Property:
                    rightPropertyImage.sprite = propertySprite;
                    rightPropertyImage.color = Color.white;//??DESIGN
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropertyImage.sprite = railroadSprite;
                    leftPropertyImage.color = Color.white;//??DESIGN
                    break;
                case MonopolyNodeType.Utility:
                    rightPropertyImage.sprite = utilitySprite;
                    rightPropertyImage.color = Color.black;//DESIGN!!
                    break;
            }
        }
    }
    public void AcceptOffer()
    {
        Trade(currentPlayer,nodeOwner,requestedNode,offeredNode,offeredMoney,requestedMoney);
        ResetOffer();
    }
    public void RejectOffer()
    {
        currentPlayer.ChangeState(Player.AIStates.IDLE);
        ResetOffer();
    }

    private void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null;
        offeredMoney = 0;
        requestedMoney = 0;
        offeredNode = null;
        requestedNode = null;
    }
}

