using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Resources.Game;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Scene setup")]
    public GameObject mainTarget;

    public Transform elementsParent;
    public Transform[] elementsSpawnPoints;

    [Header("Multiplayer (Readonly)")]
    public List<Player> players = new List<Player>();
    public Player localPlayer;
    public bool isGameStarted = false;

    [Header("Experiences")]
    public Experience experience;

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