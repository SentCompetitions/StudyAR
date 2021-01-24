using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Resources.Game.ExperienceProcessor;
using Resources.Structs;
using UnityEngine;
using UnityEngine.Events;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager instance;
    public static readonly UnityEvent OnNetworkGameManagerStarted = new UnityEvent();
    public static readonly UnityEvent OnGameStarted = new UnityEvent();

    [SyncVar] public Experience experience;
    public List<ExperienceProcessor> experienceProcessors;

    public ExperienceProcessor processor;

    void Start()
    {
        instance = this;
        OnGameStarted.AddListener(OnGameStartedEvent);

        foreach (var experienceProcessor in experienceProcessors)
        {
            experienceProcessor.enabled = false;
        }

        OnNetworkGameManagerStarted.Invoke();
        OnNetworkGameManagerStarted.RemoveAllListeners();
    }

    private void OnGameStartedEvent()
    {
        processor = experienceProcessors.Find(p => p.GetExperienceType() == experience.Pack.subject);
        processor.enabled = true;
        Debug.Log("[NetGameManager] Selected " + processor.GetType().Name);
    }
}
