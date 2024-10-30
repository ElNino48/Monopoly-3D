using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel;//СКРЫТЬ/ПОКАЗАТЬ
    [SerializeField] Transform propertyUIGrid;//РОДИТЕЛЬСКАЯ ПРИВЯЗКА-СЕТКА
    [SerializeField] GameObject propertySetPrefab;//UI, который вставляется в propertyUIGrid где храню 0-3 карточки
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText, yourMoneyTextBottomPanel;
    [SerializeField] TMP_Text systemMessageText;
    [SerializeField] Button autoHandleFundsButton;
    [SerializeField] Button bankruptButton;

    public TMP_Text GetBottomTextPanel => yourMoneyTextBottomPanel;

    private void Awake()
    {
        instance = this;
    }

    //НАСТРОЙКИ ДО
    private void Start()
    {
        autoHandleFundsButton.interactable = false;
        bankruptButton.interactable = true;
        managePanel.SetActive(false);
    }

    public void OpenManagmentPanel()//ПРИВЯЗАТЬ К КНОПКЕ "МЕНЕДЖМЕНТ"
    {
        yourMoneyTextBottomPanel.gameObject.SetActive(true);
        playerReference = GameManager.instance.GetCurrentPlayer;
        CreateProperties();
        
        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        ClearProperties();
    }//FIX №19+

    void ClearProperties()
    {
        //ОЧИСТКА ВСЕХ КАРТОЧЕК В МЕНЕДЖМЕНТЕ ЧТОБЫ НЕ СОЗДАВАТЬ ДУБЛИКАТЫ
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)//FIX+ Проблема была в i++/ надо i--
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }//FIX №19+

    void CreateProperties()
    {
        List<MonopolyNode> processedSet = null;

        //PLAN=
        //СРАВНИТЬ: ВЛАДЕЛЕЦ = playerReference (current)
        //ЗАПОЛНИТЬ PROPERTY SETS И СОЗДАТЬ ИХ ВСЕ

        //АНАЛОГИЧНЫЙ foreach КАК И В Player CheckIfPlayerHasASet и т.д. 
        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //было List<MonopolyNode> nodeSets = list;
            //FIX Когда манипулирую nodeSets, MonopolyBoard теряет связь и информацию о карточках сета у игрока
            List<MonopolyNode> nodeSets = new List<MonopolyNode>();
            nodeSets.AddRange(list);//Теперь это копия

            if (nodeSets != null && list != processedSet)
            {

                //ЛОГИКА - СОБРАТЬ ВСЕ ПО СЕТАМ ВЗЯТЬ OWNED УБРАТЬ ТЕ ЧТО OWNED ДРУГИМИ PLAYER'амі 
                //СНАЧАЛА - ОБНОВИТЬ processedSet
                processedSet = list;
                nodeSets.RemoveAll(node => node.Owner != playerReference);//node => node.Owner дает доступ

                //СОЗДАТЬ ПРЕФАБ СО ВСЕМИ КАРТОЧКАМИ КОТОРЫЕ В НАЛИЧИИ У ИГРОКА(current)

                //propertySETprefab!!! *затуп*: +
                // + Instantiate as GameObject а не Object МОЖНО УКАЗАТЬ ЯВНО (иногда не работает падла)!!!
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyUIGrid, false);
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSets, playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? ""+playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        if (playerReference.ReadMoney < 0)
        {
            autoHandleFundsButton.interactable = true;
            bankruptButton.interactable = true;
        }
        else
        {
            autoHandleFundsButton.interactable = false;
            //bankruptButton.interactable = false;
        }
        yourMoneyText.text = "БАНК: " + showMoney + "BYN";
        yourMoneyTextBottomPanel.text = "БАНК: " + showMoney + "BYN";
    }
    
    public void UpdateBottomMoneyText(int money)
    {
        //string showMoney = (playerReference.ReadMoney >= 0) ? "" + playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyTextBottomPanel.text = "БАНК: " + money + "BYN";
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }

    public void AutoHandleFunds()//Вызывается кнопкой "Автоматич."
    {
        if (playerReference.ReadMoney > 0)
        {
            string message = "<br><br><br><color=red> Ничего не произошло. На счету положительный баланс.</color>";
            UpdateSystemMessage(message);
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));//беру по модулю поскольку число с минусом 
        //ОБНОВИТЬ UI
        ClearProperties();
        CreateProperties();
        UpdateMoneyText();
        //ОБНОВИТЬ SYSTEM MSG

    }
}
