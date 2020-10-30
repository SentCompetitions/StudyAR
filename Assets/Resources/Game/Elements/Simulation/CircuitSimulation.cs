﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CircuitSimulation : MonoBehaviour
{
    public static UnityEvent UpdateSimulationEvent;

    private float allResistance;
    private List<Element> _elements;
    private Battery source;
    void Start()
    {
        UpdateSimulationEvent = new UnityEvent();
        UpdateSimulationEvent.AddListener(UpdateSimulation);
    }

    private void UpdateSimulation()
    {
        Debug.Log("[CIRCUIT] Starting update...");
        StopAllCoroutines();
        StartCoroutine(UpdateSimulationCoroutine());
    }

    private IEnumerator UpdateSimulationCoroutine()
    {
        source = FindObjectsOfType<Battery>().First();
        _elements = new List<Element>();
        _elements.Clear();
        
        StepInto(WirePort.GetWirePort(source.wirePorts, "+"));

        yield break;
    }

    private void StepInto(WirePort wirePort)
    {
        var sub = wirePort.element.GetComponent<Element>();

        if (!(sub.Equals(source) && wirePort.type == "-"))
        {
            allResistance += sub.resistance;
            _elements.Add(sub);
            Debug.Log(wirePort.type);
            try
            {
                if (string.IsNullOrEmpty(wirePort.way))
                    throw new NotFullCircuit("No way");
                var wires = sub.GetWires(wirePort.way);
                if (wires.Count > 0)
                    StepInto(wires.First().GetAnother(wirePort));
                else
                    throw new NotFullCircuit("No wires");
            }
            catch (NotFullCircuit e)
            {
                Debug.Log("[CIRCUIT] " + e);
                allResistance = 0f;
                SetElementState();
            }
        }
        else
        {
            SetElementState();
        }
    }

    private void SetElementState()
    {
        if (allResistance != 0f)
        {
            float apmerage = source.maxVoltage / allResistance;
            Debug.Log($"[CIRCUIT] Result resistance: {allResistance}");
            Debug.Log($"[CIRCUIT] Result amperage: {apmerage}");
            _elements.ForEach(e => e.amperage = apmerage);
        }
        else
        {
            foreach (var e in _elements)
            {
                e.amperage = 0f;
                e.voltage = source.maxVoltage;
            }
        }
    }
}

public class NotFullCircuit : Exception
{
    public NotFullCircuit(string message) : base(message) { }
}