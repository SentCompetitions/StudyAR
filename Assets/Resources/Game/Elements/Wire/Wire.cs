using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Wire : NetworkBehaviour
{
    [Header("Color setting")]
    public WireColor[] colors;
    [Header("Debug (ReadOnly)")]
    [SyncVar(hook = nameof(SetWirePort1))]
    public WirePort wirePort1;
    [SyncVar(hook = nameof(SetWirePort2))]
    public WirePort wirePort2;

    void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color =
            colors.ToList().Find(c => c.type == wirePort1.type).color;
    }

    void SetWirePort1(WirePort old, WirePort wirePort)
    {
        wirePort1.wirePos
            = wirePort1.element.GetComponent<Element>().wirePorts.ToList().Find(w => w.type == wirePort1.type).wirePos;
    }

    void SetWirePort2(WirePort old, WirePort wirePort)
    {
        wirePort2.wirePos
            = wirePort2.element.GetComponent<Element>().wirePorts.ToList().Find(w => w.type == wirePort2.type).wirePos;
    }

    void Update()
    {
        Vector3 pos1 = wirePort1.wirePos.position;
        Vector3 pos2 = wirePort2.wirePos.position;

        transform.position = pos1;
        transform.LookAt(pos2);

        float dist = Vector3.Distance(pos1, pos2);
        Debug.Log(dist);
        transform.localScale = new Vector3(1f,1f, dist*5);
        transform.position += transform.forward * dist/2;
    }
}

[Serializable]
public struct WireColor
{
    public string type;
    public Color color;
}
