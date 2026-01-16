using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AssetBundleDownloader : MonoBehaviour
{
    private string BundlePath; // Caminho persistente para armazenar os asset bundles
    private string manifestFile;
    private Dictionary<string, string> assetBundleDict;

    [Serializable]
    private class JsonData
    {
        public string version;
        public AssetBundleData[] assetBundles;
    }

    [Serializable]
    private class AssetBundleData
    {
        public string name;
        public string version;
        public string url;
    }

    public UnityEvent DownloadAssetStart;
    public UnityEvent DownloadComplete;
    public UnityEvent NoInternet;
    public TextMeshProUGUI mensagemUI;
    public TextMeshProUGUI progressText;
    public TextData AssetBundleTextUI;
    public string manifestURL; // URL do arquivo JSON do manifesto
    public string assetBundleName;


    public void StartDownloadAssetBundle()
    {
        BundlePath = Application.persistentDataPath + "/AssetBundles";
        manifestFile = Application.persistentDataPath + "/IELmanifest.json";
        assetBundleDict = new Dictionary<string, string>();
        DownloadAssetStart.Invoke();
        Debug.Log("Pasta do cache do aplicativo localizado em " + Application.persistentDataPath);

        StartCoroutine(DownloadManifest());
    }

    private void PopulateAssetBundleDict(string json)
    {
        JsonData container = JsonUtility.FromJson<JsonData>(json);

        foreach (var assetBundleData in container.assetBundles)
        {
            assetBundleDict[assetBundleData.name] = assetBundleData.url;
        }
    }

    private IEnumerator DownloadManifest()
    {
        string assetBundleFolderPath = Path.Combine(Application.persistentDataPath, "AssetBundles");
        string assetBundlePath = Path.Combine(assetBundleFolderPath, assetBundleName);

        bool assetBundleNotDownloaded = false;

        if (Directory.Exists(Application.persistentDataPath) && !File.Exists(assetBundlePath))
            assetBundleNotDownloaded = true;

        bool existingManifestExists = File.Exists(manifestFile);
        bool isInternetReachable = Application.internetReachability != NetworkReachability.NotReachable;

        if (!isInternetReachable)
        {
            ErrorHandler.ShowError("NoInternet");
            Debug.LogError("Erro 401: Sem conex�o com a internet.");
            mensagemUI.text = "Por favor, reinicie a aplica��o e tente novamente";
            NoInternet?.Invoke();
            yield break;
        }

        if (!existingManifestExists)
        {
            Debug.Log("DownloadManifest: Manifesto n�o encontrado. Baixando novo arquivo.");
            yield return StartCoroutine(NewManifest());
        }
        else
        {
            // Parse the existing manifest
            Debug.Log("DownloadManifest: Manifesto existente encontrado");
            string existingManifestJson = System.IO.File.ReadAllText(manifestFile);
            Debug.Log("DownloadManifest: Lendo Manifesto existente: " + Environment.NewLine + existingManifestJson);
            string ExistingVersion = ParseManifestJsonVersion(existingManifestJson);

            // Download the new manifest
            using UnityWebRequest www = UnityWebRequest.Get(manifestURL);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("DownloadManifest: Erro ao baixar o Manifesto JSON: " + www.error);
                AssetBundleTextUI.SetTextUI(0, mensagemUI);
                NoInternet?.Invoke();

                // Delete existingmanifest to avoid corrupted file
                if (File.Exists(manifestFile))
                {
                    File.Delete(manifestFile);
                }

                yield break;
            }

            string newManifestJson = www.downloadHandler.text;
            Debug.Log("Lendo manifesto baixado da url: " + Environment.NewLine + newManifestJson);

            // Parse the new manifest
            PopulateAssetBundleDict(newManifestJson);

            if (assetBundleNotDownloaded)
                File.WriteAllText(manifestFile, newManifestJson);
            else
            {
                string NewVersion = ParseManifestJsonVersion(newManifestJson);

                // Compare the versions
                int result = CompareVersions(ExistingVersion, NewVersion);

                if (result == 1)
                {
                    Debug.Log(ExistingVersion + " � maior que " + NewVersion);
                    Debug.Log("A vers�o existente do Manifesto JSON � maior. Nenhuma a��o necess�ria.");
                    AssetBundleTextUI.SetTextUI(1, mensagemUI);
                    DownloadComplete.Invoke();
                    yield break;
                }
                else if (result == -1)
                {
                    Debug.Log(ExistingVersion + " � menor que " + NewVersion);
                    Debug.Log(
                        "DownloadManifest: Nova vers�o do Manifesto JSON encontrada. Substituindo o manifesto existente.");
                    AssetBundleTextUI.SetTextUI(2, mensagemUI);
                    File.WriteAllText(manifestFile, newManifestJson);
                    yield return StartCoroutine(GetAssetBundleURL());
                }
                else if (result == 0)
                {
                    Debug.Log(ExistingVersion + " � igual a " + NewVersion);
                    Debug.Log("As vers�es do Manifesto JSON s�o iguais. Nenhuma a��o necess�ria.");
                    AssetBundleTextUI.SetTextUI(1, mensagemUI);
                    DownloadComplete.Invoke();
                    yield break;
                }
                else
                {
                    Debug.Log("Compara��o de vers�es n�o � poss�vel");
                }
            }

            // Proceed with downloading asset bundles
            yield return StartCoroutine(GetAssetBundleURL());
        }
    }

    private IEnumerator NewManifest()
    {
        // Download the new manifest
        using UnityWebRequest www = UnityWebRequest.Get(manifestURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string newManifestJson = www.downloadHandler.text;

            // Save the downloaded manifest JSON to a file
            File.WriteAllText(manifestFile, newManifestJson);
            Debug.Log("NewManifest: Novo Manifesto salvo");

            // Parse the new manifest
            ParseManifestJsonVersion(newManifestJson);
            PopulateAssetBundleDict(newManifestJson);

            // Proceed with downloading asset bundles
            yield return StartCoroutine(GetAssetBundleURL());
        }
        else
        {
            Debug.LogError("NewManifest: Erro ao baixar o Manifesto JSON: " + www.error);
        }
    }

    private IEnumerator GetAssetBundleURL()
    {
        if (assetBundleDict.Count == 0)
        {
            Debug.LogError("GetAssetBundleURL: Nenhuma chave encontrada em assetBundleDict.");
            yield break;
        }

        foreach (var kvp in assetBundleDict)
        {
            string name = kvp.Key;
            string url = kvp.Value;

            Debug.Log("GetAssetBundleURL: Lendo JSON: " + name + " " + url);
            yield return DownloadAssetBundle(name, url);
        }
    }

    private string ParseManifestJsonVersion(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("ParseManifestJsonVersion: JSON vazio. O JSON do manifesto n�o pode estar vazio.");
            return string.Empty;
        }

        // Faz a leitura do JSON
        JsonData data = JsonUtility.FromJson<JsonData>(json);

        // Obt�m o n�mero da vers�o como string
        string versionNumber = data.version;
        Debug.Log("ParseManifestJsonVersion: N�mero da vers�o: " + versionNumber);

        // Retorna a vers�o como string
        return versionNumber;
    }

    private int CompareVersions(string string1, string string2)
    {
        // Extrai os n�meros de vers�o major e minor
        int major1 = GetMajorVersion(string1);
        int major2 = GetMajorVersion(string2);

        // Compara os n�meros de vers�o major
        if (major1 > major2)
        {
            return 1;
        }
        else if (major1 < major2)
        {
            return -1;
        }
        else
        {
            // Os n�meros de vers�o major s�o iguais, ent�o verifica os n�meros de vers�o minor
            int minor1 = GetMinorVersion(string1);
            int minor2 = GetMinorVersion(string2);

            // Compara os n�meros de vers�o minor
            if (minor1 > minor2)
            {
                return 1;
            }
            else if (minor1 < minor2)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private int GetMajorVersion(string versionString)
    {
        // Extrai o n�mero de vers�o major do formato "X.Y"
        string[] parts = versionString.Split('.');
        if (parts.Length >= 1 && int.TryParse(parts[0], out int major))
        {
            return major;
        }

        // Retorno padr�o caso haja algum erro
        return 0;
    }

    private int GetMinorVersion(string versionString)
    {
        // Extrai o n�mero de vers�o minor do formato "X.Y"
        string[] parts = versionString.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int minor))
        {
            return minor;
        }

        // Retorno padr�o caso haja algum erro
        return 0;
    }

    private IEnumerator DownloadAssetBundle(string name, string url)
    {
        // Prepara o caminho para salvar o arquivo no dispositivo do usuário.
        string savePath = Path.Combine(BundlePath, name);
        string directoryPath = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        Debug.Log("Tentando baixar o AssetBundle: " + name);
        AssetBundleTextUI.SetTextUI(3, mensagemUI);

        // Etapa 1: Exibe um popup de confirmação com uma mensagem genérica ANTES de iniciar o download.
        bool confirm = false;
        yield return new PopupManager.WaitPopup(new PopupData("Atualização Necessária",
            $"Uma nova atualização de conteúdo precisa ser baixada. Deseja continuar?", "Baixar", "Sair",
            () => confirm = true, Application.Quit));

        // Se o usuário clicar em "Sair" (ou negar), a aplicação fecha.
        if (!confirm)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            yield break;
        }

        // Etapa 2: Inicia o download usando a requisição GET, que é mais confiável.
        Debug.Log("Iniciando download do AssetBundle de: " + url);
        using UnityWebRequest www = UnityWebRequest.Get(url);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Envia a requisição e monitora o progresso.
        var asyncOperation = www.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            // Calcula a porcentagem do progresso.
            float progress = Mathf.Clamp01(asyncOperation.progress);
            int progressPercentage = Mathf.FloorToInt(progress * 100);

            // Tenta obter o tamanho total do arquivo a partir dos cabeçalhos da requisição GET.
            // Isso pode aparecer no meio do download.
            string totalSizeStr = www.GetResponseHeader("Content-Length");
            string sizeInfo = "";
            if (ulong.TryParse(totalSizeStr, out ulong totalSizeBytes))
            {
                sizeInfo = $" / {totalSizeBytes.BytesToString()}";
            }

            // Atualiza a UI de progresso.
            Debug.Log($"Progresso do download do AssetBundle {name}: {progressPercentage}%");
            AssetBundleTextUI.SetTextUI(4, mensagemUI);
            progressText.text = $"{progressPercentage}% ({www.downloadedBytes.BytesToString()}{sizeInfo})";

            // Pausa a corrotina por um frame para não travar o jogo.
            yield return null;
        }

        // Etapa 3: Verifica o resultado após o término do download.
        if (www.result != UnityWebRequest.Result.Success)
        {
            // Se houve um erro de conexão ou de protocolo...
            AssetBundleTextUI.SetTextUI(0, mensagemUI);

            // Deleta o manifesto local para forçar uma nova verificação na próxima vez.
            if (File.Exists(manifestFile))
            {
                File.Delete(manifestFile);
            }

            // Lança uma exceção para parar o fluxo e registrar o erro.
            throw new Exception($"O download do AssetBundle '{name}' falhou: {www.error}");
        }
        else
        {
            // Se o download foi bem-sucedido...
            // Pega os dados baixados.
            byte[] data = www.downloadHandler.data;
            // Salva os dados no arquivo local.
            File.WriteAllBytes(savePath, data);

            // Atualiza a UI para mostrar que o download foi concluído.
            Debug.Log("AssetBundle " + name + " baixado com sucesso a partir do URL: " + url);
            AssetBundleTextUI.SetTextUI(5, mensagemUI);
            progressText.enabled = false;
            DownloadComplete.Invoke();
        }
    }
}