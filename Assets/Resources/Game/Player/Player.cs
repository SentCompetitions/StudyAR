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
    [Space]
    public GameObject connectButtons;
    public Color buttonError;
    public GameObject connectButtonPrefab;
    public GameObject wirePrefab;

    [Header("Debug (Readonly)")]
    public Element grabbedElement;
    private WirePort selectedWirePort;
    private Element selectedElement;

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

    private Element _oldElement;

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
        else if (!IsPointerOverUIObject())
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

        var e = GetElement();
        if (e)
        {
            if (_oldElement) {
                _oldElement.GetComponentInChildren<cakeslice.Outline>().enabled = false;
            }
            _oldElement = e;
            e.GetComponentInChildren<cakeslice.Outline>().enabled = true;
        }
        else if (_oldElement)
        {
            _oldElement.GetComponentInChildren<cakeslice.Outline>().enabled = false;
            _oldElement = null;
        }

        if (Time.time - _clickTime < timeForClick && phase == TouchPhase.Ended)
        {
            if (selectedWirePort.Equals(default(WirePort)))
            {
                if (e && !UI.GetComponent<Animator>().GetBool("Connect"))
                {
                    foreach (Transform o in connectButtons.transform) Destroy(o.gameObject);
                    foreach (WirePort wirePort in e.wirePorts)
                    {
                        GameObject newButton = Instantiate(connectButtonPrefab, connectButtons.transform);
                        newButton.GetComponentInChildren<Text>().text = wirePort.type;
                        WirePort tempWirePort = wirePort;
                        newButton.GetComponent<Button>().onClick.AddListener(() => StartConnectingPorts(tempWirePort));
                    }

                    UI.GetComponent<Animator>().SetBool("Connect", true);
                    NewPointer = selectPointer;
                    selectedElement = e;
                    SetWirePortIcons(true);
                }
                else if (UI.GetComponent<Animator>().GetBool("Connect"))
                {
                    UI.GetComponent<Animator>().SetBool("Connect", false);
                    NewPointer = selectPointer;
                    SetWirePortIcons(false);
                }
                else
                {
                    StartCoroutine(BlinkPointer(errorPointer));
                }
            }
            else
            {
                if (e && e != selectedElement)
                {
                    foreach (Transform o in connectButtons.transform) Destroy(o.gameObject);
                    foreach (WirePort wirePort in e.wirePorts)
                    {
                        GameObject newButton = Instantiate(connectButtonPrefab, connectButtons.transform);
                        WirePort from = selectedWirePort;
                        WirePort to = wirePort;

                        Wire wire = FindObjectsOfType<Wire>().ToList()
                            .Find(w =>
                                (w.wirePort1.wirePos == from.wirePos || w.wirePort2.wirePos == from.wirePos) &&
                                (w.wirePort1.wirePos == to.wirePos || w.wirePort2.wirePos == to.wirePos));

                        newButton.GetComponentInChildren<Text>().text = wirePort.type;
                        if (!wire)
                        {
                            from.element = selectedElement.gameObject;
                            to.element = e.gameObject;
                            newButton.GetComponent<Button>().onClick.AddListener(() => ConnectPorts(from, to));
                        }
                        else
                        {
                            newButton.GetComponent<Image>().color = buttonError;
                            GameObject temp = wire.gameObject;
                            newButton.GetComponent<Button>().onClick.AddListener(() => DestroyWithClose(temp));
                        }
                    }

                    UI.GetComponent<Animator>().SetBool("Connect", true);
                    NewPointer = selectPointer;
                    selectedElement = null;
                    selectedWirePort = default;
                }
                else
                {
                    StartCoroutine(BlinkPointer(errorPointer));
                }
            }

            Debug.Log("Click");
        }
        else if (Time.time - _clickTime > timeForClick && _clickTime != 0f)
        {
            if (!_isHold) phase = TouchPhase.Began;
            _isHold = true;

            if (phase == TouchPhase.Began && selectedWirePort.Equals(default(WirePort)))
            {
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

            if (phase == TouchPhase.Ended && selectedWirePort.Equals(default(WirePort)))
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

    private void StartConnectingPorts(WirePort originPort)
    {
        selectedWirePort = originPort;
        NewPointer = connectPointer;
        UI.GetComponent<Animator>().SetBool("Connect", false);
    }

    private void ConnectPorts(WirePort from, WirePort to)
    {
        CmdSpawnWire(from, to);
        UI.GetComponent<Animator>().SetBool("Connect", false);
        SetWirePortIcons(false);
    }

    private void DestroyWithClose(GameObject o)
    {
        UI.GetComponent<Animator>().SetBool("Connect", false);
        CmdDestroy(o);
    }

    [Command]
    private void CmdDestroy(GameObject o)
    {
        NetworkServer.Destroy(o);
    }

    private void SetWirePortIcons(bool active)
    {
        if (active)
        {
            foreach (var element in FindObjectsOfType<Element>())
            {
                foreach (var elementWirePort in element.wirePorts)
                {
                    GameObject icon = new GameObject("Wireicon");
                    icon.transform.SetParent(UI.transform);
                    var image = icon.AddComponent<Image>();
                    image.sprite = Array.Find(wirePrefab.GetComponent<Wire>().wireSettings,
                            w => w.type == elementWirePort.type).icon;
                    image.rectTransform.sizeDelta = new Vector2(20f, 20f);
                    var script = icon.AddComponent<WirePortIcon>();
                    script.mainCamera = mainCamera.GetComponent<Camera>();
                    script.target = elementWirePort.wirePos;
                }
            }
        }
        else
        {
            Debug.Log("De");
            foreach (Transform t in UI.transform)
            {
                Debug.Log(t.gameObject.name);
                if (t.gameObject.name == "Wireicon") Destroy(t.gameObject);
            }
        }
    }

    [Command]
    private void CmdSpawnWire(WirePort from, WirePort to)
    {
        Debug.Log($"[SERVER] Created link between {from.element} and {to.element}");
        GameObject obj = Instantiate(wirePrefab);
        Wire wire = obj.GetComponent<Wire>();
        wire.wirePort1 = from;
        wire.wirePort2 = to;
        NetworkServer.Spawn(obj);
        RpcSpawnWire(obj);
    }

    [ClientRpc]
    private void RpcSpawnWire(GameObject wire)
    {
        wire.transform.SetParent(_manager.elementsParent);
    }

    private IEnumerator BlinkPointer(Sprite blinkPointer)
    {
        Sprite old = pointer.sprite;
        NewPointer = blinkPointer;
        yield return new WaitForSeconds(0.5f);
        if (pointer.sprite == blinkPointer) NewPointer = old;
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