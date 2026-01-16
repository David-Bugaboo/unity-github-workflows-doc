using UnityEngine;

public class OSVersionChecker : MonoBehaviour
{
    public string windowsURL;
    public string androidURL;
    public string iOSURL;
    public string defaultURL;

    private void Start()
    {
        string url = "";

        // Verifica o sistema operacional atual
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Debug.Log("Sistema Operacional encontrado: Windows.");
        url = windowsURL;
#elif UNITY_ANDROID
        Debug.Log("Sistema Operacional encontrado: Android.");
        url = androidURL;
#elif UNITY_IOS
        Debug.Log("Sistema Operacional encontrado: iOS.");
        url = iOSURL;
#else
        Debug.LogWarning("Sistema Operacional n�o encontrado!.");
        url = defaultURL;
#endif

        AssetBundleDownloader downloadAsset = FindObjectOfType<AssetBundleDownloader>();
        if (downloadAsset != null)
        {
            downloadAsset.manifestURL = url;
            downloadAsset.StartDownloadAssetBundle();
        }
        else
        {
            Debug.LogWarning("Script AssetBundleDownloader n�o encontrado nessa cena.");
        }
    }
}