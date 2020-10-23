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
    public Color grabColor;
    public Color errorColor;
    private Color normalColor;

    [Header("Debug (Readonly)")]
    public Element grabbedElement;

    private GameObject mainCamera;
    private GameManager _manager = GameManager.instance;

    private void Start()
    {
        if (isLocalPlayer)
        {
            mainCamera = Camera.main.gameObject;
            normalColor = pointer.color;
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

        if(!_manager.IsGameStarted) return;

        if (Input.touchCount == 1 && !IsPointerOverUIObject())
        {
            GetElement(Input.GetTouch(0).phase);
        }

        if (Input.GetMouseButtonDown(0)) GetElement(TouchPhase.Began);
        else if (Input.GetMouseButtonUp(0)) GetElement(TouchPhase.Ended);

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

    private void GetElement(TouchPhase phase)
    {
        if (phase == TouchPhase.Began)
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit))
            {
                if (hit.transform.gameObject.TryGetComponent(out Element e))
                {
                    grabbedElement = e;
                    CmdSetObjectOwn(grabbedElement.gameObject);
                    pointer.color = grabColor;
                }
                else
                {
                    pointer.color = errorColor;
                }
            }
            else
            {
                pointer.color = errorColor;
            }
        }

        if (phase == TouchPhase.Ended)
        {
            grabbedElement = null;
            pointer.color = normalColor;
        }
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

    [Command]
    public void CmdDamage(int damage)
    {
        // _manager.battery.health -= damage;
    }

    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}