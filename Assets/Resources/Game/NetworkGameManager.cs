using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Resources.Structs;
using UnityEngine;
using UnityEngine.Events;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager instance;
    public static UnityEvent onNetworkGameManagerStarted = new UnityEvent();

    [SyncVar] public Experience experience;

    void Start()
    {
        instance = this;
        onNetworkGameManagerStarted.Invoke();
        onNetworkGameManagerStarted.RemoveAllListeners();
    }
}
