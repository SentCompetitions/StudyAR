using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = System.Object;

public class Player : NetworkBehaviour
{
    [Header("UI")]
    public GameObject UI;
    public List<GameObject> hostOnlyObjects;
    public List<GameObject> clientOnlyObjects;
    [Space]
    public Image pointer;
    private Sprite selectPointer;
    public Sprite movePointer;
    public Sprite connectPointer;
    public Sprite errorPointer;
    [Space]
    public float timeForClick = 0.1f;
    private float _clickTime = 0f;
    private bool _isHold;

    [Header("Debug (Readonly)")] public Element grabbedElement;

    private GameObject mainCamera;
    private GameManager _manager = GameManager.instance;

    private Sprite _newPointer;

    public Sprite NewPointer
    {
        get => _newPointer;
        private set
        {
            UI.GetComponent<Animator>().SetTrigger("ChangePointer");
            _newPointer = value;
        }
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            mainCamera = Camera.main.gameObject;
            selectPointer = pointer.sprite;
        }
        else UI.SetActive(false);

        if (!isServer) hostOnlyObjects.ForEach(o => o.SetActive(false));
        else clientOnlyObjects.ForEach(o => o.SetActive(false));
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;

        if (!_manager.IsGameStarted) return;

        if (Input.touchCount == 1 && !IsPointerOverUIObject())
        {
            Debug.Log(Input.GetTouch(0).phase);
            UpdateTouchInput(Input.GetTouch(0).phase);
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) UpdateTouchInput(TouchPhase.Began);
            else if (Input.GetMouseButtonUp(0)) UpdateTouchInput(TouchPhase.Ended);
            else if (Input.touchCount == 0) UpdateTouchInput(TouchPhase.Stationary);
        }

        if (grabbedElement)
        {
            RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, mainCamera.transform.forward);
            foreach (var hit in hits)
            {
                if (hit.transform.gameObject.CompareTag("Platform"))
                {
                    grabbedElement.transform.position = hit.point;
                }
            }
        }
    }

    private void UpdateTouchInput(TouchPhase phase)
    {
        if (phase == TouchPhase.Began || phase == TouchPhase.Ended && !_isHold && _clickTime != 0f) _clickTime = Time.time;

        if (Time.time - _clickTime < timeForClick && phase == TouchPhase.Ended) Debug.Log("Click");
        else if (Time.time - _clickTime > timeForClick && _clickTime != 0f)
        {
            if (!_isHold) phase = TouchPhase.Began;
            _isHold = true;

            if (phase == TouchPhase.Began)
            {
                var e = GetElement();
                if (e)
                {
                    grabbedElement = e;
                    CmdSetObjectOwn(grabbedElement.gameObject);
                    NewPointer = movePointer;
                }
                else
                {
                    NewPointer = errorPointer;
                }
            }

            if (phase == TouchPhase.Ended)
            {
                grabbedElement = null;
                NewPointer = selectPointer;
            }

            Debug.Log("Hold " + phase);
        }

        if (phase == TouchPhase.Ended)
        {
            _clickTime = 0f;
            _isHold = false;
        }
    }

    private Element GetElement()
    {
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit))
        {
            if (hit.transform.gameObject.TryGetComponent(out Element e))
            {
                return e;
            }
        }

        return null;
    }

    [Command]
    public void CmdStartGame()
    {
        for (var i = 0; i < _manager.experience.AllElements.Length; i++)
        {
            GameObject elementPrefab = _manager.experience.AllElements[i];
            GameObject obj = Instantiate(elementPrefab);
            obj.transform.position =
                _manager.elementsSpawnPoints[i].position - elementPrefab.transform.GetChild(0).position;
            NetworkServer.Spawn(obj);
        }

        NetworkManager.singleton.maxConnections = 0;

        RcpStartGame();
    }

    [ClientRpc]
    void RcpStartGame()
    {
        _manager.localPlayer.UI.GetComponent<Animator>().SetBool("Game", true);
        _manager.IsGameStarted = true;
        FindObjectsOfType<Element>().ToList().ForEach(x => x.transform.SetParent(_manager.elementsParent));
    }

    [Command]
    void CmdSetObjectOwn(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
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