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

    [SyncVar(hook=nameof(OnExperienceUpdate))] public Experience experience;

    void Start()
    {
        instance = this;
        onNetworkGameManagerStarted.Invoke();
        onNetworkGameManagerStarted.RemoveAllListeners();
    }

    void OnExperienceUpdate(Experience oldExp, Experience newExp)
    {
        experience = newExp;
    }
}
