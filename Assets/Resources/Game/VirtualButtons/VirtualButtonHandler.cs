using UnityEngine;
using UnityEngine.Events;
using Vuforia;

[RequireComponent(typeof(VirtualButtonBehaviour))]
public class VirtualButtonHandler : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    [Header("View")]
    public GameObject buttonObject;

    public Color pressedColor = Color.black;
    public Color releasedColor = Color.gray;

    void Start()
    {
        GetComponent<VirtualButtonBehaviour>().RegisterOnButtonPressed(OnVirtualButtonPressed);
        GetComponent<VirtualButtonBehaviour>().RegisterOnButtonReleased(OnVirtualButtonReleased);

        buttonObject.GetComponent<MeshRenderer>().material.color = releasedColor;
    }

    void OnVirtualButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Virtual button {name} pressed");

        buttonObject.transform.Translate(Vector3.down * 0.5f);
        buttonObject.GetComponent<MeshRenderer>().material.color = pressedColor;

        onPressed.Invoke();
    }

    void OnVirtualButtonReleased(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Virtual button {name} released");

        buttonObject.transform.Translate(Vector3.up * 0.5f);
        buttonObject.GetComponent<MeshRenderer>().material.color = releasedColor;

        onReleased.Invoke();
    }

    private void OnValidate()
    {
        // buttonObject.GetComponent<MeshRenderer>().material.color = releasedColor;
    }
}
