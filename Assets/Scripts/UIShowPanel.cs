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
    [SerializeField] Button jailFreeChanceButton;
    [SerializeField] Button jailFreeCommunityButton;

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

    void ShowPanel(bool showPanel, bool enableRollDice, bool enableEndTurn, bool hasChanceJailFreeCard, bool hasCommunityJailFreeCard)
    {
        humanPanel.SetActive(showPanel);
        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;
        jailFreeChanceButton.gameObject.SetActive(hasChanceJailFreeCard);
        jailFreeCommunityButton.gameObject.SetActive(hasCommunityJailFreeCard);//DESIGN можно сделать так чтобы показывать только одну или обе вместе в одну
    }
    public void UpdateBalance(int money)
    {
        balanceBottom = money;
        ManageUI.instance.UpdateBottomMoneyText(money);
    }
}
