using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Vuforia;

public class Player : NetworkBehaviour
{
    private GameObject mainCamera;

    private void Start()
    {
        if (isLocalPlayer) mainCamera = Camera.main.gameObject;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }
}