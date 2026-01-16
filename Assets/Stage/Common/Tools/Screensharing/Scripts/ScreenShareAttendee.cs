using Fusion.Addons.Avatar;
using UnityEngine;


namespace Fusion.Samples.Stage
{
    /**
     * 
     * Synchronize a screen share user status (saying if the user wants to share its screen)
     * 
     **/

    public class ScreenShareAttendee : NetworkBehaviour, IScreenShare
    {
        [SerializeField] ScreenShareRegistry screenShareRegistry;
                
        [SerializeField] private ScreenSharingEmitter screenSharingEmitter;

        UserInfo userInfo;

        ChangeDetector changeDetector;

        private void Awake()
        {
            if (!screenSharingEmitter) screenSharingEmitter = FindObjectOfType<ScreenSharingEmitter>();
            if (!screenSharingEmitter)
                Debug.LogWarning("Can not find screenSharingEmitter");

            userInfo = GetComponent<UserInfo>();
            userInfo.onUserNameChange.AddListener(OnUserNameChange);
        }

        private void OnUserNameChange()
        {
            UpdateScreenShareStatus();
        }

        public override void Spawned()
        {
            base.Spawned();

            if (!screenShareRegistry) screenShareRegistry = FindObjectOfType<ScreenShareRegistry>(true);
            if (screenShareRegistry)
            {
                screenShareRegistry.RegisterScreenShare(this);
            }
            else Debug.LogError("screenShareRegistry not found !");
 
            changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
            UpdateScreenShareStatus();
        }


        public override void Render()
        {
            foreach (var changedNetworkedVarName in changeDetector.DetectChanges(this))
            {
                if (changedNetworkedVarName == nameof(ScreenShareStatus))
                {
                    UpdateScreenShareStatus();
                }
            }
        }

        private void OnDestroy()
        {
            if (screenShareRegistry)
            {
                screenShareRegistry.UnRegisterScreenShare(this);
            }
        }

        #region IAttendee
        [Networked]
        public ScreenShareStatus ScreenShareStatus { get; set; }

        public PlayerRef ScreenSharePlayer => Object.StateAuthority;

        public string ScreenShareName
        {
            get
            {

                string username = userInfo.UserName.ToString();
                if (username != null && username != "")
                    return userInfo.UserName.ToString();
                else
                    return "Username not defined";
            }
        }



        public void ChangeScreenShareStatus(ScreenShareStatus status)
        {
            // Changed received from the registry
            if (!Object.HasStateAuthority)
            {
                Debug.LogError("Unable to change  ScreenShare status");
                return;
            }
            Debug.Log($"Changed received from the registry: {status}");
            ScreenShareStatus = status;
        }
        #endregion




        void UpdateScreenShareStatus()
        {
            if (!screenShareRegistry)
            {
                Debug.Log("screenShareRegistry not found !");

                return;
            }

            screenShareRegistry.OnScreenShareUpdate(this);

            if (!Object.HasStateAuthority) return;
            UpdateLocalScreenShareStatus();
        }

        void UpdateLocalScreenShareStatus() {
            if (!screenSharingEmitter) return; 

            if (ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
            {
                Debug.Log("Starting Screen Sharing !");
                screenSharingEmitter.ConnectScreenSharing();
            }
            else
            {
                if (ScreenShareStatus == ScreenShareStatus.ScreenShareRequested)
                {
                    Debug.Log("Screen Sharing Requested !");
                }
                else
                if (ScreenShareStatus == ScreenShareStatus.ScreenShareRejected)
                {
                    Debug.Log("Screen Sharing Rejected !");

                }
                else if (ScreenShareStatus == ScreenShareStatus.ScreenShareStopped)
                {
                    Debug.Log("Screen Sharing Stopped !");

                }
                else if (ScreenShareStatus == ScreenShareStatus.NoScreenShareRequested)
                {
                    Debug.Log("No Screen Share requested !");

                }
                screenSharingEmitter.DisconnectScreenSharing();
            }
        }
    }
}
