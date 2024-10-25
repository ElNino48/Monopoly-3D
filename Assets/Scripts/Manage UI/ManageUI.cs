using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel;//ÑÊĞÛÒÜ/ÏÎÊÀÇÀÒÜ
    [SerializeField] Transform propertyUIGrid;//ĞÎÄÈÒÅËÜÑÊÀß ÏĞÈÂßÇÊÀ-ÑÅÒÊÀ
    [SerializeField] GameObject propertySetPrefab;//UI, êîòîğûé âñòàâëÿåòñÿ â propertyUIGrid ãäå õğàíş 0-3 êàğòî÷êè
    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();
    [SerializeField] TMP_Text yourMoneyText, yourMoneyTextBottomPanel;

    private void Awake()
    {
        instance = this;
    }

    //ÍÀÑÒĞÎÉÊÈ ÄÎ
    private void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManagmentPanel()//ÏĞÈÂßÇÀÒÜ Ê ÊÍÎÏÊÅ "ÌÅÍÅÄÆÌÅÍÒ"
    {
        playerReference = GameManager.instance.GetCurrentPlayer;
        //ÏÎËÓ×ÈÒÜ ÂÑÅ ÊÀĞÒÎ×ÊÈ Â ÊÀ×ÅÑÒÂÅ ÑÅÒÎÂ ÊÀĞÒÎ×ÅÊ
        List<MonopolyNode> processedSet = null;

        //PLAN=
        //ÑĞÀÂÍÈÒÜ: ÂËÀÄÅËÅÖ = playerReference (current)
        //ÇÀÏÎËÍÈÒÜ PROPERTY SETS È ÑÎÇÄÀÒÜ ÈÕ ÂÑÅ

        //ÀÍÀËÎÃÈ×ÍÛÉ foreach ÊÀÊ È Â Player CheckIfPlayerHasASet è ò.ä. 
        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.instance.PlayerHasAllNodesOfSet(node);
            //áûëî List<MonopolyNode> nodeSets = list;
            //FIX Êîãäà ìàíèïóëèğóş nodeSets, MonopolyBoard òåğÿåò ñâÿçü è èíôîğìàöèş î êàğòî÷êàõ ñåòà ó èãğîêà
            List<MonopolyNode> nodeSets = new List<MonopolyNode>();
            nodeSets.AddRange(list);//Òåïåğü ıòî êîïèÿ

            if (nodeSets != null && list != processedSet)
            {

                //ËÎÃÈÊÀ - ÑÎÁĞÀÒÜ ÂÑÅ ÏÎ ÑÅÒÀÌ ÂÇßÒÜ OWNED ÓÁĞÀÒÜ ÒÅ ×ÒÎ OWNED ÄĞÓÃÈÌÈ PLAYER'àì³ 
                //ÑÍÀ×ÀËÀ - ÎÁÍÎÂÈÒÜ processedSet
                processedSet = list;
                nodeSets.RemoveAll(node => node.Owner != playerReference);//node => node.Owner äàåò äîñòóï

                //ÑÎÇÄÀÒÜ ÏĞÅÔÀÁ ÑÎ ÂÑÅÌÈ ÊÀĞÒÎ×ÊÀÌÈ ÊÎÒÎĞÛÅ Â ÍÀËÈ×ÈÈ Ó ÈÃĞÎÊÀ(current)

                //propertySETprefab!!! *çàòóï*: +
                // + Instantiate as GameObject à íå Object ÌÎÆÍÎ ÓÊÀÇÀÒÜ ßÂÍÎ (èíîãäà íå ğàáîòàåò ïàäëà)!!!
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyUIGrid, false);
                newPropertySet.GetComponent<ManagePropertyUI>().SetProperty(nodeSets,playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
        //ÑÍÀ×ÀËÀ ÇÀÏÎËÍÈÒÜ - ÇÀÒÅÌ ÎÒÊĞÛÒÜ ÏÀÍÅËÜ
        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        //Î×ÈÑÒÊÀ ÂÑÅÕ ÊÀĞÒÎ×ÅÊ Â ÌÅÍÅÄÆÌÅÍÒÅ ×ÒÎÁÛ ÍÅ ÑÎÇÄÀÂÀÒÜ ÄÓÁËÈÊÀÒÛ
        for (int i = propertyPrefabs.Count-1; i >=0; i--)//FIX+ Ïğîáëåìà áûëà â i++/ íàäî i--
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }//FIX ¹19

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? ""+playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyText.text = "ÁÀÍÊ: " + showMoney + "BYN";
        yourMoneyTextBottomPanel.text = "ÁÀÍÊ: " + showMoney + "BYN";
    }
    
    public void UpdateBottomMoneyText(int money)
    {

        Debug.Log(money + "= money 1TYT");
        //string showMoney = (playerReference.ReadMoney >= 0) ? "" + playerReference.ReadMoney : "<color=red>" + playerReference.ReadMoney;
        yourMoneyTextBottomPanel.text = "ÁÀÍÊ: " + money + "BYN";
    }

}
