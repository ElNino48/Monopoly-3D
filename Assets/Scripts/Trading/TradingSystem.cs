using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    //��������:
    public static TradingSystem instance;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] Image failTradeIcon;//DESIGN
    [SerializeField] Image successTradeIcon;//DESIGN
    [SerializeField] TMP_Text resultMessageText;
    [SerializeField] TMP_Text fromWhoText;
    [SerializeField] TMP_Text titleText;

    [Header("����� ������� - OFFER")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup; //��������� ������ ��������
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoneyText;
    [SerializeField] Slider leftOfferMoneySlider;

    List<GameObject> leftCardPrefabList = new List<GameObject>();
    Player leftPlayerReference;

    [Header("�����")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject choosePlayerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();
    [SerializeField] Button submitTradeOfferButton;

    [Header("������ ������� - REQUEST")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup; //��������� ������ ��������
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoneyText;
    [SerializeField] Slider rightOfferMoneySlider;

    List<GameObject> rightCardPrefabList = new List<GameObject>();
    Player rightPlayerReference;

    [Header("���� ������������ ������(�� �� � ������)")]
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

    //������ ��� ����������� ������ �� �� ������
    Player currentPlayer, nodeOwner;
    MonopolyNode offeredNode, requestedNode;
    int offeredMoney, requestedMoney;

    public bool isCorrupted = false;

    //MESSAGE SYSTEM - ������� ���������� ����
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

    //���������� ����������� �������� ��� ���� (��)
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;//�� �������� processedSet!!!
        MonopolyNode requestedNode = null;

        //Debug.Log("TRADING+  FIND");
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSets = new List<MonopolyNode>();
            nodeSets.AddRange(list);//������ ��� ����� (���������� ManageUI)

            //���������, ������� �� ���
            //���� ���� ���� - null, �� ������� �� ��� (notAllPurchased = true)
            bool notAllPurchased = list.Any(n => n.Owner == null);

            //� �� ��� ���� ���� ���?
            if (allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            //����� ��, ��� ������ ������ �������
            //�� ���������, ��� � ������ ���������� �������� ������ ��������
            if (list.Count == 2)//��� �� 2 �������� + !!! (�� - 4, ������� - 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);//������
                if (requestedNode != null)
                {
                    //������� ����������� ��������� ��������
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);//����� ������ Decision
                    break;//��� return;??
                }
            }
            if (list.Count >= 3)//��� �� 3 �������� � ��
            {//LINQ
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)//+�� ����
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);//������
                    //������� ����������� ��������� ��������
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;//��� return;??
                }
            }
        }
        //����������� ���� ���� ������ �� ������� (����� ����������)   
        if (requestedNode == null)
        {
            //Debug.Log("TRADING+  null");
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //�������� ������� � ���������������� ������
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        Debug.Log("7"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
        //��������, ���� ����� (������� �������� ��������)
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode, currentPlayer))
        {
            //�������� ������ ������ ��������:
            //�������� ������
            Debug.Log("�������� �����:");
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode, currentPlayer), 0);
            //offeredNode=null ��� �������� ��������
            return;
        }
        bool foundDecision = false;//�������� + ���� ���� (�� ������� �� ����� ���� �� ������� ��� �����������)

        //����� ��� �������� ���� � ������� ��� � ����������� ���������
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            //Contains:
            var checkedSet = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node).list;
            if (checkedSet.Contains(requestedNode))
            {
                //������, ���������� �����
                continue;
            }
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)//������ ��������, ��� �������� - currentPLayer
            {
                if (CalculateValueOfNode(node, currentPlayer) + currentPlayer.ReadMoney >= requestedNode.Price)
                {
                    //������� �������� ������� ����� ���������� ����������
                    int difference = CalculateValueOfNode(requestedNode, currentPlayer) - CalculateValueOfNode(node, currentPlayer);
                    Debug.Log(difference + "= difference");
                    if (difference >= 0)
                    {
                        //������� ����������� = ��������� ������
                        //������� ��� �����
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0);
                    }
                    else
                    {
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference));
                    }
                    //��� ������� ����� + 
                    //offeredNode = node ����� ������� ��� ����
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

    //------------------------------������� �����
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
            //UI ������������ ������ ��� ������ (HUMAN)
            ShowTradeOfferPanel(currentPlayer, nodeOwner,requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }

    //����������� �����(��)(AI)
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

        Debug.Log((CalculateValueOfNode(requestedNode, currentPlayer) + requestedMoney) + " = ����(�� Req) -");
        Debug.Log((CalculateValueOfNode(offeredNode, currentPlayer) + offeredMoney) + " = �����(��Offer).");
        Debug.Log(valueOfTrade + " = valueOfTrade");
        //�� ��������� ������ �����, ��� ������� ������� �������� �� $$$-������-BYN (������ ����� �������� �� ����� �������...
        //��� �����: 1) �� ������ ���� ������� ��������
        //           2) �� ������ ���� ������� ����� � ������
        //           3) ������ ���� ������������ ��������
        //           4) �������: ����������� ����� < ���-�� ����� ������ / 2
        //           5) �� �� ������ �����������, ���� � ���� ���� ���
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
        
        //������� �����, ��� ����� �������� �������� � ��������(��� ���)
        if (valueOfTrade <= 0 && !MonopolyBoard.instance.PlayerHasAllNodesOfSet(requestedNode).areAllSame)
        {
            Debug.Log("4"+currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
            //�������� ��������
            Trade(currentPlayer,nodeOwner,requestedNode,offeredNode,offeredMoney,requestedMoney);
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
            {
                Debug.Log("5" + currentPlayer.nickname + " = current. \n" + nodeOwner.nickname + " = nodeOwner");
                if (!isCorrupted)
                {
                    TradeResult(true);
                    Debug.Log("�� ���������� �����");
                }
                else
                {
                    TradeResult(false);
                    Debug.Log("�� �� ���������� �����");
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
            //DEBUG ��� ERROR "�� ������� ���";
            Debug.Log("�� �� ���������� �����");
        }
    }

    //���������� ��������� ��������(��)
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

    //�������� �������:
    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode,
        MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        Debug.Log("Trade FUNCTION.");
        if (requestedNode != null)
        {
            Debug.Log(requestedMoney + " requestedMoney");
            Debug.Log(requestedNode.name + " requested");
            //currentPlayer ������:
            currentPlayer.PayMoney(offeredMoney); //��������� ����, ���� ����������
            requestedNode.ChangeOwner(currentPlayer); //���������� ����������� �������� ���������� ���, ��� ���������

            //nodeOwner ������:
            nodeOwner.CollectMoney(offeredMoney); //���, ���� ���������� �������� ������ � ������
            nodeOwner.PayMoney(requestedMoney);//���, ���� ��������� - ������
            if (offeredNode != null)
            {
                Debug.Log(offeredNode.name + " offered node");
                offeredNode.ChangeOwner(nodeOwner);//���������� ������������ �������� ���������� ���, ���� ���������
            }
            //�������� UI(����� ������� �������)
            //UI ��������:
            string offeredNodeName = (offeredNode != null) ? " ������ " + offeredNode.name : "";
            if (offeredMoney != 0)
            {
                Debug.Log(offeredMoney + " offeredMoney");
                OnUpdateMessage.Invoke("<color=red>������!</color> " + currentPlayer.nickname + " ������� " + requestedNode.name +
                    offeredNodeName + " ��  " + nodeOwner.nickname + "� " + offeredMoney + "BYN ����� �����.");
            }
            else
            {
                OnUpdateMessage.Invoke("<color=red>������!</color> " + currentPlayer.nickname + " ������� " + requestedNode.name +
                    offeredNodeName + " �� " + nodeOwner.nickname + ".");
            }
        }
        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);//���, ��� ��������� - ��������
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);//���������� ������������ �������� ���������� ���, ���� ���������
            //UI ��������:
            OnUpdateMessage.Invoke("<color=red>������!</color> " + currentPlayer.nickname + " ������ " + offeredNode.name + " " +
                nodeOwner.nickname + " �� " + requestedMoney + ".");
        }
        else if(offeredNode==null && requestedNode == null && (requestedMoney!=0 || offeredMoney!=0))
        {
            Debug.Log("������� ������.");
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

        //�������� UI
        Debug.Log("������ ���������!");
        CloseTradePanel();
        if(currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AIStates.IDLE);
        }
    }

    //UI---------------------------- ���������

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "����?";
        //submitTradeOfferButton.interactable = false;
        CreateLeftPanel();
        CreateMiddleButtons();
    }
    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        ClearAll();
    }

    //CURRENT PLAYER---------------- ����
    void CreateLeftPanel()
    {
        leftOffererNameText.text = leftPlayerReference.nickname + " ���������� ";

        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);//leftCardGrid->cardPrefab local WS
            //��������� ���������� ��������(prefab'a)
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], leftToggleGroup);

            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = "����: " + leftPlayerReference.ReadMoney + "BYN";
        //��������� �������� � ������
    leftOfferMoneySlider.maxValue = leftPlayerReference.ReadMoney;
    leftOfferMoneySlider.value = 0;
    UpdateLeftSlider(leftOfferMoneySlider.value);//���������� �������� � �������� �������
        //RESET ����� �������� �������

        tradePanel.SetActive(true);
    }
    
    public void UpdateLeftSlider(float value)
    {
        leftOfferMoneyText.text = "�������� ����� �������������: " + leftOfferMoneySlider.value + "BYN"; 
        if (leftOfferMoneySlider.value > 0) //FIX �������� ��������: ������� �� �������� + UpdateRightSlider 
        {
            submitTradeOfferButton.interactable = true;
        }
        else
        {
            submitTradeOfferButton.interactable = false;
        }
    }


    //SELECTED PLAYER--------------- �����

    public void ShowRightPlayer(Player player)
    {
        rightPlayerReference = player;

        //!!!�������� ������� ������� (�� ����������� ������)
        ClearRightPanel();
        //�������� ������ ����� (REQUEST) ��� ���������� ������

        rightOffererNameText.text = "���������� � " + rightPlayerReference.nickname;
        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);//rightCardGrid->cardPrefab +local WS
            //��������� ���������� ��������(prefab'a)
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);
            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = "����: " + rightPlayerReference.ReadMoney + "BYN";
        //�������� ������ ������� � ������
        rightOfferMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightOfferMoneySlider.value = 0;
        UpdateLeftSlider(rightOfferMoneySlider.value);//���������� �������� � �������� �������
      
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoneyText.text = "��������� ����� �������������: " + rightOfferMoneySlider.value + "BYN";
        if (rightOfferMoneySlider.value > 0)
        {
            submitTradeOfferButton.interactable = true;
        }
        else
        {
            submitTradeOfferButton.interactable = false;
        }
    }

    //MIDDLE----------------------- �����
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

        //���� ���� �������
    }
    
    //�������---------------------- �������
    void ClearAll()
    {
        rightOffererNameText.text = "����?";
        rightYourMoneyText.text = "����: ";
        rightOfferMoneySlider.maxValue = 0;
        rightOfferMoneySlider.value = 0;
        UpdateRightSlider(rightOfferMoneySlider.value);
        //CLEAR ������ ������ ������
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //CLEAR ���� - ��������
        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        //CLEAR ����� - ��������
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }
    void ClearRightPanel()
    {
        //CLEAR ����� - ��������
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
        //��������� �������� � ������
        rightOfferMoneySlider.maxValue = 0;//��������� ��������� ��������
        rightOfferMoneySlider.value = 0;
        UpdateRightSlider(rightOfferMoneySlider.value);//���������� �������� � �������� �������(�������)
    }

    //�����---------------�����---------------�����
    public void MakeOfferButton() //������������� ������ (������)
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;
        if (rightPlayerReference == null)
        {
            //ERROR ("������ ��� �������� �� ���� �������")//FIX ������ �����
            //Debug.Log("������ ��� �������� �� ���� �������");
            return;
        }
        //����� ��������� ��������
        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();//���������� ������� ��������� ������ (����� ���� �������)
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();//(�������� ����� //FIX)
        }
        //������ ��������� ��������

        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();//���������� ������� ��������� ������ (����� ���� �������)
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();//(�������� ����� //FIX)�143
        }
        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode,
            (int)leftOfferMoneySlider.value, (int)rightOfferMoneySlider.value);//������ �������������� ����
    }

    //------------------��������� ������----------------------
    void TradeResult(bool isAccepted)
    {
        
        //DESIGN
        if (isAccepted)
        {
            titleText.text = "�������������";
            fromWhoText.text = "��: " +rightPlayerReference.nickname;
            resultMessageText.text = "\"������ �������\". ������ - ��� �������.<br>" +
                " ������ ���������� � ���� <color=green>������������</color>.";
        }
        else
        {
            titleText.text = "������ �� ����������";
            fromWhoText.text = "��: " + rightPlayerReference.nickname;
            resultMessageText.text = "\"�� ���� ������ ��.. ��������, �?\" <br>� ���������, " + 
                " ������ �� ���������� � ���� <color=red>���������</color>.";
        }
        resultPanel.SetActive(true);
    }

    //------------------------�����������-----------------------(�� �� ������)
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

        //��������� ���������:
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        offeredMoney = _offeredMoney;
        requestedMoney = _requestedMoney;
        offeredNode = _offeredNode;
        requestedNode = _requestedNode;
        
        //�������� ������:
        leftCard.SetActive(false);
        rightCard.SetActive(false);
        tradeOfferPanel.SetActive(true);
        fromWhoOfferText.text = "��: " + currentPlayer.nickname;
        descriptionText.text = nodeOwner.nickname + ", � - ������������� �������, �������� �� ����������� ������ ������." +
            " � ���� ���������� ��� ������. �� �����?";
        rightRequestedMoneyText.text = "+ " + requestedMoney + "BYN";
        rightCard.SetActive(requestedNode != null ? true : false);
        leftOfferedMoneyText.text = "+ " + offeredMoney + "BYN";
        leftCard.SetActive(offeredNode != null ? true : false);
        if (leftCard.activeInHierarchy && offeredNode!=null)
        {
            leftCardPriceText.text = offeredNode.Price + "BYN";
            leftCardNameText.text = offeredNode.name;
            leftColorField.color = (offeredNode.propertyColorField!=null)?offeredNode.propertyColorField.color:Color.black;//DESIGN + ��� ICON
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
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;//DESIGN + ��� ICON
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

