using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class MessageSystem : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    private void OnEnable()
    {
        ClearMessage();
        GameManager.OnUpdateMessage += RecieveMessage;//subscribe
        Player.OnUpdateMessage += RecieveMessage;
        MonopolyNode.OnUpdateMessage += RecieveMessage;
    }

    private void OnDisable()
    {
        GameManager.OnUpdateMessage -= RecieveMessage;//unsub
        Player.OnUpdateMessage -= RecieveMessage;
        MonopolyNode.OnUpdateMessage -= RecieveMessage;
    }

    void RecieveMessage(string _message)
    {
        messageText.text = _message;
    }

    private void ClearMessage()
    {
        messageText.text = "";
    }
}
