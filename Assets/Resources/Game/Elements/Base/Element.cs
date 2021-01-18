using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mirror;
using Resources.Structs;
using UnityEngine;

[SelectionBase]
public class Element : NetworkBehaviour
{
    [Header("Element settings")] 
    public float resistance = 10f;
    public float maxAllowedPower = .05f;
    [Header("Wires")]
    public WirePort[] wirePorts;

    [Header("Element Properties")]
    [SyncVar(hook=nameof(OnElementPropertiesUpdate))] public ElementProperties elementProperties;

    [Header("Runtime values")] 
    [SyncVar] public float amperage;
    [SyncVar] public float voltage;

    private void Start()
    {
        for (var i = 0; i < wirePorts.Length; i++)
        {
            wirePorts[i].element = gameObject;
        }

        if (isServer)
        {
            OnElementPropertiesUpdate(default, elementProperties);
        }
    }

    private void OnElementPropertiesUpdate(ElementProperties oldProperties, ElementProperties newProperties)
    {
        elementProperties = newProperties;
        foreach (var elementProperty in elementProperties.propertiesArray)
        {
            FieldInfo info = this.GetType().GetField(elementProperty.name);
            info.SetValue(this, Convert.ChangeType(elementProperty.value, info.GetValue(this).GetType()));
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