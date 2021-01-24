using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Resources.Structs;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Scene setup")]
    public GameObject mainTarget;

    public Transform elementsParent;
    public Transform[] elementsSpawnPoints;

    [Header("Multiplayer scene prepair")]
    public List<GameObject> inMenu;
    public List<GameObject> inGame;

    [Header("Multiplayer (Readonly)")]
    public List<Player> players = new List<Player>();
    public Player localPlayer;
    private bool _isGameStarted = false;
    public bool IsGameStarted
    {
        get => _isGameStarted;
        set
        {
            if (value)
            {
                inGame.ForEach(x => x.SetActive(true));
                inMenu.ForEach(x => x.SetActive(false));
            }
            else
            {
                inGame.ForEach(x => x.SetActive(false));
                inMenu.ForEach(x => x.SetActive(true));
            }
            _isGameStarted = value;
        }
    }


    [Header("Loaded packs")]
    public Pack[] packs;

    void Start()
    {
        instance = this;
        inGame.ForEach(x => x.SetActive(false));
        inMenu.ForEach(x => x.SetActive(true));
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