using System.Threading.Tasks;
using Fusion;
using Fusion.XR.Shared.Rig;
using Fusion.Addons.Spaces;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;
using UnityEngine.SceneManagement;


/**
 * 
 * SpaceLoader is in charge to load a new scene when the player collides with the box collider
 * 
 **/
[DefaultExecutionOrder(-1)]
public class SimpleSpaceLoader : MonoBehaviour
{
    [Header("Target space")]
    [SerializeField] private string spaceId;
    [SerializeField] private SpaceDescription spaceDescription;
    private bool loading;

    public SpaceDescription SpaceDescription
    {
        get
        {
            LoadSpaceDescriptionInfo();
            return spaceDescription;
        }
    }

    string SceneName => (spaceDescription != null) ? spaceDescription.sceneName : spaceId;

    [Header("Automatically set")]
    [SerializeField] private NetworkRunner runner;

    // Position to spawn at when we come back from this scene
    [SerializeField] private Transform returnPosition;
    [SerializeField] private float returnRadius = 1f;

    [SerializeField] private EnableDisableAnimation fade;
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadSpaceDescriptionInfo();

        if (returnPosition == null)
            returnPosition = transform;

        SceneSpawnManager spawnManager = FindObjectOfType<SceneSpawnManager>(true);

        if (spawnManager)
            spawnManager.RegisterSpawnPosition(spaceId, returnPosition, returnRadius);

        runner = FindFirstObjectByType<NetworkRunner>();

        loading = false;
    }

    void LoadSpaceDescriptionInfo()
    {
        if (spaceDescription && string.IsNullOrEmpty(spaceId))
            spaceId = spaceDescription.spaceId;

        if (spaceDescription == null && !string.IsNullOrEmpty(spaceId))
            spaceDescription = SpaceDescription.FindSpaceDescription(spaceId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<HardwareHand>())
        {
            SwitchScene();
        }
    }

    private async void SwitchScene()
    {
        if(loading) return;
        loading = true;
        
        fade.gameObject.SetActive(true);
        fade.OnContentIn();

        // Espera a animação do fade terminar
        float fadeDuration = fade.animationDuration;
        await Task.Delay((int)(fadeDuration * 1000));

        // Sinaliza que o shutdown é intencional para evitar loop de reconexão
        SessionManager.Instance?.SetIntentionalShutdownFlag();
        SessionPersistence.ClearData(); // Limpa dados de reconexão para evitar redirecionamento automático
        FindFirstObjectByType<ApplicationManager>().isQuitting = true;

        Debug.Log("Disconectado desativado");
        await runner.Shutdown(true);

        // Destrói o runner para evitar duplicação na nova cena
        if (runner != null) DestroyImmediate(runner.gameObject);

        Debug.Log("Loading new scene " + SceneName);
        SpaceRoom.RegisterSpaceRequest(spaceDescription);
        SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
    }
}
