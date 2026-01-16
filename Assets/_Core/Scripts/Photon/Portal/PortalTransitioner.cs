using Fusion;
using Fusion.Addons.Spaces;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTransitioner : MonoBehaviour
{
    [Header("Configuração do Destino")]
    [Tooltip("A descrição do espaço para o qual este portal levará. Contém o ID do espaço e o nome da cena.")]
    [SerializeField] private SpaceDescription spaceDescription;

    [Header("Ponto de Retorno (Opcional)")]
    [Tooltip("Define para onde o jogador deve retornar se voltar deste espaço. Se vazio, usa a posição do portal.")]
    [SerializeField] private Transform returnPosition;
    [SerializeField] private float returnRadius = 1f;
    
    private NetworkRunner activeRunner;
    private bool isSwitchingScene = false;

    #region Ciclo de Vida e Gerenciamento de Cena

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (returnPosition == null)
        {
            returnPosition = transform;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSwitchingScene = false;
        FindActiveRunner();
        RegisterSpawnPoint();
    }

    #endregion

    #region Lógica do Portal

    private void OnTriggerEnter(Collider other)
    {
        if (isSwitchingScene) return;
        
        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();
        if (networkObject != null && networkObject.HasInputAuthority)
        {
            SwitchToSpaceAsync();
        }
    }

    private async void SwitchToSpaceAsync()
    {
        if (activeRunner == null)
        {
            Debug.LogError("NetworkRunner não foi encontrado! A transição não pode continuar.", this);
            return;
        }
        
        if (spaceDescription == null)
        {
            Debug.LogError("SpaceDescription de destino não foi configurado neste portal!", this);
            return;
        }
        
        isSwitchingScene = true;

        Debug.Log($"[SpacePortal] Iniciando transição para o espaço '{spaceDescription.spaceId}'. Desligando o runner...");

        // Sinaliza que o shutdown é intencional para evitar loop de reconexão
        SessionManager.Instance?.SetIntentionalShutdownFlag();
        SessionPersistence.ClearData(); // Limpa dados de reconexão para evitar redirecionamento automático

        var appManager = FindFirstObjectByType<ApplicationManager>();
        if (appManager != null) appManager.isQuitting = true;

        await activeRunner.Shutdown(true);

        // Destrói o runner para evitar duplicação na nova cena
        if (activeRunner != null) DestroyImmediate(activeRunner.gameObject);

        Debug.Log($"[SpacePortal] Runner desligado. Registrando o pedido e carregando a cena '{spaceDescription.sceneName}'...");

        SpaceRoom.RegisterSpaceRequest(spaceDescription);
        
        SceneManager.LoadScene(spaceDescription.sceneName, LoadSceneMode.Single);
    }

    #endregion

    #region Métodos de Suporte
    
    private void FindActiveRunner()
    {
        activeRunner = FindFirstObjectByType<NetworkRunner>(0);
        if (activeRunner != null)
        {
            Debug.Log("[SpacePortal] NetworkRunner ativo encontrado e referenciado.", this);
        }
    }
    
    private void RegisterSpawnPoint()
    {
        SceneSpawnManager spawnManager = FindFirstObjectByType<SceneSpawnManager>(0);
        if (spawnManager && spaceDescription != null)
        {
            spawnManager.RegisterSpawnPosition(spaceDescription.spaceId, returnPosition, returnRadius);
            Debug.Log($"[SpacePortal] Ponto de retorno para o espaço '{spaceDescription.spaceId}' registrado.", this);
        }
    }

    #endregion
}