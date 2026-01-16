using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Spaces;
using Fusion.Sockets;
using Fusion.XR.Shared;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ReconnectionState
{
    public Vector3 position;
    public Vector3 forward;
    public Vector3 cameraForward;

    public ReconnectionState(Vector3 position, Vector3 forward, Vector3 cameraForward)
    {
        this.position = position;
        this.forward = forward;
        this.cameraForward = cameraForward;
    }
}

public class Buga_Reconnection : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runner;
    private float pauseTime;

    [SerializeField]
    private bool debugSimulateReconnection;

    bool alreadyDisconnected = false;

    public static ReconnectionState reconnectionPlayerState;
    public bool started = false;
    public const string SETTINGS_RECONNECTION_POSITION = "ReconnectionPosition";
    public string objectToCheckName;

    void DisableStartPointing()
    {
        GameObject ssmGO = FindObjectOfType<SceneSpawnManager>().gameObject;
        Destroy(ssmGO);

        RandomizeStartPosition rsp = GetComponent<RandomizeStartPosition>();
        rsp.startCenterPosition = transform;
        rsp.randomRadius = 0f;
        rsp.enabled = false;
    }

    void PlacePlayer()
    {
        transform.position = reconnectionPlayerState.position;
        GetComponent<NavMeshAgent>().Warp(reconnectionPlayerState.position);
        transform.forward = reconnectionPlayerState.forward;
    }

    private void Start()
    {
        if (runner == null) runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this);

        if (reconnectionPlayerState != null)
        {
            DisableStartPointing();
            PlacePlayer();
            reconnectionPlayerState = null;
        }

        started = true;
    }

    bool reloading = false;
    bool preventReconnect = false;

    void Update()
    {
        if (GameObject.Find(objectToCheckName) == null)
        {
            reloading = true;
            ReconnectPlayer();
        }

    }

    void ReconnectPlayer()
    {
        ReloadScene();
    }

    async void ReloadScene()
    {
        PlayerPrefs.DeleteKey(SETTINGS_RECONNECTION_POSITION);
        reconnectionPlayerState = new ReconnectionState(transform.position, transform.forward, transform.forward);
        await runner.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        preventReconnect = true;
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
}