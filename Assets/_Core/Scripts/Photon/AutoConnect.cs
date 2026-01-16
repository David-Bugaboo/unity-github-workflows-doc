using System;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.ConnectionManagerAddon;
using Fusion.Sockets;
using TMPro;
using UnityEngine;

public class AutoConnect : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Configurações da Sala")]
    [Tooltip("AppId base para a conexão. Deve ser o mesmo da build principal.")]
    public string appId = "SampleFusionVR0.1";
    public bool addVersionToAppId = true;
    
    private string spaceId = "";
    private string groupId = "";
    private string instanceId = "";
    
    [Header("Referências da Cena")]
    [Tooltip("Arraste para cá o GameObject que contém o ConnectionManager e o Runner.")]
    [SerializeField]
    private ConnectionManager connectionManager;
    
    [Tooltip("Arraste aqui o objeto da cena que contém o script ScreenSharingEmitter.")]
    [SerializeField]
    private ScreenSharingEmitter screenEmitter;
    public string RoomName => string.Join("-", new string[]{appId, spaceId, groupId, instanceId}.Where(s => !string.IsNullOrEmpty(s)));
 
    [Header("Debug")]
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI roomName;
    
    async void Start()
    {
        LogToUI("Iniciando AutoConnect...");

        // --- 1. Validações Iniciais ---
        if (connectionManager == null || screenEmitter == null)
        {
            LogToUI("ERRO: ConnectionManager ou ScreenEmitter não associados no Inspector!");
            Application.Quit();
            return;
        }

        // --- 2. Leitura dos Argumentos de Linha de Comando ---
        LogToUI("Lendo argumentos de linha de comando...");
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--spaceId") spaceId = args[i + 1];
            else if (args[i] == "--groupId") groupId = args[i + 1];
            else if (args[i] == "--instanceId") instanceId = args[i + 1];
        }

        if (string.IsNullOrEmpty(spaceId))
        {
            LogToUI($"ERRO: Nenhum 'spaceId' foi fornecido via argumentos. Fechando.");
            await Task.Delay(5000); // Espera 5s para dar tempo de ler o erro
            Application.Quit();
            return;
        }
        LogToUI($"Argumentos recebidos: spaceId={spaceId}, groupId={groupId}, instanceId={instanceId}");

        if (addVersionToAppId)
        {
            appId += Application.version;
        }

        // --- 3. Configura e Inicia a Conexão ---
        if (roomName != null) roomName.text = $"Sala: {RoomName}";
        LogToUI($"Nome da sala montado: {RoomName}");
        LogToUI("Configurando e conectando...");
        await ConfigureAndConnect();
    }
    
    private void LogToUI(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text += message + "\n";
        }
    }
    
    private async Task ConfigureAndConnect()
    {
        connectionManager.connectOnStart = false;
        connectionManager.connectionCriterias = ConnectionManager.ConnectionCriterias.RoomName;
        connectionManager.roomName = this.RoomName;

        // Adiciona este script como um ouvinte dos eventos do runner
        connectionManager.runner.AddCallbacks(this);
        
        await connectionManager.Connect();
    }


    #region Callbacks do Fusion
    
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Application.Quit(); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { Application.Quit(); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Application.Quit(); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    #endregion
}