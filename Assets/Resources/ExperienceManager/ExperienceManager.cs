using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Resources.Structs;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    public Text path;

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
                packs.Add(JsonUtility.FromJson<Pack>(File.ReadAllText(packFile)));
                Debug.Log("[ExperienceManager] Loaded " + packFile);
            }
            catch (Exception e)
            {
                Debug.LogError("[ExperienceManager] Error on loading " + packFile + '\n' + e);
            }
        }

        return packs;
    }
}