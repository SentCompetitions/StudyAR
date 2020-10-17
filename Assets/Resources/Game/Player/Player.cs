using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Vuforia;

public class Player : NetworkBehaviour
{
    public GameObject UI;
    public List<GameObject> hostOnlyObjects;

    private GameObject mainCamera;

    private void Start()
    {
        if (isLocalPlayer) mainCamera = Camera.main.gameObject;
        else UI.SetActive(false);
        if (!isServer) hostOnlyObjects.ForEach(o => o.SetActive(false));
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }

    [Command]
    public void CmdStartGame()
    {
        GameObject battery = Instantiate(GameManager.instance.batteryPrefab, Vector3.zero, Quaternion.identity,
            GameManager.instance.batteryTarget.transform);
        NetworkServer.Spawn(battery);
    }

    [Command]
    public void CmdDamage(int damage)
    {
        GameManager.instance.battery.health -= damage;
    }
}