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

    [Header("Scale")]
    public Slider scaleSlider;
    public Transform objectToScale;

    private List<Transform> _childs;
    void Start()
    {
        _childs = new List<Transform>();
        foreach (Transform o in targetMethod.First().transform) _childs.Add(o);

        trackingMethod.value = PlayerPrefs.GetInt("TrackingMethod", 0);
        ChangeTrackingMethod(PlayerPrefs.GetInt("TrackingMethod", 0));

        scaleSlider.value = PlayerPrefs.GetFloat("Scale", 0.01f);
        ChangeScale();
    }

    public void ChangeTrackingMethod(int method)
    {
        PlayerPrefs.SetInt("TrackingMethod", method);
        PlayerPrefs.Save();

        planeMethod.Last().SetActive(false);

        switch (PlayerPrefs.GetInt("TrackingMethod", 0))
        {
            case 0:
                SetParent(targetMethod.First().transform);
                break;
            case 1:
                planeMethod.Last().SetActive(true);
                SetParent(planeMethod.First().transform);
                break;
        }
    }

    private void SetParent(Transform t) => _childs.ForEach(c =>
    {
        foreach (Transform tr in _childs)
        {
            tr.position = Vector3.zero;
            tr.rotation = Quaternion.identity;
        }
        c.SetParent(t);
    });

    public void ChangeScale()
    {
        PlayerPrefs.SetFloat("Scale", scaleSlider.value);
        PlayerPrefs.Save();

        objectToScale.localScale = new Vector3(scaleSlider.value, scaleSlider.value, scaleSlider.value);
    }
}
