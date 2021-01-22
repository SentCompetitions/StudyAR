using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Led : PhysicsElement
{
    [Header("Led")]
    public GameObject lightObject;

    public Sprite warningSprite;
    private GameObject icon;

    private bool _warning;
    public bool warning
    {
        get { return _warning; }
        set
        {
            if (!_warning & value) Warning();
            else if (_warning & !value) Destroy(icon);
            _warning = value;
        }
    }

    void Update()
    {
        float intensity = amperage * voltage / maxAllowedPower * 3;
        lightObject.transform.localScale = new Vector3(intensity, intensity, intensity);
        if (amperage * voltage > maxAllowedPower) warning = true;
        else warning = false;
    }

    void Warning()
    {
        icon = new GameObject("Warning");
        icon.transform.SetParent(GameManager.instance.localPlayer.UI.transform);
        var image = icon.AddComponent<Image>();
        image.sprite = warningSprite;
        var script = icon.AddComponent<WirePortIcon>();
        script.image = image;
        script.target = transform;
        script.size = 1.5f;
    }
}
