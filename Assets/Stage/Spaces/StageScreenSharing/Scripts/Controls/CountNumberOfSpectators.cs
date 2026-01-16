using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Fusion.Samples.IndustriesComponents;

namespace Fusion.Samples.Stage
{
    /**
     * Update on the control desk the number of user in the scene
     */
    public class CountNumberOfSpectators : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private TextMeshProUGUI numberOfSpectatorTMP;
        [SerializeField] private NetworkRunner runner;
        private Managers managers;
        [SerializeField] private int numberOfPlayers = 0;
        private const string numberOfSpectator = "Número de pessoas: ";

        private void Awake()
        {
            if (!numberOfSpectatorTMP)
                numberOfSpectatorTMP = GetComponent<TextMeshProUGUI>();
        }

        private async void Start()
        {
            // Find the associated runner, if not defined
            if (managers == null)
                managers = Managers.FindInstance();
            if (managers == null)
                Debug.LogError("Managers not found !");
            runner = managers.runner;
            if (runner == null)
            {
                Debug.LogError("Runner not found !");
                return;
            }
            while (!runner.IsConnectedToServer)
            {
                // We should not call AddCallback before the Start() of the runner. But due to the RigSelection disabling it, we may have to may a bit before its Start() occuring
                await Task.Delay(100);
            }
            runner.AddCallbacks(this);
            UpdateNumberOfSpectators();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            UpdateNumberOfSpectators();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            UpdateNumberOfSpectators();
        }

        private void UpdateNumberOfSpectators()
        {
            if (runner.ActivePlayers != null)
            {
                numberOfPlayers = runner.ActivePlayers.Count();
            }
            if (numberOfSpectatorTMP) numberOfSpectatorTMP.text = numberOfSpectator + numberOfPlayers.ToString();
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {

        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
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

        public void OnConnectedToServer(NetworkRunner runner)
        {

        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
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

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {

        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {

        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        #region INetworkRunnerCallbacks (unused)


        #endregion
    }
}
