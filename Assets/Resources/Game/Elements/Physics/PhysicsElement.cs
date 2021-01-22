using Mirror;
using UnityEngine;

public class PhysicsElement: Element
{
    [Header("Physics Element Settings")]
    public float resistance = 10f;
    public float maxAllowedPower = .05f;

    [Header("Runtime Values")]
    [SyncVar] public float amperage;
    [SyncVar] public float voltage;
}