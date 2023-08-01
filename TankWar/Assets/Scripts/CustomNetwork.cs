using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CustomNetwork : MonoBehaviour
{

    [SerializeField] Button startHost;
    [SerializeField] Button startServer;
    [SerializeField] Button startClient;
    [SerializeField] NetworkManager networkManager;


    // Start is called before the first frame update
    void Awake()
    {
        startHost.onClick.AddListener(() => networkManager.StartHost());
        startServer.onClick.AddListener(() => networkManager.StartServer());
        startClient.onClick.AddListener(() => networkManager.StartClient());
    }

}
