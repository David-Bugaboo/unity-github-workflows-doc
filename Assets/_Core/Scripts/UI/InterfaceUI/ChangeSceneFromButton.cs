using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Spaces;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneFromButton : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private EnableDisableAnimation fade;

    void Start()
    {
        if (runner == null)
        {
            Debug.LogError("ServerSelector não conseguiu encontrar o script SpaceRoom na cena!");
        }
    }

    public void JoinPalcoA(string instanceId)
    {
        JoinServer(instanceId, "03_PalcoA");
    }

    public void JoinPalcoB(string instanceId)
    {
        JoinServer(instanceId, "04_PalcoB");
    }

    public void JoinPalcoC(string instanceId)
    {
        JoinServer(instanceId, "05_PalcoC");
    }


    public async void JoinServer(string instanceId, string sceneName)
    {
        if (runner == null)
        {
            Debug.LogError("A conexão falhou porque o SpaceRoom não foi encontrado.");
            return;
        }

        fade.gameObject.SetActive(true);
        fade.OnContentIn();

        // Espera a animação do fade terminar
        float fadeDuration = fade.animationDuration;
        await Task.Delay((int)(fadeDuration * 1000));

        SessionManager.Instance.SetIntentionalShutdownFlag();
        SessionPersistence.ClearData(); // Limpa dados de reconexão para evitar redirecionamento automático
        FindFirstObjectByType<ApplicationManager>().isQuitting = true;
        await runner.Shutdown(true);
        DestroyImmediate(runner.gameObject);
        var spaceDescription = new SpaceDescription();
        spaceDescription.spaceId = instanceId.Replace(" ", "_");
        spaceDescription.sceneName = sceneName;
        SpaceRoom.RegisterSpaceRequest(spaceDescription);
        SceneManager.LoadScene(spaceDescription.sceneName, LoadSceneMode.Single);

        // Opcional: Desativa os botões para não clicar duas vezes
        // (Você pode criar referências para os botões e desativá-los aqui)
        // server1Button.interactable = false;
        // server2Button.interactable = false;
    }
}