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

    private int _maxConnections;

    NetworkManager manager;
    Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private bool _isHost;

    void Start()
    {
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
        discoveredServers[info.serverId] = info;
        ip.text = info.uri.Host;
        // SetServers();
    }

    public void Host()
    {
        manager.StartHost();
        networkDiscovery.AdvertiseServer();
        _isHost = true;
        NetworkGameManager.onNetworkGameManagerStarted.AddListener(SetExperience);
    }

    private void SetExperience()
    {
        NetworkGameManager.instance.experience = default;
    }

    private void Client(Uri uri)
    {
        _isHost = false;
        manager.StartClient(uri);
    }

    public void Client(InputField ip)
    {
        manager.networkAddress = ip.text;
        _isHost = false;
        manager.StartClient();
    }

    public void Exit()
    {
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
    }
}
