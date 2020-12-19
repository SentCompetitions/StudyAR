using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TouchInputManager : MonoBehaviour
{
    public float timeFoClick = 0.2f;

    public bool IsHolding { get; private set; }

    public UnityEvent onClick;
    public UnityEvent onHolding;
    public UnityEvent onHoldStart;
    public UnityEvent onHoldEnd;

    private float _beginTime;

    void Update()
    {
        if (!IsPointerOverUIObject())
        {
            if (Input.touchCount == 1)
            {
                if (new[] {TouchPhase.Began, TouchPhase.Ended, TouchPhase.Stationary}
                    .Contains(Input.GetTouch(0).phase))
                {
                    UpdateTouchInput(Input.GetTouch(0).phase);
                }
            }
            else if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0)) UpdateTouchInput(TouchPhase.Began);
                else if (Input.GetMouseButtonUp(0)) UpdateTouchInput(TouchPhase.Ended);
                else if (Input.GetMouseButton(0)) UpdateTouchInput(TouchPhase.Stationary);
            }
        }
    }

    private void UpdateTouchInput(TouchPhase phase)
    {
        if (phase == TouchPhase.Began)
        {
            _beginTime = Time.time;
        } else if (phase == TouchPhase.Ended)
        {
            if (Time.time - _beginTime <= timeFoClick)
            {
                Debug.Log("[InputManager] Click");
                onClick?.Invoke();
            }
            else if (IsHolding)
            {
                Debug.Log("[InputManager] End holding");
                IsHolding = false;
                onHoldEnd?.Invoke();
            }
        }
        else
        {
            if (Time.time - _beginTime > timeFoClick)
            {
                if (!IsHolding)
                {
                    Debug.Log("[InputManager] Start holding");
                    IsHolding = true;
                    onHoldStart?.Invoke();
                }
                else
                {
                    Debug.Log("[InputManager] Holding");
                    onHolding?.Invoke();
                }
            }
        }
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
