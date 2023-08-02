using System.Collections;
using System.Collections.Generic;
using TMPro;
using Topebox.Tankwars;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameLobby : NetworkBehaviour
{
    [SerializeField] Button start;
    [SerializeField] GameObject redUnready;
    [SerializeField] TextMeshProUGUI readyText;
    [SerializeField] TextMeshProUGUI redPlayer;
    [SerializeField] TextMeshProUGUI bluePlayer;
    [SerializeField] GameObject blueUnready;
    [SerializeField] Button nextMove;
    [SerializeField] Button readyButton;
    [SerializeField] TextMeshProUGUI startButtonText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameState gamestate;

    bool isRedReadyClient = false;
    bool isBlueReadyClient = false;
    bool isTheGameStarted = false;

    private readonly NetworkVariable<bool> isRedReady = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> isBlueReady = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);


    // Start is called before the first frame update


    private void Awake()
    {
        nextMove.gameObject.SetActive(false);
    }
    public override void OnNetworkSpawn()
    {
        statusText.SetText("Host or join to play.");
        if (IsOwner)
        {
            redPlayer.SetText("You");
            bluePlayer.SetText("Opponent");
            statusText.SetText("Wait for players to ready.");

        }
        else
        {
            redPlayer.SetText("Opponent");
            bluePlayer.SetText("You");
            statusText.SetText("Wait for players to ready.");
        }
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTheGameStarted)
        {
            if (isRedReady.Value && isBlueReady.Value)
            {
                start.enabled = true;
                if (NetworkManager.Singleton.IsHost)
                {
                    startButtonText.SetText("Start");
                }
                else
                {
                    startButtonText.SetText("Wait for Host");
                    start.enabled = false;
                }

                statusText.SetText("Wait for game to start.");
            }
            else
            {
                start.enabled = false;

                startButtonText.SetText("Waiting");
                statusText.SetText("Wait for players to ready.");
            }
            if (IsOwner)
            {
                isBlueReady.Value = isBlueReadyClient;
                isRedReady.Value = isRedReadyClient;
            }
            else
            {
                isRedReadyClient = isRedReady.Value;
                isBlueReadyClient = isBlueReady.Value;
            }
            redUnready.SetActive(!isRedReadyClient);
            blueUnready.SetActive(!isBlueReadyClient);
            if (NetworkManager.Singleton.IsHost)
            {
                if (isRedReadyClient)
                {
                    readyText.SetText("Unready");
                }
                else
                {
                    readyText.SetText("Ready");
                }
            }
            else
            {
                if (isBlueReadyClient)
                {
                    readyText.SetText("Unready");
                }
                else
                {
                    readyText.SetText("Ready");
                }
            }
        }
        
    }

    public void ReadyButtonClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (isRedReadyClient)
            {
                isRedReadyClient = false;
                readyText.SetText("Ready");
            }
            else
            {
                isRedReadyClient = true;
                readyText.SetText("Unready");
            }
        }
        else
        {
            SubmitBlueReadyOnLickServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitBlueReadyOnLickServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isBlueReadyClient)
        {
            isBlueReadyClient = false;
        }
        else
        {
            isBlueReadyClient = true;
        }
    }
    public void StartGame()
    {
        gamestate.StartGame();
        StartGameServerRPC();
    }

    [ServerRpc(RequireOwnership =false)]
    public void StartGameServerRPC()
    {
        StartGameClientRPC();
    }

    [ClientRpc]
    public void StartGameClientRPC()
    {
        isTheGameStarted = true;
        readyButton.gameObject.SetActive(false);
        start.enabled = false;
        startButtonText.SetText("Game started");
    }

}

