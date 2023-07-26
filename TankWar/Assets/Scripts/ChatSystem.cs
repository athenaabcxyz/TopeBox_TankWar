using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ChatSystem : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI chatBox;
    [SerializeField] TMP_InputField message;
    string currentUser;

    public void GetCurrentUser()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            currentUser = "Red";
        }
        else
        if(NetworkManager.Singleton.IsClient)
        {
            currentUser = "Blue";
        }
    }

    public void CallMessageRPC()
    {
        string getMessage = message.text;
        SendMessageServerRPC(currentUser, getMessage);
    }
    [ServerRpc]
    public void SendMessageServerRPC(string currentUser, string message)
    {
        if(currentUser=="Red")
        {
            //$"<color=red>{ScoreRed}</color> - <color=blue>{ScoreBlue}</color>";
            chatBox.text += $"<color=red>{currentUser}</color>: {message}\n";
        }
        else
        {
            chatBox.text += $"<color=blue>{currentUser}</color>: {message}\n";
        }
        
    }
}
