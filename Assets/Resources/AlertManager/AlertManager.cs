using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlertManager : MonoBehaviour
{
    public GameObject alertBox;

    public GameObject panel;

    public Text titleText;
    public Text alertText;
    public Text mainButtonText;

    public Button closeButton;
    public Button mainButton;

    private Alert _currentAlert;
    private Queue<Alert> _alerts;

    private static AlertManager instance;
    private static readonly int Show = Animator.StringToHash("Show");

    private void Start()
    {
        instance = this;
        _alerts = new Queue<Alert>();
        closeButton.onClick.AddListener(delegate { Close(); });
        Disable();
    }

    private void Update()
    {
        if (_currentAlert != null && _currentAlert.hidePanel && !_currentAlert.hideClose && (Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)))
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            if (results.Count(result => result.gameObject == alertBox) == 0)
            {
                Close();
            }
        }
    }

    public static void ShowAlert(Alert alert)
    {
        instance._alerts.Enqueue(alert);
        if (instance._currentAlert == null)
            instance.ShowNextAlert();
    }

    private void ShowNextAlert()
    {
        try
        {
            Alert alert = _alerts.Dequeue();
            _currentAlert = alert;

            titleText.text = alert.title;
            alertText.text = alert.text;
            closeButton.interactable = !alert.hideClose;
            panel.SetActive(!alert.hidePanel);
            mainButton.gameObject.SetActive(alert.buttonText != null);
            mainButtonText.text = alert.buttonText;
            mainButton.onClick.RemoveAllListeners();
            if (alert.onButtonClick != null) mainButton.onClick.AddListener(delegate { alert.onButtonClick(); Close(); });

            GetComponent<Animator>().SetBool(Show, true);

            Debug.Log($"[AlertManager] Alert shown (Title: {alert.title})\nDescription: {alert.text}", gameObject);
        }
        catch (InvalidOperationException) { }
    }

    public void ToggleState()
    {
        if (instance.GetComponent<Animator>().GetBool(Show))
        {
            Enable();
        }
        else
        {
            if (_alerts.Count == 0) Disable();
            ShowNextAlert();
        }
    }

    public void Disable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void Enable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void Close()
    {
        GetComponent<Animator>().SetBool(Show, false);
        _currentAlert = null;
    }
}

public class Alert
{
    public string title;
    public string text;
    public string buttonText;
    public bool hidePanel;
    public bool hideClose;

    public delegate void OnButtonClick();
    public OnButtonClick onButtonClick;
}
