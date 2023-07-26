using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameLobby : NetworkBehaviour
{
    [SerializeField] Button start;
    [SerializeField] GameObject redUnready;
    [SerializeField] TextMeshProUGUI readyText;
    [SerializeField] GameObject blueUnready;
    [SerializeField] GameObject redTurn;
    [SerializeField] GameObject blueTurn;
    [SerializeField] Button nextMove;

    private readonly NetworkVariable<bool> isRedReady = new NetworkVariable<bool>();
    private readonly NetworkVariable<bool> isBlueReady = new NetworkVariable<bool>();


    // Start is called before the first frame update
    void Awake()
    {
        nextMove.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isRedReady.Value&&isBlueReady.Value) 
        {
            start.enabled = true;
        }
        else
        {
            start.enabled=false;
        }

    }
    public void ReadyButtonClick()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (isRedReady.Value)
            {
                isRedReady.Value = false;
                readyText.SetText("Ready");
                redUnready.SetActive(true);
            }
            else
            {
                isRedReady.Value = true;
                readyText.SetText("Unready");
                redUnready.SetActive(false);
            }
        }
        else
        if (NetworkManager.Singleton.IsClient)
        {
            if (isBlueReady.Value)
            {
                isBlueReady.Value = false;
                readyText.SetText("Ready");
                blueUnready.SetActive(true);
            }
            else
            {
                isBlueReady.Value = true;
                readyText.SetText("Unready");
                blueUnready.SetActive(false);
            }
        }
    }
    public void StartGame()
    {
        nextMove.enabled = true;
    }
}

