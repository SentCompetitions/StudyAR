using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    public Text path;
    void Start()
    {
        // Coping packs from StreamingAssets to Application.persistentDataPath
        BetterStreamingAssets.Initialize();

        string[] paths = BetterStreamingAssets.GetFiles("Packs", "pack.json", SearchOption.AllDirectories);
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = Path.GetDirectoryName(paths[i]);
        }

        path.text = Application.persistentDataPath + '\n' + String.Join(", ", paths);

        foreach (string packPath in paths)
        {
            string dirPath = Path.Combine(Application.persistentDataPath, packPath);
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
            Directory.CreateDirectory(dirPath);
            foreach (string file in BetterStreamingAssets.GetFiles(packPath, "*", SearchOption.TopDirectoryOnly))
            {
                byte[] data = BetterStreamingAssets.ReadAllBytes(file);
                File.WriteAllBytes(Path.Combine(Application.persistentDataPath, file), data);
            }
        }
    }
}
