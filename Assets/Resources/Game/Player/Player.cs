using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Mirror;
using Resources.Structs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Object = System.Object;

public class Player : NetworkBehaviour
{
    [Header("UI")] public GameObject UI;
    public List<GameObject> hostOnlyObjects;
    public List<GameObject> clientOnlyObjects;
    [Space]
    public Image pointer;
    private Sprite selectPointer;
    public Sprite movePointer;
    public Sprite connectPointer;
    public Sprite errorPointer;
    [Space]
    public GameObject connectButtons;
    public Transform elementInfoContent;
    public Color buttonError;
    public GameObject connectButtonPrefab;
    public GameObject elementInfoPrefab;
    public GameObject wirePrefab;
    [NonSerialized] public UnityEvent<string> onGameAction;
    [Space]
    public Transform instructionParent;
    public GameObject textPrefab;
    public Color currentActionColor;

    [Header("Local Player Scripts")] public TouchInputManager touchInputManager;

    [Header("Debug (Readonly)")] public Element e;
    public Element grabbedElement;
    private WirePort selectedWirePort;
    private Element selectedElement;

    [NonSerialized] public GameObject mainCamera;
    private GameManager _manager = GameManager.instance;
    private NetworkGameManager _netManager = NetworkGameManager.instance;

    private Sprite _newPointer;

    /// <summary>
    ///  Меняет указатель на другой с анимацией
    /// </summary>
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
        onGameAction =
            new UnityEvent<string>(); // Если игрок возьмёт что-то или создаст цепь, то вызоветься это событие
        if (isLocalPlayer)
        {
            mainCamera = Camera.main.gameObject;
            selectPointer = pointer.sprite;

            onGameAction.AddListener(CmdGameAction);
            touchInputManager.onClick.AddListener(Click);
            touchInputManager.onHolding.AddListener(Holding);
            touchInputManager.onHoldStart.AddListener(HoldStart);
            touchInputManager.onHoldEnd.AddListener(HoldEnd);
        }
        else
        {
            UI.SetActive(false);
            touchInputManager.enabled = false;
        }

        if (!isServer) hostOnlyObjects.ForEach(o => o.SetActive(false));
        else clientOnlyObjects.ForEach(o => o.SetActive(false));
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        transform.position = mainCamera.transform.position; // Привязка положения игрока к камере
        transform.rotation = mainCamera.transform.rotation;

        if (!_manager.IsGameStarted) return;

        e = GetElement();

        // Обработка обводки
        if (e)
        {
            if (_oldElement)
            {
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
    }

    private void HoldStart()
    {
        if (selectedWirePort.Equals(default(WirePort)))
        {
            if (e)
            {
                grabbedElement = e;
                CmdSetObjectOwn(grabbedElement.gameObject);
                NewPointer = movePointer;
                onGameAction.Invoke($"PLAYER_GRAB_{e.name}");
            }
            else
            {
                NewPointer = errorPointer;
            }
        }
    }

    private void Holding()
    {
        // Обработка взятого элемента
        if (grabbedElement)
        {
            RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, mainCamera.transform.forward);
            foreach (var hit in hits)
            {
                GameObject hitGameObject = hit.transform.gameObject;
                if (hitGameObject.CompareTag("Platform"))
                {
                    grabbedElement.transform.position = hit.point;

                    if (!grabbedElement.transform.parent) return;
                    if (!grabbedElement.transform.parent.gameObject.CompareTag("Point")) return;
                    grabbedElement.GetComponentInParent<Point>().boundElem = null;

                    CmdSetParent(_manager.elementsParent.gameObject, grabbedElement.gameObject);
                }

                if (hitGameObject.CompareTag("Point"))
                {
                    Point point = hitGameObject.GetComponent<Point>();
                    if (!point.boundElem)
                    {
                        grabbedElement.transform.position = hit.point;
                        point.boundElem = grabbedElement;
                        CmdSetParent(hitGameObject, grabbedElement.gameObject);
                        point.ElemToCenter();
                    }
                }
            }
        }
    }

    private void HoldEnd()
    {
        if (selectedWirePort.Equals(default(WirePort)))
        {
            grabbedElement = null;
            NewPointer = selectPointer;
        }
    }

    private void Click()
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

                foreach (Transform o in elementInfoContent) Destroy(o.gameObject);
                foreach (var elementProperty in e.elementProperties.propertiesArray)
                {
                    GameObject newInfo = Instantiate(elementInfoPrefab, elementInfoContent);
                    newInfo.GetComponent<Text>().text =
                        $"{elementProperty.displayName}: {elementProperty.value} {elementProperty.unitName}";
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
    }

    /// <summary>
    /// Синхронизирует выполненное действие по сети (между другими игоками)
    /// </summary>
    /// <param name="action">Строка типа SUBJECT_ACTION_OBJECT (Например "PLAYER_GRAB_Battery")</param>
    [Command]
    private void CmdGameAction(string action)
    {
        Debug.Log("[SERVER] Action " + action);
        RpcGameAction(action);
    }

