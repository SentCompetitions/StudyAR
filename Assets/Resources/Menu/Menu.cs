using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    [Header("Main menu")]
    public GameObject mainMenu;
    public GameObject serversList;
    public GameObject serverButtonPrefab;

    NetworkManager manager;
    Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

    private bool _isHost;

    void Start()
    {
        manager = NetworkManager.singleton;
        FindServers();
    }

    void Update()
    {
        
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
        DestroyServers();
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        discoveredServers[info.serverId] = info;
        SetServers();
    }

    public void Host()
    {
        manager.StartHost();
        networkDiscovery.AdvertiseServer();
        _isHost = true;
        ToggleMenu(mainMenu);
    }

    private void Client(Uri uri)
    {
        _isHost = false;
        manager.StartClient(uri);
        ToggleMenu(mainMenu);
    }

    public void Client(InputField ip)
    {
        manager.networkAddress = ip.text;
        _isHost = false;
        manager.StartClient();
        ToggleMenu(mainMenu);
    }

    private void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }
}
