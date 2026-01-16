using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputProvider : MonoBehaviour, INetworkRunnerCallbacks
{
    private PlayerControls _playerControls;
    private Transform _cameraTransform;

    private void Awake()
    {
        // Inicializa o sistema de input
        _playerControls = new PlayerControls();
        _playerControls.Enable();
    }

    private void OnEnable()
    {
        if (TryGetComponent<NetworkRunner>(out var runner))
        {
            runner.AddCallbacks(this);
        }
    }

    private void OnDisable()
    {
        if (TryGetComponent<NetworkRunner>(out var runner))
        {
            runner.RemoveCallbacks(this);
        }
        _playerControls.Disable();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new NetworkInputData();

        // Pega o input Vector2 do teclado (WASD)
        Vector2 moveInput = _playerControls.Player.Move.ReadValue<Vector2>();

        // Precisamos da referência da câmera para que o movimento seja relativo a ela.
        // "W" deve mover para a frente da câmera, não para o eixo Z do mundo.
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_cameraTransform != null)
        {
            // Calcula a direção "para frente" e "para a direita" da câmera.
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            // Zera o componente Y para que o movimento seja apenas no plano horizontal.
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Combina as direções com o input do teclado para obter o vetor de movimento final.
            Vector3 desiredDirection = (forward * moveInput.y + right * moveInput.x);
            myInput.direction = desiredDirection;
        }
        
        // Define o input para a rede.
        input.Set(myInput);
    }
    
    // ... (Cole aqui os outros métodos vazios da interface INetworkRunnerCallbacks como na resposta anterior)
    #region Unused Callbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
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