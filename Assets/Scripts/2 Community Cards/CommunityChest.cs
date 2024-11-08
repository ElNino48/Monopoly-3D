using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class CommunityChest : MonoBehaviour
{
    //singletone:
    public static CommunityChest instance;

    [SerializeField] List<SCR_CommunityCard> cards = new List<SCR_CommunityCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] Image eventImage;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime = 3;//������� ����� �������������, ���� ����
    [SerializeField] Button closeCardButton;

    List<SCR_CommunityCard> cardDeck = new List<SCR_CommunityCard>();//������ ��������
    List<SCR_CommunityCard> usedCardDeck = new List<SCR_CommunityCard>();//������ ������

    //���� � ������� �������� � ������� ������
    SCR_CommunityCard jailFreeCard;
    SCR_CommunityCard pickedCard;
    Player currentPlayer;

    //HUMAN UI
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard);
    public static ShowHumanPanel OnShowHumanPanel;//������ ��� ����� �� ���-�� ������ - ���������� Invoke Human Panel 

    private void OnEnable()
    {
        MonopolyNode.OnDrawCommunityCard += DrawCard;
    }

    private void OnDisable()
    {
        MonopolyNode.OnDrawCommunityCard -= DrawCard;
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
            SCR_CommunityCard tempCard = cardDeck[index];
            cardDeck[index] = cardDeck[i];
            cardDeck[i] = tempCard;
        }
    }

    void DrawCard(Player cardTaker)
    {
        //����� ��������
        pickedCard = cardDeck[0];//������ ��������
        cardDeck.RemoveAt(0);//����� �������������� �������� �� ������
        usedCardDeck.Add(pickedCard);//������� �������������� �������� � �����
        if (pickedCard == jailFreeCard)
        {
            jailFreeCard = pickedCard;
        }
        else
        {
            usedCardDeck.Add(pickedCard);//������� �������������� �������� � �����
        }
        if (cardDeck.Count==0)
        {
            //�� �������� �������� = ����� ������� ����� � ���������
            //Debug.Log("������ ���������");
            cardDeck.AddRange(usedCardDeck);
            usedCardDeck.Clear();
            ShuffleCards();
        }
        //��� ������� �����?
        currentPlayer = cardTaker;

        //�������� ��� ��������
        cardHolderBackground.SetActive(true);

        //�������� ����� �������� ��������
        cardText.text = pickedCard.descriptionOnCard;
        eventImage = pickedCard.eventImage;

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
        //pickedCard = cardDeck[0];//������ ��������� ������ ��� �������� �� �������� //FIX //DESIGN
        //Debug.Log("TYT?pickedCard.collectFromPlayer= " + pickedCard.collectFromPlayer + " - pickedCard.rewardMoney "+pickedCard.rewardMoney);
        if (pickedCard.rewardMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if(pickedCard.penaltyMoney !=0)
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

            if(currentIndex < pickedCard.moveToBoardIndex)
                //��� ���� ����� �� ���� 2 ����� � ���� 8.
                //����� �� ���� 2 ������ (8-2)
            {
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;
            }
            else if(currentIndex>pickedCard.moveToBoardIndex)
                //��� ���� ����� �� ���� 8 ����� � ���� 2.
                //����� �� ���� 8 ������ (40-8+2), ������� ����� ���� GO(0).
            {
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;
            }

            //������ �����������
            MonopolyBoard.instance.MovePlayerToken(stepsToMove,currentPlayer);
        }
        else if (pickedCard.collectFromPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayers;
            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    //�������� �����������
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);
                    player.PayMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);
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
            currentPlayer.AddCommunityJailFreeCard();
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {   
        if(currentPlayer.playerType == Player.PlayerType.AI)
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
                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble,
                    jailFreeChanceCard, jailFreeCommunityCard);
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
