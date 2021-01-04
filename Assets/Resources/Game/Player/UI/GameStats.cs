using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Mirror;
using Resources.Structs;
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

    [Header("Experience")]
    public Text experienceText;
    public string experienceString;
    public string experienceNoneString;

    void Update()
    {
        playersText.text = String.Format(playersString,  GameManager.instance.players.Count, NetworkManager.singleton.maxConnections);
        if (NetworkGameManager.instance)
        {
            if (!NetworkGameManager.instance.experience.Equals(default(Experience)))
                experienceText.text = String.Format(experienceString, NetworkGameManager.instance.experience.name);
            else
                experienceText.text = experienceNoneString;
        }


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
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && f.ToString().Contains("192.168"))
            .ToString();
    }
}
