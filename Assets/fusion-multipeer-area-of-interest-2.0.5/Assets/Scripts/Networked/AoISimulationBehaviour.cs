using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Simulation Behaviour in charge of Registering input and spawning players
/// </summary>
public class AoISimulationBehaviour : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField, Tooltip("Reference to the player prefab.")]
    private NetworkObject playerObject;

    [SerializeField, Tooltip("The radius in which players are spawned.")]
    float playerSpawnRadius;

    /// <summary>
    /// Handles input for the player using a basic WASD  and Up and Down Arrow configuration.
    /// </summary>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        ArcadePlayerInput api = new ArcadePlayerInput();

        if (Input.GetKey(KeyCode.A))
        {
            api.buttons.Set(ArcadePlayerInput.LEFT, true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            api.buttons.Set(ArcadePlayerInput.RIGHT, true);
        }


        if (Input.GetKey(KeyCode.W))
        {
            api.buttons.Set(ArcadePlayerInput.FORWARD, true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            api.buttons.Set(ArcadePlayerInput.BACK, true);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            api.buttons.Set(ArcadePlayerInput.UP, true);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            api.buttons.Set(ArcadePlayerInput.DOWN, true);
        }

        input.Set(api);
    }

    /// <summary>
    /// When a player joins, a new object is spawned.  If the runner can spawn new objects, then this is done.
    /// </summary>
    /// <param name="runner">The NetworkRunner in which the player has joined</param>
    /// <param name="player">THe PlayerRef of the joining player, which is used to provited the InputAuthority for the new player.</param>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.CanSpawn)
        {
            Debug.Log("Runner cannot spawn:  " + runner);
            return;
        }

        if (runner.GameMode != GameMode.Shared || runner.LocalPlayer == player)
        {
            CreateNewPlayer(runner, player);
        }
    }

    /// <summary>
    /// Creates a new player within the radius defined by playerSpawnRadius.
    /// </summary>
    /// <param name="runner">The network runner creating the player</param>
    /// <param name="inputAuth">The newly spawned object's Input Authority</param>
    private void CreateNewPlayer(NetworkRunner runner, PlayerRef inputAuth)
    {
        Vector2 pos = UnityEngine.Random.insideUnitCircle;
        pos *= playerSpawnRadius;
        Vector3 initPos = new Vector3(pos.x, 0f, pos.y);

        runner.Spawn(playerObject, initPos, Quaternion.identity, inputAuth);
    }

    #region UNUSED_INETWORKRUNNERCALLBACKS

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    #endregion
}
