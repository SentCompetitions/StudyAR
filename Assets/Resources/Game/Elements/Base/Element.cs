using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Element : NetworkBehaviour
{
    [Header("Element settings")] 
    public float resistance = 10f;
    public float maxAllowedPower = .05f;
    [Header("Wires")]
    public WirePort[] wirePorts;

    [Header("Runtime values")] 
    [SyncVar] public float amperage;
    [SyncVar] public float voltage;

    private void Start()
    {
        for (var i = 0; i < wirePorts.Length; i++)
        {
            wirePorts[i].element = gameObject;
        }
    }

    public List<Wire> GetWires(string type)
    {
        var wires = FindObjectsOfType<Wire>().ToList();
        GameObject e = gameObject;
        return wires.Where(w => w.wirePort1.element.Equals(e) && w.wirePort1.type == type 
                                   || w.wirePort2.element.Equals(e) && w.wirePort2.type == type).ToList();
    }
}


[Serializable]
public struct WirePort
{
    public string type;
    public string way;
    public Transform wirePos;
    public GameObject element;

    public static WirePort GetWirePort(WirePort[] wirePorts, string type) => wirePorts.ToList().Find(w => w.type == type);
}