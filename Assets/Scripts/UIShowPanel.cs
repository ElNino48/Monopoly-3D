using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIShowPanel : MonoBehaviour
{

    public static UIShowPanel instance;

    [SerializeField] GameObject humanPanel;
    [SerializeField] Button rollDiceButton;
    [SerializeField] Button endTurnButton;
    [SerializeField] int balanceBottom;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnShowHumanPanel += ShowPanel;
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        ChanceField.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;

        //GameManager.OnUpdateBottomBalance += UpdateBalance;
    }

    private void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        ChanceField.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;

        //GameManager.OnUpdateBottomBalance -= UpdateBalance;
    }

    void ShowPanel(bool showPanel, bool enableRollDice, bool enableEndTurn)
    {
        humanPanel.SetActive(showPanel);
        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;
    }
    public void UpdateBalance(int money)
    {
        balanceBottom = money;
        ManageUI.instance.UpdateBottomMoneyText(money);
    }
}
