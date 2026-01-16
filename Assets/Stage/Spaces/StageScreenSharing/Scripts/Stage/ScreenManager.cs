using Fusion.Addons.ScreenSharing;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /***
     * 
     * ScreenManager manages the visibility of the screens :
     *  - the offLinescreen is display by default
     *  - the video screen is displayed when a video start. 
     *  - the video is paused and the screenSharing screen is enabled if a screen sharing start
     * 
     ***/

    public class ScreenManager : MonoBehaviour
    {
        public enum Status
        {
            Standby,
            ScreenSharing,
            ScreenSharingWithBackgroundVideoMusic,
            Video
        }

        public Status status = Status.Standby;

        [SerializeField] private VideoControl videoControl;
        [SerializeField] private ScreenSharingScreen screenSharingScreen;
        [SerializeField] private GameObject offLineScreen;

        private void Awake()
        {

            if (screenSharingScreen)
            {
                screenSharingScreen.onScreensharingScreenVisibility.AddListener(OnScreensharingScreenVisibility);

            }
            else
            {
                Debug.LogError("[ScreenManager] screenSharingScreen is not set");
            }

            if (videoControl)
            {
                videoControl.onVideoStart.AddListener(OnVideoStart);
                videoControl.onVideoStop.AddListener(OnVideoStop);
            }
            else
            {
                Debug.LogError("[ScreenManager] VideoControl is not set");
            }
        }

        void UpdateOfflineScreen()
        {
            offLineScreen.SetActive(status == Status.Standby);
        }

        private void OnScreensharingScreenVisibility(bool visible)
        {
            if (visible)
            {
                OnScreenShareStart();
            }
            else
            {
                OnScreenShareStop();
            }
        }

        private void OnScreenShareStart()
        {
            // Stop video if needed
            videoControl.PausePlayer();

            status = Status.ScreenSharing;

            UpdateOfflineScreen();
        }

        private void OnScreenShareStop()
        {
            // Display video if needed - not hidden by screensharing, so nothing to do
            if (status == Status.ScreenSharingWithBackgroundVideoMusic)
            {
                status = Status.Video;
            }
            else if (status == Status.ScreenSharing)
            {
                status = Status.Standby;
            }
            else if (status != Status.Standby)
            {
                Debug.LogError("[ScreenManager] Unexpected screen state");
            }
            UpdateOfflineScreen();
        }

        private void OnVideoStart()
        {
            if (status == Status.ScreenSharing)
            {
                status = Status.ScreenSharingWithBackgroundVideoMusic;
            }
            else if (status == Status.Standby)
            {
                status = Status.Video;
            }
            else
            {
                Debug.LogError("[ScreenManager] Unexpected screen state");
            }
            UpdateOfflineScreen();
        }

        private void OnVideoStop()
        {
            if (status == Status.ScreenSharingWithBackgroundVideoMusic)
            {
                status = Status.ScreenSharing;
            }
            else if (status == Status.Video)
            {
                status = Status.Standby;
            }
            UpdateOfflineScreen();
        }

        private void OnDestroy()
        {
            if (videoControl)
            {
                videoControl.onVideoStart.RemoveListener(OnVideoStart);
                videoControl.onVideoStop.RemoveListener(OnVideoStop);
            }
        }
    }
}
