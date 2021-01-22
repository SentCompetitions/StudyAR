using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Battery : PhysicsElement
{
    [Header("Source settings")]
    public float maxAmperage;
    public float targetVoltage;
}
