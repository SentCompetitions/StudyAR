using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class Wire : NetworkBehaviour
{
    [Header("Color setting")]
    public WireSettings[] wireSettings;
    [Header("Debug (ReadOnly)")]
    [SyncVar(hook = nameof(SetWirePort1))]
    public WirePort wirePort1;
    [SyncVar(hook = nameof(SetWirePort2))]
    public WirePort wirePort2;

    void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color =
            wireSettings.ToList().Find(c => c.type == wirePort1.type).color;
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
        transform.localScale = new Vector3(1f,1f, dist*500);
        transform.position += transform.forward * dist/2;
    }

    public WirePort GetAnother(WirePort wirePort)
    {
        if (wirePort.element.Equals(wirePort1.element)) return wirePort2;
        if (wirePort.element.Equals(wirePort2.element)) return wirePort1;
        return default;
    }
}

[Serializable]
public struct WireSettings
{
    public string type;
    public Color color;
    public Sprite icon;
}
