#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleManifestGenerator : EditorWindow
{
    private List<AssetBundleData> assetBundles = new();
    private string manifestVersion = "1.0"; // Vers�o do manifesto com duas casas decimais

    private Vector2 scrollPosition;

    private string saveFilePath = ""; // Caminho do arquivo de salvamento

    [MenuItem("Assets/Create Assetbundle Manifest JSON file")]
    private static void OpenWindow()
    {
        GetWindow<AssetBundleManifestGenerator>("AssetBundle Manifest Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("AssetBundle Manifest Generator", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Manifest Version");
        manifestVersion = EditorGUILayout.TextField(manifestVersion);

        for (int i = 0; i < assetBundles.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("Asset Bundle " + (i + 1).ToString(), EditorStyles.boldLabel);

            assetBundles[i].name = EditorGUILayout.TextField("Bundle Name:", assetBundles[i].name);
            assetBundles[i].version = EditorGUILayout.TextField("Bundle Version:", assetBundles[i].version);
            assetBundles[i].url = EditorGUILayout.TextField("Bundle URL:", assetBundles[i].url);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Asset Bundle"))
        {
            assetBundles.Add(new AssetBundleData());
        }

        if (GUILayout.Button("Create Manifest"))
        {
            CreateManifest();
        }

        if (GUILayout.Button("Save Data"))
        {
            SaveData();
        }

        if (GUILayout.Button("Load Data"))
        {
            LoadData();
        }
    }

    private void CreateManifest()
    {
        AssetBundleManifestData manifestData = new()
        {
            version = manifestVersion,
            assetBundles = assetBundles
        };

        string json = JsonUtility.ToJson(manifestData, true);

        string filePath = EditorUtility.SaveFilePanel("Save Manifest JSON", "", "IELmanifest.json", "json");
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, json);
            Debug.Log("Manifest JSON file created at: " + filePath);
        }
    }

    private void SaveData()
    {
        string filePath = EditorUtility.SaveFilePanel("Save Data", "", "data.txt", "txt");
        if (!string.IsNullOrEmpty(filePath))
        {
            saveFilePath = filePath;

            // Cria um objeto de dados de salvamento
            SaveDataObject saveData = new()
            {
                manifestVersion = manifestVersion,
                assetBundles = assetBundles
            };

            // Converte o objeto de dados em formato JSON
            string jsonData = JsonUtility.ToJson(saveData);

            // Salva os dados no arquivo
            File.WriteAllText(saveFilePath, jsonData);

            Debug.Log("Data saved at: " + saveFilePath);
        }
    }

    private void LoadData()
    {
        string filePath = EditorUtility.OpenFilePanel("Load Data", "", "txt");
        if (!string.IsNullOrEmpty(filePath))
        {
            saveFilePath = filePath;

            // L� os dados do arquivo
            string jsonData = File.ReadAllText(saveFilePath);

            // Converte os dados JSON em objeto de dados
            SaveDataObject saveData = JsonUtility.FromJson<SaveDataObject>(jsonData);

            // Restaura os valores salvos
            manifestVersion = saveData.manifestVersion;
            assetBundles = saveData.assetBundles;

            Debug.Log("Data loaded from: " + saveFilePath);
        }
    }

    [System.Serializable]
    private class AssetBundleManifestData
    {
        public string version;
        public List<AssetBundleData> assetBundles;
    }

    [System.Serializable]
    private class AssetBundleData
    {
        public string name;
        public string version;
        public string url;
    }

    [System.Serializable]
    private class SaveDataObject
    {
        public string manifestVersion;
        public List<AssetBundleData> assetBundles;
    }
}
#endif