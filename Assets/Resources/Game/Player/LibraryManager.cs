using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Resources.Structs;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class LibraryManager : MonoBehaviour
{
    public GameObject libraryUI;
    public GameObject libraryButton;
    public Transform libraryListContent;
    public InputField search;

    public Button startButton;

    [Header("Text")]
    public Text title;
    public String selectPack;
    public String selectExperience;

    private Animator _animator;
    private static readonly int Library = Animator.StringToHash("Library");
    private ObservableCollection<LibraryElement> _allLibraryElements;
    private ObservableCollection<LibraryElement> _libraryElements;
    private Pack selectedPack;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _allLibraryElements = new ObservableCollection<LibraryElement>();
        _allLibraryElements.CollectionChanged += AllLibraryElementsOnCollectionChanged;

        _libraryElements = new ObservableCollection<LibraryElement>();
        _libraryElements.CollectionChanged += LibraryElementsOnCollectionChanged;

        search.onValueChanged.AddListener(delegate { AllLibraryElementsOnCollectionChanged(this, null); });

        NetworkGameManager.instance.experience = default;
    }

    private void ShowPacks()
    {
        search.text = "";
        selectedPack = default;
        title.text = selectPack;
        _allLibraryElements.Clear();
        for (var i = 0; i < GameManager.instance.packs.Length; i++)
        {
            Pack pack = GameManager.instance.packs[i];
            _allLibraryElements.Add(new LibraryElement {text = $"№{i+1}\n{pack.subjectDisplay}\n\n{pack.name}", pack = pack});
        }
    }

    private void ShowExperiences()
    {
        search.text = "";
        title.text = String.Format(selectExperience, selectedPack.name);
        _allLibraryElements.Clear();
        for (var i = 0; i < selectedPack.experiences.Length; i++)
        {
            Experience experience = selectedPack.experiences[i];
            _allLibraryElements.Add(new LibraryElement {text = $"№{i+1}\n\n\n{experience.name}", experience = experience});
        }
    }

    public void OnLibraryOpenButtonClick()
    {
        _animator.SetBool(Library, true);
        ShowPacks();
    }

    public void OnLibraryBackButtonClick()
    {
        if (!selectedPack.Equals(default(Pack)))
            ShowPacks();
        else
            Close();
    }

    private void Close()
    {
        _animator.SetBool(Library, false);
    }

    private void AllLibraryElementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _libraryElements.Clear();
        _allLibraryElements.Where(e => e.text.ToLower().Contains(search.text.ToLower()))
            .ToList().ForEach(e => _libraryElements.Add(e));
    }

    private void LibraryElementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (Transform child in libraryListContent) Destroy(child.gameObject);

            foreach (LibraryElement element in _libraryElements)
        {
            GameObject newButton = Instantiate(libraryButton, Vector3.zero, Quaternion.identity, libraryListContent);
            newButton.GetComponentInChildren<Text>().text = element.text;

            if (selectedPack.Equals(default(Pack)))
            {
                Pack temp = element.pack;
                newButton.GetComponent<Button>().onClick.AddListener(delegate { OnLibraryButtonClick(temp); });
            }
            else
            {
                Experience temp = element.experience;
                newButton.GetComponent<Button>().onClick.AddListener(delegate { OnLibraryButtonClick(temp); });
            }
        }
    }

    private void OnLibraryButtonClick(Pack pack)
    {
        selectedPack = pack;
        ShowExperiences();
    }

    private void OnLibraryButtonClick(Experience experience)
    {
        NetworkGameManager.instance.experience = experience;
        startButton.interactable = true;
        Close();
    }

    private struct LibraryElement
    {
        public String text;
        public Pack pack;
        public Experience experience;
    }
}