    /// <summary>
    /// Вызываеться у каждого игрока
    /// </summary>
    /// <param name="action"></param>
    [ClientRpc]
    private void RpcGameAction(string action)
    {
        Debug.Log("Action " + action);
        int step = _netManager.experience.GetFirstUnCompleteStep();
        if (_netManager.experience.actions[step].action == action)
        {
            Debug.Log(_netManager.experience.actions[step].description + " completed");
            _netManager.experience.actions[step].isCompleted = true;
            _manager.localPlayer.UpdateInstruction();
        }
    }

    /// <summary>
    /// Обновляет UI список инструкций
    /// </summary>
    public void UpdateInstruction()
    {
        foreach (Transform o in instructionParent) Destroy(o.gameObject);
        bool first = true;
        _netManager = NetworkGameManager.instance;
        for (var i = 0; i < _netManager.experience.actions.Length; i++)
        {
            Step step = _netManager.experience.actions[i];
            if (step.isCompleted)
            {
                first = true;
            }
            else
            {
                var text = Instantiate(textPrefab, instructionParent).GetComponent<Text>();
                text.text = $"{i + 1}. {step.description}";
                if (first) text.color = currentActionColor;
                first = false;
            }
        }
    }

    /// <summary>
    /// Получение элемента на который смотрит игрок
    /// </summary>
    /// <returns>Элемент на который смотрит игрок</returns>
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
        SetWirePortIcons(false);
    }

    [Command]
    private void CmdDestroy(GameObject o)
    {
        NetworkServer.Destroy(o);
        StartCoroutine(WaitDestroy(o));
    }

    private IEnumerator WaitDestroy(GameObject gameObject)
    {
        while (gameObject)
        {
            Debug.Log($"[SERVER] Wait for {gameObject} destroy");
            yield return new WaitForEndOfFrame();
        }

        CircuitSimulation.UpdateSimulationEvent.Invoke();
    }

    /// <summary>
    /// Показывает или отключает иконки +, - итд
    /// </summary>
    /// <param name="active">Показывать или нет иконки</param>
    private void SetWirePortIcons(bool active)
    {
        List<GameObject> icons = new List<GameObject>();
        if (active)
        {
            foreach (var element in FindObjectsOfType<Element>())
            {
                foreach (var elementWirePort in element.wirePorts)
                {
                    GameObject icon = new GameObject("Wireicon");
                    icons.Add(icon);
                    icon.transform.SetParent(UI.transform);
                    var image = icon.AddComponent<Image>();
                    image.sprite = Array.Find(wirePrefab.GetComponent<Wire>().wireSettings,
                        w => w.type == elementWirePort.type).icon;
                    float resolution = 4f + 1f * Screen.dpi;
                    if (resolution == 0) resolution = 25f;
                    image.rectTransform.sizeDelta = new Vector2(resolution, resolution);
                    var script = icon.AddComponent<WirePortIcon>();
                    script.image = image;
                    script.target = elementWirePort.wirePos;
                }
            }
        }
        else
        {
            foreach (Transform t in UI.transform)
            {
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
        Debug.Log("Need to update");
        CircuitSimulation.UpdateSimulationEvent.Invoke();
        RpcSpawnWire(obj);
    }

    [ClientRpc]
    private void RpcSpawnWire(GameObject wire)
    {
        wire.transform.SetParent(_manager.elementsParent);
    }

    /// <summary>
    /// Кратковременное переключения указателя
    /// </summary>
    /// <param name="blinkPointer">Новый указатель</param>
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
        for (var i = 0; i < _netManager.experience.allElements.Length; i++)
        {
            GameObject elementPrefab = _netManager.experience.allElements[i];
            GameObject obj = Instantiate(elementPrefab);

            Element element = obj.GetComponent<Element>();
            element.elementProperties = _netManager.experience.elementProperties[i];

            Point point = _manager.elementsSpawnPoints[i].GetComponent<Point>();
            point.boundElem = element;
            point.ElemToCenter();

            NetworkServer.Spawn(obj);
            RpcSetParent(_manager.elementsSpawnPoints[i].gameObject, obj);
        }

        NetworkManager.singleton.maxConnections = 0;

        RcpStartGame();
    }

    [ClientRpc]
    void RcpStartGame()
    {
        _manager.localPlayer.UI.GetComponent<Animator>().SetBool("Game", true);
        _manager.IsGameStarted = true;
        _manager.localPlayer.UpdateInstruction();
    }

    /// <summary>
    /// Устанавливает у всех игроков для данного элемента нового родителя
    /// </summary>
    /// <param name="parent">Родитель</param>
    /// <param name="child">Ребёнок</param>
    [Command]
    void CmdSetParent(GameObject parent, GameObject child) => RpcSetParent(parent, child);

    [ClientRpc]
    void RpcSetParent(GameObject parent, GameObject child) => child.transform.parent = parent.transform;

    /// <summary>
    /// Устанавливает нового владельца объекта в сети (Необходимо для движения объекта и изменения его свойств)
    /// </summary>
    /// <param name="gameObject">Объект</param>
    [Command]
    void CmdSetObjectOwn(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }


    /// <summary>
    /// Перекрывает ли палец игрока или указатель мыши объекты UI
    /// </summary>
    /// <returns>true если перекрывает, false если нет</returns>
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}