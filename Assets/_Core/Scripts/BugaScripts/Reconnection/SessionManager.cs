using System.Collections;
using Fusion;
using Fusion.Addons.ConnectionManagerAddon;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class SessionManager : MonoBehaviour
{
    private static SessionManager instance;
    public static SessionManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SessionManager>();
            return instance;
        }
    }

    [SerializeField] private ConnectionManager connectionManager;
    [SerializeField] private ChangeSceneFromButton changeSceneFromButton;
    private bool _isIntentionalShutdown = false;

    private void Awake()
    {
        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<ConnectionManager>();
        }
    }

    private void Start()
    {
        if (SessionPersistence.HasReconnectionData())
        {
            Debug.Log("[SessionManager] Dados de reconexão encontrados. Iniciando reconexão automática.");
            connectionManager.connectOnStart = false;
            AttemptReconnection();
        }
    }

    private async void AttemptReconnection()
    {
        var data = SessionPersistence.LoadData();
        if (data == null) return;
        
        SessionPersistence.DataToRestore = data;

        if (!string.IsNullOrEmpty(data.SceneName) && !string.IsNullOrEmpty(data.SessionName))
        {
            SessionPersistence.LoadScene();
            changeSceneFromButton.JoinServer(data.SessionName, data.SceneName);   
            return;
        }
        
        // connectionManager.roomName = data.SessionName;
        // await connectionManager.Connect();

        if (SessionPersistence.DataToRestore != null)
        {
            transform.position = SessionPersistence.DataToRestore.PlayerPosition;
            transform.rotation = SessionPersistence.DataToRestore.PlayerRotation;
            SessionPersistence.DataToRestore = null;
        }
        SessionPersistence.ClearData();   
    }

    // --- MÉTODOS DE CALLBACK MODIFICADOS ---

    public void HandlePlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer && !_isIntentionalShutdown)
        {
            TriggerReconnectionProcess();
        }
    }

    public void HandleDisconnectedFromServer()
    {
        TriggerReconnectionProcess();
    }

    public void HandleShutdown()
    {
        if (!_isIntentionalShutdown)
        {
            TriggerReconnectionProcess();
        }
        else
        {
            _isIntentionalShutdown = false;
        }
    }

    /// <summary>
    /// Salva os dados e delega a tarefa de esperar a internet para o ReconnectionService.
    /// </summary>
    private void TriggerReconnectionProcess()
    {
        Debug.Log("[SessionManager] Conexão perdida! Salvando estado e acionando o ReconnectionService.");
        SaveReconnectionState();
        BeginReconnectionCheck();
    }
    
    public void BeginReconnectionCheck()
    {
        StopAllCoroutines();
        StartCoroutine(ReconnectionLoopCoroutine());
    }
    
    private IEnumerator ReconnectionLoopCoroutine()
    {
        while (true)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                var data = SessionPersistence.LoadData();
                if (data != null && !string.IsNullOrEmpty(data.SceneName))
                {
                    Debug.Log($"[ReconnectionService] Internet detectada! Recarregando a cena '{data.SceneName}' para reconectar...");
                    SceneManager.LoadScene(data.SceneName);
                    yield break;
                }
                else
                {
                    Debug.LogError("[ReconnectionService] Dados de reconexão ou nome da cena inválidos. Abortando reconexão.");
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(1);
        }
    }
    
    #region Métodos de Suporte (Sem Alterações)
    public void SetIntentionalShutdownFlag()
    {
        _isIntentionalShutdown = true;
    }

    private void OnApplicationQuit()
    {
        _isIntentionalShutdown = true;
        SessionPersistence.ClearData();
    }
    
    private void SaveReconnectionState()
    {
        var runner = connectionManager.runner;
        if (runner == null || !runner.IsRunning) return;
        
        SessionPersistence.SaveData(
            runner.SessionInfo.Name,
            SceneManager.GetActiveScene().name,
            transform.position,
            transform.rotation
        );
    }
    #endregion
}