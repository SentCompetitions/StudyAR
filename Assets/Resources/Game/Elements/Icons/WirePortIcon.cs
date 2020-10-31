using System;
using UnityEngine;
using UnityEngine.UI;

public class WirePortIcon : MonoBehaviour
{
    GameObject mainCamera = GameManager.instance.localPlayer.mainCamera;
    public Transform target;
    public Image image;
    Player player = GameManager.instance.localPlayer;
    public float size = 1f;

    void Update()
    {
        transform.position = mainCamera.GetComponent<Camera>().WorldToScreenPoint(target.position);

        if (!image || !player) return;
        float distance = Vector3.Distance(target.transform.position, player.transform.position);
        if (distance > 0.5f)
        {
            image.rectTransform.sizeDelta = new Vector2(0, 0);//OPTIMIZATION Change the code to more optimized
            return;
        }
        float resolution = 3f + (float)Math.Pow(0.21f * Screen.dpi, 1.05f);
        resolution *= size;
        if (resolution == 0) resolution = 25f;
        resolution = resolution / (0.1f + distance * 7);
        image.rectTransform.sizeDelta = new Vector2(resolution, resolution);
    }
}
