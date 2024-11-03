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
    [SerializeField] float showTime = 3;//������� ����� �������������, ���� ����
    [SerializeField] Button closeCardButton;

    List<SCR_ChanceCard> cardDeck = new List<SCR_ChanceCard>();//������ ��������
    List<SCR_ChanceCard> usedCardDeck = new List<SCR_ChanceCard>();//������ ������

    SCR_ChanceCard jailFreeCard;
    //���� � ������� �������� � ������� ������
    SCR_ChanceCard pickedCard;
    Player currentPlayer;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//������ ��� ����� �� ���-�� ������ - ���������� Invoke Human Panel 

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
        //�������� ��� �������� � ������
        cardDeck.AddRange(cards);

        //���������� ������
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
        //����� ��������
        pickedCard = cardDeck[0];//������ ��������
        cardDeck.RemoveAt(0);//����� �������������� �������� �� ������

        if (pickedCard == jailFreeCard)
        {
            jailFreeCard = pickedCard;
        }
        else
        {
            usedCardDeck.Add(pickedCard);//������� �������������� �������� � �����
        }
        
        if (cardDeck.Count == 0)
        {
            //�� �������� �������� = ����� ������� ����� � ���������
            cardDeck.AddRange(usedCardDeck);
            usedCardDeck.Clear();
            ShuffleCards();
        }
        //��� ������� �����?
        currentPlayer = cardTaker;

        //�������� ��� ��������
        cardHolderBackground.SetActive(true);

        //Debug.Log("Chance set actvie?.");
        //�������� ����� �������� ��������
        cardText.text = pickedCard.descriptionOnCard;

        //��������� �������� (�������, ������� ��� ���)
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);//������������ �������� ��� ��/AI
        }
        else
        {
            closeCardButton.interactable = true;
        }
    }
    public void ApplyCardEffect()//�� ������ �������� �������� p.s.(����� �� ������ ���������� ��������)
    {
        bool isMoving = false;
        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penaltyMoney != 0 && pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penaltyMoney);//����������� � ������������ ���
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            //������� ����� �� ����?

            int currentIndex = MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = MonopolyBoard.instance.route.Count;//40 (����� ������ 1?) -1 
            int stepsToMove = 0;

            if (currentIndex < pickedCard.moveToBoardIndex)
            //��� ���� ����� �� ���� 2 ����� � ���� 8.
            //����� �� ���� 2 ������ (8-2)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if (currentIndex > pickedCard.moveToBoardIndex)
            //��� ���� ����� �� ���� 8 ����� � ���� 2.
            //����� �� ���� 8 ������ (40-8+2), ������� ����� ���� GO(0).
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }

            //������ �����������
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
                    //�������� �����������
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
            int totalCosts = pickedCard.streetRepairsHousePrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1];//40 �� ��� 115 �� �����
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            isMoving = true;
            currentPlayer.GoToJail(MonopolyBoard.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
        }
        else if (pickedCard.jailFreeCard)// �������� "����� �� �����"
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
                //���� �����- ������ ���� �������, ���� ����� - ������ ����� ���� ��������� � ��������
            }
        }
    }

    public void AddBackJailFreeCard()
    {
        usedCardDeck.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
