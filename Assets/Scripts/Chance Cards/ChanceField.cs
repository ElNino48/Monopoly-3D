using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ChanceField : MonoBehaviour
{
    //singletone:
    public static ChanceField instance;

    [SerializeField] List<SCR_ChanceCard> cards = new List<SCR_ChanceCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3;//Ïğÿòàòü êàğòû àâòîìàòè÷åñêè, åñëè íàäî
    [SerializeField] Button closeCardButton;

    List<SCR_ChanceCard> cardDeck = new List<SCR_ChanceCard>();//ÊÎËÎÄÀ ÊÀĞÒÎ×ÅÊ
    List<SCR_ChanceCard> usedCardDeck = new List<SCR_ChanceCard>();//ÊÎËÎÄÀ ÑÁĞÎÑÀ

    SCR_ChanceCard jailFreeCard;
    //ÈÍÔÀ Î ÒÅÊÓÙÅÉ ÊÀĞÒÎ×ÊÅ È ÒÅÊÓÙÅÌ ÈÃĞÎÊÅ
    SCR_ChanceCard pickedCard;
    Player currentPlayer;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//êàæäûé ğàç êîãäà ìû ÷òî-òî äåëàåì - ïğîèñõîäèò Invoke Human Panel 

    private void OnEnable()
    {
        MonopolyNode.OnDrawChanceCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyNode.OnDrawChanceCard -= DrawCard;
    }

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        cardHolderBackground.SetActive(false);
        //ÄÎÁÀÂÈÒÜ ÂÑÅ ÊÀĞÒÎ×ÊÈ Â ÊÎËÎÄÓ
        cardDeck.AddRange(cards);

        //ÏÅĞÅÌÅØÀÒÜ ÊÎËÎÄÓ
        ShuffleCards();
    }
    void ShuffleCards()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int index = Random.Range(0, cardDeck.Count);
            SCR_ChanceCard tempCard = cardDeck[index];
            cardDeck[index] = cardDeck[i];
            cardDeck[i] = tempCard;
        }
    }
    void DrawCard(Player cardTaker)
    {
        //ÂÇßÒÜ ÊÀĞÒÎ×ÊÓ
        pickedCard = cardDeck[0];//Âûáğàë êàğòî÷êó
        cardDeck.RemoveAt(0);//Óáğàë èñïîëüçîâàííóş êàğòî÷êó èç êîëîäû

        if (pickedCard == jailFreeCard)
        {
            jailFreeCard = pickedCard;
        }
        else
        {
            usedCardDeck.Add(pickedCard);//Çàêèíóë èñïîëüçîâàííóş êàğòî÷êó â ñáğîñ
        }
        
        if (cardDeck.Count == 0)
        {
            //ÍÅ ÎÑÒÀËÎÑÜ ÊÀĞÒÎ×ÅÊ = ÍÓÆÍÎ ÂÅĞÍÓÒÜ ÑÁĞÎÑ È ÇÀØÀÔËÈÒÜ
            cardDeck.AddRange(usedCardDeck);
            usedCardDeck.Clear();
            ShuffleCards();
        }
        //ÊÒÎ ÒÅÊÓÙÈÉ ÈÃĞÎÊ?
        currentPlayer = cardTaker;

        //ÏÎÊÀÇÀÒÜ İÒÓ ÊÀĞÒÎ×ÊÓ
        cardHolderBackground.SetActive(true);

        //Debug.Log("Chance set actvie?.");
        //ÂÑÒÀÂÈÒÜ ÒÅÊÑÒ ÎÏÈÑÀÍÈß ÊÀĞÒÎ×ÊÈ
        cardText.text = pickedCard.descriptionOnCard;

        //ÏĞÈÌÅÍÈÒÜ ÊÀĞÒÎ×ÊÓ (ıôôåêòû, êîòîğûå îíà äà¸ò)
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);//ÀÂÒÎÇÀÊĞÛÒÈÅ ÊÀĞÒÎ×ÊÈ ÄËß ÈÈ/AI
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }
    public void ApplyCardEffect()//ÍÀ ÊÍÎÏÊÓ ÇÀÊĞÛÒÈß ÊÀĞÒÎ×ÊÈ p.s.(ËÓ×ØÅ ÍÀ ÌÎÌÅÍÒ ÑÎÂÅĞØÅÍÈß ÍÀÂÅĞÍÎÅ)
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penaltyMoney != 0 && pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penaltyMoney);//ĞÀÇÎÁĞÀÒÜÑß Ñ ÁÀÍÊĞÎÒÑÒÂÎÌ ÒÓÒ
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            //ÑÊÎËÜÊÎ ØÀÃÎÂ ÄÎ ÖÅËÈ?

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;//40 (íóæíî îòíÿòü 1?) -1 
            int stepsToMove = 0;

            if (currentIndex < pickedCard.moveToBoardIndex)
            //Äëÿ òîãî ÷òîáû èç ïîëÿ 2 ïîéòè â ïîëå 8.
            //Íóæíî èç ïîëÿ 2 ïğîéòè (8-2)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            //Äëÿ òîãî ÷òîáû èç ïîëÿ 8 ïîéòè â ïîëå 2.
            //Íóæíî èç ïîëÿ 8 ïğîéòè (40-8+2), ïğîõîäÿ ÷åğåç ïîëå GO(0).
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }

            //ÄÅËÀÅÒ ÏÅĞÅÌÅÙÅÍÈÅ
            MonopolyBoard.instance.MovePlayerToken(stepsToMove, currentPlayer);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayers;
            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    //ÈÇÁÅÆÀÒÜ ÁÀÍÊĞÎÒÑÒÂÀ
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penaltyMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];//40 çà ÄÎÌ 115 çà ÎÒÅËÜ
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = true;
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard)// ÊÀĞÒÎ×ÊÀ "ÂÛÉÒÈ ÈÇ ÒÓĞÌÛ"
        {
            currentPlayer.AddChanceJailFreeCard();
        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = Mathf.Abs(pickedCard.moveStepsBackwards);
            MonopolyBoard.instance.MovePlayerToken(-steps, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextRailroad)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Railroad, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextUtility)
        {
            MonopolyBoard.instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
            isMoving = true;
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving)
            {
                GameManager.instance.Continue();
            }
        }
        else //HUMAN INPUT
        {
            if (!isMoving)
            {
                bool jailFreeChanceCard = currentPlayer.HasChanceJailFreeCard;
                bool jailFreeCommunityCard = currentPlayer.HasCommunityJailFreeCard;
                OnShowHumanPanel.Invoke(true, 
                    GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble,
                    jailFreeChanceCard,jailFreeCommunityCard);
                //ÅÑËÈ ÄÓÁËÜ- Êíîïêà ĞÎËË àêòèâíà, åñëè ÄÓÁËÜ - êíîïêà ÊÎÍÅÖ ÕÎÄÀ íåàêòèâíà è íàîáîğîò
            }
        }
    }

    public void AddBackJailFreeCard()
    {
        usedCardDeck.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
