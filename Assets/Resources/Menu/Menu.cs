using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using Resources.Structs;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class Menu : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;

    [Header("Main menu")]
    public GameObject mainMenu;
    public GameObject serversList;
    public GameObject serverButtonPrefab;

    [Header("Connection")]
    public InputField ip;

    public static Menu instance;

    private int _maxConnections;

    NetworkManager manager;
    Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private bool _isHost;
    private static readonly int Game = Animator.StringToHash("Game");

    void Start()
    {
        instance = this;
        manager = NetworkManager.singleton;
        _maxConnections = manager.maxConnections;
        FindServers();
    }

    private void DestroyServers()
    {
        for (int i = 0; i < serversList.transform.childCount; i++)
        {
            Destroy(serversList.transform.GetChild(i).gameObject);
        }
    }

    private void SetServers()
    {
        DestroyServers();
        foreach (var server in discoveredServers)
        {
            GameObject goButton = Instantiate(serverButtonPrefab);
            goButton.transform.SetParent(serversList.transform, false);
            goButton.GetComponentInChildren<Text>().text = $"{server.Value.uri.Host}";
            Button tempButton = goButton.GetComponent<Button>();
            tempButton.onClick.AddListener(() => Client(server.Value.uri));
        }
    }

    public void FindServers()
    {
        // DestroyServers();
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        if (!discoveredServers.Contains(new KeyValuePair<long, ServerResponse>(info.serverId, info)) && !manager.isNetworkActive)
        {
            Debug.Log($"[Network] Discovered server {info.uri.Host}");
            discoveredServers[info.serverId] = info;
            ip.text = info.uri.Host;
            Uri temp = info.uri;
            AlertManager.ShowAlert(new Alert
            {
                title = "Найден хост в локальной сети",
                text = $"IP: {info.uri.Host}",
                buttonText = "Подключиться",
                onButtonClick = delegate { Client(temp); },
                hidePanel = true
            });
            // SetServers();
        }
    }

    public void Host()
    {
        manager.StartHost();
        networkDiscovery.AdvertiseServer();
        _isHost = true;
        NetworkGameManager.OnNetworkGameManagerStarted.AddListener(SetExperience);
    }

    private void SetExperience()
    {
        NetworkGameManager.instance.experience = default;
    }

    private void Client(Uri uri)
    {
        manager.networkAddress = uri.Host;
        manager.StartClient(uri);
        Client();
    }

    public void Client(InputField ip)
    {
        manager.networkAddress = ip.text;
        manager.StartClient();
        Client();
    }

    private void Client()
    {
        GetComponent<Animator>().SetTrigger(Game);
        _isHost = false;
        discoveredServers.Clear();
        Debug.Log("[Client] Start client");
    }

    public void Exit()
    {
        GetComponent<Animator>().SetTrigger(Game);

        if (_isHost) manager.StopHost();
        else manager.StopClient();
        manager.maxConnections = _maxConnections;
        GameManager.instance.IsGameStarted = false;

        if (!NetworkGameManager.instance.experience.Equals(default(Experience)))
        {
            for (var i = 0; i < NetworkGameManager.instance.experience.actions.Length; i++)
            {
                NetworkGameManager.instance.experience.actions[i].isCompleted = false;
            }
        }
        NetworkGameManager.instance.experience = default;

        Debug.Log("[Client] Disconnected from server");
    }
}
