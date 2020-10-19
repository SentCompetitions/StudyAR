using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameStats : MonoBehaviour
{
    [Header("IP")]
    public Text ipText;
    public string ipString;
    [Header("Players")]
    public Text playersText;
    public string playersString;

    void Update()
    {
        playersText.text = String.Format(playersString,  GameManager.instance.players.Count, NetworkManager.singleton.maxConnections);

        if (GameManager.instance.localPlayer)
        {
            string ip = GameManager.instance.localPlayer.isServer
                ? GetLocalIPv4()
                : NetworkManager.singleton.networkAddress;
            ipText.text = String.Format(ipString, ip);
        }
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}
