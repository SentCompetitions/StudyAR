using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Resources.ExperienceManager;
using Resources.Structs;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    public Text path;

    public int packJsonVersion = 1;
    
    void Awake()
    {
        AotHelper.EnsureDictionary<string, SchemaElement>();
        AotHelper.EnsureDictionary<string, List<Dictionary<string, string>>>();
        AotHelper.EnsureList<ElementProperty>();
    }

    void Start()
    {
        CopyToPersistent();

        List<Pack> packs = GetPacks();

        path.text = "Загруженные сбоники: " + String.Join(", ", packs.Select(x => x.name).ToArray())
                                            + '\n' + Application.persistentDataPath;
        GameManager.instance.packs = packs.ToArray();
    }

    /// <summary>
    /// Copy packs from StreamingAssets to Application.persistentDataPath
    /// </summary>
    private void CopyToPersistent()
    {
        BetterStreamingAssets.Initialize();

        string[] paths = BetterStreamingAssets.GetFiles("Packs", "*", SearchOption.AllDirectories);

        string dirPath = Path.Combine(Application.persistentDataPath, "Packs", "BuildIn");
        if (Directory.Exists(dirPath))
            Directory.Delete(dirPath, true);
        Directory.CreateDirectory(dirPath);

        foreach (string filePath in paths)
        {
            byte[] data = BetterStreamingAssets.ReadAllBytes(filePath);
            string file = Path.Combine(
                Application.persistentDataPath, "Packs", "BuildIn",
                Path.Combine(filePath.Split('/').Skip(1).ToArray())
            );
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllBytes(file, data);
        }
    }

    private List<Pack> GetPacks()
    {
        List<Pack> packs = new List<Pack>();

        foreach (string packFile in Directory.GetFiles(Path.Combine(Application.persistentDataPath, "Packs"),
            "pack.json", SearchOption.AllDirectories))
        {
            try
            {
                string packJson = File.ReadAllText(packFile);

                PackVersion packVersion = JsonUtility.FromJson<PackVersion>(packJson);

                if (packVersion.packJsonVersion == packJsonVersion)
                {
                    Pack pack = JsonUtility.FromJson<Pack>(packJson);
                    for (var i = 0; i < pack.experiences.Length; i++)
                    {
                        Experience experience = pack.experiences[i];

                        List<Tuple<GameObject, string>> elements = new List<Tuple<GameObject, string>>();

                        for (var ii = 0; ii < experience.actions.Length; ii++)
                        {
                            Step action = experience.actions[ii];
                            if (String.Join("_", action.action.Split('_').Take(2)) == "SCHEME_MAKE")
                            {
                                string json = String.Join("_", action.action.Split('_').Skip(2).Take(1));
                                Dictionary<string, SchemaElement> schemaElements =
                                    JsonConvert.DeserializeObject<Dictionary<string, SchemaElement>>(json);
                                //pack.experiences[i].actions[ii].schemaElements = schemaElements;
                                foreach (var element in schemaElements)
                                {
                                    elements.Add(new Tuple<GameObject, string>((GameObject) Array.Find(
                                            UnityEngine.Resources.FindObjectsOfTypeAll(typeof(GameObject)),
                                            x => x.name == element.Value.assetName),
                                        JsonConvert.SerializeObject(new ElementProperties
                                            {propertiesArray = element.Value.properties})));
                                }
                            }

                            string imageFile = Path.Combine(Path.GetDirectoryName(packFile), action.image);
                            if (File.Exists(imageFile))
                            {
                                var fileData = File.ReadAllBytes(imageFile);
                                var texture = new Texture2D(2, 2);
                                texture.LoadImage(fileData);
                                texture.Apply();
                                experience.actions[ii].imageTexture = texture;
                            }
                        }

                        elements = elements.Distinct().ToList();
                        pack.experiences[i].allElements = elements.Select(e => e.Item1).ToArray();
                        pack.experiences[i].elementProperties = elements.Select(e => JsonConvert.DeserializeObject<ElementProperties>(e.Item2)).ToArray();
                    }

                    packs.Add(pack);
                    Debug.Log("[ExperienceManager] Loaded " + packFile);
                }
                else
                {
                    throw new PackVersionInvalid(
                        $"Версия сборника {(packVersion.packJsonVersion > packJsonVersion ? "выше" : "ниже")} требуемой. Обновите сборник или приложение"
                        );
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ExperienceManager] Error on loading " + packFile + '\n' + e);
                AlertManager.ShowAlert(new Alert
                {
                    title = "Ошибка при загрузке сборника",
                    text = $"{e.Message} ({e.GetType().Name})\nСборник: {packFile}\n"
                });
            }
        }

        return packs;
    }
}