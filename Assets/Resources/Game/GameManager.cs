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

    public Battery battery;

    void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (NetworkManager.singleton.isNetworkActive)
        {
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkIdentity.spawned)
            {
                Player comp = kvp.Value.GetComponent<Player>();
                if (!players.Contains(comp)) players.Add(comp);
            }
        }
        else
        {
            players.Clear();
        }
    }
}