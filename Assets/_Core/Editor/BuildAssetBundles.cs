using UnityEditor;
using UnityEngine;
using System.IO;
 
public class BuildAssetBundles
{
    [MenuItem("Assets/Build AssetBundles (Unity 6)")]
    static void BuildAllAssetBundles()
    {
        string path = "Assets/AssetBundles";
 
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
 
        BuildPipeline.BuildAssetBundles(
            path,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget // gera para a plataforma ativa
        );
 
        Debug.Log("✅ AssetBundles gerados em: " + path);
    }
}