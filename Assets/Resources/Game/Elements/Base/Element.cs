using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Element : NetworkBehaviour
{
    public WirePort[] wirePorts;
}


[Serializable]
public struct WirePort
{
    public string type;
    public Transform wirePos;
    public GameObject element;
}