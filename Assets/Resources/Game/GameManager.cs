using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject batteryPrefab;
    public GameObject batteryTarget;

    public List<Player> players = new List<Player>();
    public Player localPlayer;

    public Battery battery;

    void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            players.Clear();
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
            {
                if (kvp.Value.TryGetComponent(out Player comp))
                {
                    players.Add(comp);
                    if (comp.isLocalPlayer) localPlayer = comp;
                }
            }
        }
        else
        {
            players.Clear();
        }
    }
}