using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WirePortIcon : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target;
    void Update()
    {
        transform.position = mainCamera.WorldToScreenPoint(target.position);
    }
}
