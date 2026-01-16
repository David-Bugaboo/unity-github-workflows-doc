using Fusion.Addons.Avatar;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Synchronize an attendee status (saying if the user want voice access to the stage)
     */
    [RequireComponent(typeof(StageNetworkRig))]
    public class VoiceableAttendee : NetworkBehaviour, IAttendee
    {
        AttendeeRegistry attendeeRegistry;
        [SerializeField] private GameObject voiceRequestingSpectatorVisualFeedback;
        [SerializeField] private GameObject voicedSpectatorSpectatorVisualFeedback;
        [SerializeField] private GameObject mutedSpectatorSpectatorVisualFeedback;

        StageNetworkRig stageNetworkRig;
        PublicSpeechHandler publicSpeechHandler;

        private void Awake()
        {
            publicSpeechHandler = GetComponent<PublicSpeechHandler>();
            if(publicSpeechHandler == null)
            {
                Debug.LogError("Missing AvatarLODConfigurationHandler");
            }
            if (voiceRequestingSpectatorVisualFeedback == null || voicedSpectatorSpectatorVisualFeedback == null || mutedSpectatorSpectatorVisualFeedback == null)
            {
                Debug.LogError("Visual feedback not configured");
            }

            stageNetworkRig = GetComponent<StageNetworkRig>();

            voiceRequestingSpectatorVisualFeedback.SetActive(false);
            voicedSpectatorSpectatorVisualFeedback.SetActive(false);
            mutedSpectatorSpectatorVisualFeedback.SetActive(false);
        }

        public override void Spawned()
        {
            base.Spawned();

            if (!attendeeRegistry) attendeeRegistry = FindObjectOfType<AttendeeRegistry>(true);
            if (attendeeRegistry)
            {
                attendeeRegistry.RegisterAttendee(this);
            }
            else
                Debug.LogError("AttendeeRegistry not found");

            UpdateAttendeeStatus();
            renderChangeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
        }

        private void OnDestroy()
        {
            if (attendeeRegistry)
            {
                attendeeRegistry.UnRegisterAttendee(this);
            }
        }

        #region IAttendee
        [Networked]
        public AttendeeStatus AttendeeStatus { get; set; }

        public PlayerRef AttendeePlayer => Object.StateAuthority;

        ChangeDetector renderChangeDetector;

        bool TryDetectAttendeeStatusChange()
        {
            foreach (var changedNetworkedVarName in renderChangeDetector.DetectChanges(this))
            {
                if (changedNetworkedVarName == nameof(AttendeeStatus))
                {
                    return true;
                }
            }
            return false;
        }


        public override void Render()
        {
            // Check if the SelectedRequestingAttendeeInfo changed
            if (TryDetectAttendeeStatusChange())
            {
                UpdateAttendeeStatus();
            }
        }

        public string AttendeeName
        {
            get
            {
                if (stageNetworkRig.SeatStatus.seated)
                {
                    string username= stageNetworkRig.GetComponent<UserInfo>().UserName.ToString();
                    if (username!=null && username !="")
                        return username;
                    else
                        return $"Seat {stageNetworkRig.SeatStatus.seatId}";

                }
                return $"Unseated #{Object.StateAuthority.PlayerId}";
            }
        }

        public void ChangeAttendeeStatus(AttendeeStatus status)
        {
            // Changed received from the registry
            if (!Object.HasStateAuthority)
            {
                Debug.LogError("Unable to change  attendee status");
                return;
            }
            Debug.Log($"Changed received from the registry: {status}");
            AttendeeStatus = status;
        }
        #endregion

        void UpdateAttendeeStatus()
        {
            if (!attendeeRegistry) return;

            attendeeRegistry.OnAttendeeUpdate(this);

            // If we have been voiced, we ask all the dynamic audio member (and so including the one for the local user that will handle the actual voice settings) to listen to us
            // Otherwise, we ask to leave this whitelist
            if (AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                publicSpeechHandler.SetIsOnMic(true);
            }
            else
            {
                publicSpeechHandler.SetIsOnMic(false);
            }

            UpdateSpectatorVisualFeedback();
        }

        private void UpdateSpectatorVisualFeedback()
        {
            if (AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                voiceRequestingSpectatorVisualFeedback.SetActive(false);
                voicedSpectatorSpectatorVisualFeedback.SetActive(true);
                mutedSpectatorSpectatorVisualFeedback.SetActive(false);
            }
            else if (AttendeeStatus == AttendeeStatus.MutedSpectator)
            {
                voiceRequestingSpectatorVisualFeedback.SetActive(false);
                voicedSpectatorSpectatorVisualFeedback.SetActive(false);
                mutedSpectatorSpectatorVisualFeedback.SetActive(true);
            }
            else if (AttendeeStatus == AttendeeStatus.VoiceRequestingSpectator)
            {
                voiceRequestingSpectatorVisualFeedback.SetActive(true);
                voicedSpectatorSpectatorVisualFeedback.SetActive(false);
                mutedSpectatorSpectatorVisualFeedback.SetActive(false);
            }
            else
            {
                voiceRequestingSpectatorVisualFeedback.SetActive(false);
                voicedSpectatorSpectatorVisualFeedback.SetActive(false);
                mutedSpectatorSpectatorVisualFeedback.SetActive(false);
            }
        }
    }
}
