using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CircuitSimulation : MonoBehaviour
{
    public static UnityEvent UpdateSimulationEvent;
    void Start()
    {
        UpdateSimulationEvent = new UnityEvent();
        UpdateSimulationEvent.AddListener(UpdateSimulation);
    }

    private void UpdateSimulation()
    {
        Debug.Log("Starting update...");
        StopAllCoroutines();
        StartCoroutine(UpdateSimulationCoroutine());
    }

    private IEnumerator UpdateSimulationCoroutine()
    {
        // FindObjectOfType(typeof(ITransform))
        yield return null;
    }
}
