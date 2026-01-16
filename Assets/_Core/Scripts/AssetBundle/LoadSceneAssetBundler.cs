using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadSceneAssetBundler : MonoBehaviour
{
    
    static LoadSceneAssetBundler instance;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string assetBundleName; // Nome do AssetBundle (definido no Inspetor)
    public string sceneName; // Nome da cena a ser carregada (definido no Inspetor)
    public UnityEvent OnSceneLoadComplete;

    private bool isSceneLoading = false;
    private AssetBundle assetBundle;

    public void SetSceneName(string sceneName) { this.sceneName = sceneName; }

    public void StartSceneLoading(bool additive)
    {
        if (!isSceneLoading)
        {
            StartCoroutine(LoadSceneFromAssetBundle(additive));
        }
    }

    public static void StartSceneLoading(string sceneName, bool additive)
    {

        if (!instance.isSceneLoading)
        {
            instance.SetSceneName(sceneName);
            instance.StartCoroutine(instance.LoadSceneFromAssetBundle(additive));
        }
    }

    private IEnumerator LoadSceneFromAssetBundle(bool additive)
    {
        if (assetBundle == null) {
            isSceneLoading = true;

            string assetBundleFolderPath = Path.Combine(Application.persistentDataPath, "AssetBundles");
            string assetBundlePath = Path.Combine(assetBundleFolderPath, assetBundleName);

            // Verifica se o arquivo AssetBundle existe
            if (!File.Exists(assetBundlePath))
            {
                Debug.LogError("AssetBundle n�o encontrado no caminho: " + assetBundlePath);
                isSceneLoading = false;
                yield break;
            }

            // Carrega o AssetBundle da pasta persistente
            assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            Debug.Log("AssetBundle Carregado: " + assetBundleName);
        }

        // Carrega a cena do AssetBundle de maneira ass�ncrona
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName,additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        yield return asyncOperation;

        // Dispara o evento de conclus�o do carregamento da cena
        // assetBundle.Unload(false);  
        Debug.Log("Cena carregada: " + sceneName);

        isSceneLoading = false;
        OnSceneLoadComplete?.Invoke();
    }
}