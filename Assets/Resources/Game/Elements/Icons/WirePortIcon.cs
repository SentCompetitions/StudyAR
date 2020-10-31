using System;
using UnityEngine;
using UnityEngine.UI;

public class WirePortIcon : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target;
    public Image image;
    public Player player;

    void Update()
    {
        transform.position = mainCamera.WorldToScreenPoint(target.position);

        if (!image || !player) return;
        float distance = Vector3.Distance(target.transform.position, player.transform.position);
        if (distance > 0.5f)
        {
            image.rectTransform.sizeDelta = new Vector2(0, 0);//OPTIMIZATION Change the code to more optimized
            return;
        }
        float resolution = 3f + (float)Math.Pow(0.21f * Screen.dpi, 1.05f);
        if (resolution == 0) resolution = 25f;
        resolution = resolution / (0.1f + distance * 7);
        image.rectTransform.sizeDelta = new Vector2(resolution, resolution);
    }
}
