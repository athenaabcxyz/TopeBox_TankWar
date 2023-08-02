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


    public override void OnNetworkDespawn()
    {
        chatBox.text = "";
        base.OnNetworkDespawn();
    }

    public void Start()
    {
        chatBox.text = "";
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CallMessageRPC();
        }
    }
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
        GetCurrentUser();
        string getMessage = message.text;
        SendMessageServerRPC(currentUser, getMessage);
    }

    [ClientRpc]
    public void SendMessageClientRpc(string currentUser, string message)
    {
        Addtext(currentUser, message);
        chatBox.text = "";
    }
    [ServerRpc(RequireOwnership =false)]
    public void SendMessageServerRPC(string currentUser, string message)
    {       
        SendMessageClientRpc(currentUser, message);
    }
    public void Addtext(string currentUser, string message)
    {
        if (currentUser == "Red")
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
