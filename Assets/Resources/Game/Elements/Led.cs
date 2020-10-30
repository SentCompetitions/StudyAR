using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Led : Element
{
    [Header("Led")]
    public Light light;

    void Update()
    {
        light.intensity = amperage * voltage / maxAllowedPower * 10;
    }
}
