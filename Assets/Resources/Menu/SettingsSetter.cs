using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public class SettingsSetter : MonoBehaviour
{
    public VuforiaBehaviour vuforia;
    [Header("Tracking Objects")]
    public Dropdown trackingMethod;
    public List<GameObject> targetMethod;
    public List<GameObject> planeMethod;

    private List<Transform> _childs;
    void Start()
    {
        _childs = new List<Transform>();
        foreach (Transform o in targetMethod.First().transform) _childs.Add(o);

        trackingMethod.value = PlayerPrefs.GetInt("TrackingMethod", 0);

        Disable(targetMethod);
        Disable(planeMethod);
        switch (PlayerPrefs.GetInt("TrackingMethod", 0))
        {
            case 0:
                Enable(targetMethod);
                SetParent(targetMethod.First().transform);
                break;
            case 1:
                Enable(planeMethod);
                SetParent(planeMethod.First().transform);
                break;
        }
    }

    public void ChangeTrackingMethod(int method)
    {
        if (PlayerPrefs.GetInt("TrackingMethod", 0) != method)
        {
            PlayerPrefs.SetInt("TrackingMethod", method);
            PlayerPrefs.Save();
            Application.Quit();
        }
    }

    private void Disable(List<GameObject> gameObjects) => gameObjects.ForEach(g => g.SetActive(false));

    private void Enable(List<GameObject> gameObjects) => gameObjects.ForEach(g => g.SetActive(true));

    private void SetParent(Transform t) => _childs.ForEach(c => c.SetParent(t));
}
