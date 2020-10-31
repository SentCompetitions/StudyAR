using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Led : Element
{
    [Header("Led")]
    public GameObject lightObject;

    void Update()
    {
        float intensity = amperage * voltage / maxAllowedPower * 3;
        lightObject.transform.localScale = new Vector3(intensity, intensity, intensity);
    }
}
