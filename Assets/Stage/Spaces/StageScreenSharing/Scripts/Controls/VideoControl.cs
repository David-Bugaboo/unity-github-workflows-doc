using Fusion.Addons.ExtendedRigSelectionAddon;
using Fusion.XR.Shared;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Fusion.Samples.Stage
{
    /**
     * Synchronize the video player through a [Network] var containing a PlayState, holding play state info
     * To be able to change this variable, and so to control the synchronized status, the player touching the control desk becomes the StateAuthority.
     * VideoControls listens the ScreenShareRequestHandler to stop the video if a screen share starts
     */
    public class VideoControl : NetworkBehaviour, IStateAuthorityChanged
    {
        public float maxDesyncBetweenControllerAndClients = 10;
        [SerializeField] private TextMeshProUGUI buttonPlayPause;
        [SerializeField] private Slider videoPositionSlider;
        [SerializeField] private TextMeshProUGUI timerTMP;
        [SerializeField] private TextMeshProUGUI videoDurationTMP;
        //[SerializeField] private RecorderAppRigSelection recorderAppRigSelection;
        [SerializeField] private ExtendedRigSelection extendedRigSelection;
        public UnityEvent onVideoStart;
        public UnityEvent onVideoStop;
        public UnityEvent onAuthorityChange;

        [System.Serializable]
        public struct PlayState : INetworkStruct
        {
            public double lastPlayTime;
            public bool isPlaying;
        }
        public PlayState _status;

        [Networked]
        public PlayState Status { get; set; }
        public VideoPlayer videoPlayer;

        [Networked]
        public bool NoProperAuthority { get; set; } = false;

        private int minutes = 0;
        private int seconds = 0;

        private bool syncingSlider = false;
        private bool isRequestingAuthority = false;

        ChangeDetector changeDetector;


        #region Change detection
        public override void Spawned()
        {
            base.Spawned();
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            UpdatePlayerWithStatus();
        }

        public override void Render()
        {
            base.Render();

            foreach (var changedVarName in changeDetector.DetectChanges(this))
            {
                if (changedVarName == nameof(Status))
                {
                    UpdatePlayerWithStatus();
                }
            }
        }
        #endregion

        private void Awake()
        {
            videoPositionSlider.value = 0f;
            videoPositionSlider.onValueChanged.AddListener(SliderPositionChanged);
        }

        float lastManualSliderValue;
        float lastManuelSliderUpdate = -1;
        bool restoringManualSliderValue = false;
        private async void SliderPositionChanged(float value)
        {
            if (restoringManualSliderValue) return;

            if (!syncingSlider)
            {
                // slider updated by user using UI
                if (Object.StateAuthority != Runner.LocalPlayer)
                {
                    if (isRequestingAuthority) return;
                    if (!await RequestAuthority()) return;
                }

                UpdateStatus(isPlaying: videoPlayer.isPlaying, videoPlayer.length * value);
                lastManualSliderValue = value;
                lastManuelSliderUpdate = Time.time;
            }
            else
            {
                // slider updated due to Status changed
                if (lastManuelSliderUpdate != -1 && Time.time < (lastManuelSliderUpdate + 1))
                {
                    // we do not want to override new position by an automatic status update
                    restoringManualSliderValue = true;
                    videoPositionSlider.value = lastManualSliderValue;
                    restoringManualSliderValue = false;
                }
                else
                    lastManuelSliderUpdate = -1;
            }
        }

        private async Task<bool> RequestAuthority()
        {
            //Debug.LogError("RequestAuthority");
            isRequestingAuthority = true;
            if (!await Object.WaitForStateAuthority()) return false;
            isRequestingAuthority = false;
            return true;
        }

        [ContextMenu("Do TogglePlay")]
        public async void TogglePlay()
        {

            if (Object.StateAuthority != Runner.LocalPlayer)
            {
                if (!await RequestAuthority()) return;
            }

            if (videoPlayer.isPlaying)
            {
                // Pause
                Debug.Log("Video TogglePlay Pause");
                LocalPausePlayer();
                UpdateStatusWithVideoPlayer();
            }
            else
            {
                // Play
                Debug.Log("Video TogglePlay Resume");
                ResumePlayer();
                UpdateStatusWithVideoPlayer();
            }
        }

        public void PausePlayer()
        {
            LocalPausePlayer();
            UpdateStatusWithVideoPlayer();
        }

        // Locally pause the player and updates the display
        void LocalPausePlayer()
        {
            Debug.Log($"Video PausePlayer");
            videoPlayer.Pause();
            buttonPlayPause.text = "Play";
        }

        private void ResetPlayer()
        {
            Debug.Log($"Video ResetPlayer");
            // can not use videoPlayer.Stop() because of crash on Android
            videoPlayer.Pause();
            buttonPlayPause.text = "Play";
            onVideoStop.Invoke();
        }

        private void ResumePlayer()
        {
            //if (recorderAppRigSelection.appKind != RecorderAppRigSelection.AppKind.Recorder) // we don't want to play the video on the recorder
            if (extendedRigSelection.selectedRig.name != "Recorder")
            {
                if (!videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                    buttonPlayPause.text = "Pause";
                    onVideoStart.Invoke();
                }
            }
        }

        // UPdate Status (for the state authority only) based on the player local state
        public void UpdateStatusWithVideoPlayer()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }
            UpdateStatus(videoPlayer.isPlaying, videoPlayer.time);
        }

        void UpdateStatus(bool isPlaying, double time)
        {
            _status.isPlaying = isPlaying;
            _status.lastPlayTime = time;
            Status = _status;
            UpdateVideoSlider();
        }

        private void UpdateVideoSlider()
        {
            syncingSlider = true;
            if (videoPlayer.length != 0)
            {
                // We use status lastPlayTime, as videoPlayer.time might not be up to date in all cases (when resetting for instance)
                double elapsedSeconds = Status.lastPlayTime;
                double videoPosition = elapsedSeconds / videoPlayer.length;
                videoPositionSlider.value = (float)videoPosition;

                minutes = Mathf.FloorToInt((float)elapsedSeconds / 60);
                seconds = Mathf.FloorToInt((float)elapsedSeconds - minutes * 60);
                timerTMP.text = string.Format("{0:0}:{1:00}", minutes, seconds);
                videoDurationTMP.text = string.Format("{0:0}:{1:00}", Mathf.FloorToInt((float)videoPlayer.length / 60), Mathf.FloorToInt((float)videoPlayer.length % 60));
            }
            syncingSlider = false;
        }

        public async void ResetVideo()
        {
            if (Object.StateAuthority != Runner.LocalPlayer)
            {
                if (!await Object.WaitForStateAuthority()) return;
            }

            Debug.Log($"Video ResetVideo");
            UpdateStatus(isPlaying: false, time: 0);
            UpdatePlayerWithStatus();
        }

        bool firstStart = true;
        void UpdatePlayerWithStatus()
        {
            //Debug.LogError("UpdatePlayerWithStatus "+Status.isPlaying+" " +Status.lastPlayTime+ " "+Object.StateAuthority);
            bool mustResetPlayer = false;
            if (!Status.isPlaying)
            {
                // Determine if the user press the Reset button
                if (Status.lastPlayTime == 0)
                    mustResetPlayer = true;

                // Stop the video player if the user press the Reset button
                if (mustResetPlayer)
                    ResetPlayer();
                else
                {
                    // Video is paused, but we have to enable the video screen first in case the user join the room when video is paused
                    if (firstStart)
                    {
                        firstStart = false;
                        ResumePlayer();
                    }
                    LocalPausePlayer();
                }
                //Debug.LogError($"Pausing {videoPlayer.time} -> {Status.lastPlayTime}");
                videoPlayer.time = Status.lastPlayTime;
            }
            else
            {
                // We are already playing. We only "jump" in time if we are too far from the requested timestamp
                if (Mathf.Abs((float)(Status.lastPlayTime - videoPlayer.time)) > maxDesyncBetweenControllerAndClients)
                {
                    //Debug.LogError($"Desync {videoPlayer.time} -> {Status.lastPlayTime}");
                    videoPlayer.time = Status.lastPlayTime;
                }

                ResumePlayer();

                // we have to consume firstStart it when video is playing when joining
                if (firstStart)
                    firstStart = false;
            }

            UpdateVideoSlider();
        }


        private float lastUpdateStatusWithVideoPlayer;
        private float updateStatusWithVideoPlayerPeriod = 1;

        private void Update()
        {
            if (videoPlayer.isPlaying && Object.HasStateAuthority)
            {
                // no need to update Status at each update
                if (Time.time > lastUpdateStatusWithVideoPlayer + updateStatusWithVideoPlayerPeriod)
                {
                    UpdateStatusWithVideoPlayer();
                    lastUpdateStatusWithVideoPlayer = Time.time;
                }
            }
        }

        public async override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (NoProperAuthority && (!extendedRigSelection || extendedRigSelection.selectedRig.name != "Recorder"))
            {
                // We don't want the recorder (which does not play the video) to have the authority over controls)
                //Debug.LogError("We don't want the recorder (which does not play the video) to have the authority over controls)");
                await RequestAuthority();
                NoProperAuthority = false;
            }
        }

        #region IStateAuthorityChanged
        public void StateAuthorityChanged()
        {
            if (onAuthorityChange != null) onAuthorityChange.Invoke();

            //Debug.LogError("StateAuthorityChanged "+Object.StateAuthority);
            if (Object && Object.HasStateAuthority && extendedRigSelection && extendedRigSelection.selectedRig.name == "Recorder")
            {
                if (Runner.ActivePlayers.Count() == 1)
                {
                    // Only the recorder remains in the scene: we stop the player
                    //Debug.LogError("Only the recorder remains in the scene: we stop the player and keep the current progress "+ Status.lastPlayTime);
                    UpdateStatus(isPlaying: false, Status.lastPlayTime);
                }
                NoProperAuthority = true;
            }
        }
        #endregion
    }
}
