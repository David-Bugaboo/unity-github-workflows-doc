using Fusion.XR.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    public struct AttendeeInfo : INetworkStruct
    {
        public AttendeeStatus attendeeStatus;
        public PlayerRef attendeePlayer;
    }

    /**
     * Store all attendee that request voice access to the stage.
     * Select one attendee (the first one) to be displayed on the control desk
     * Broadcast to the network any attendee AttendeeStatus change request (for instance when validating a voice access request) so that the client owning the attendee can change its status
     */
    public class AttendeeRequestHandler : NetworkBehaviour, IAttendeeRegistryListener
    {
        public List<IAttendee> requestingAttendees = new List<IAttendee>();

        [Networked]
        public AttendeeInfo SelectedRequestingAttendeeInfo { get; set; }
        public IAttendee selectedRequestingAttendee;
        public GameObject requestPanel;
        public TMPro.TextMeshProUGUI username;
        public TMPro.TextMeshProUGUI headline;
        public TMPro.TextMeshProUGUI okButtonText;

        public AttendeeRegistry attendeeRegistry;

        ChangeDetector renderChangeDetector;

        void Awake()
        {
            if(attendeeRegistry == null)
            {
                attendeeRegistry = FindObjectOfType<AttendeeRegistry>(true);
            }
            attendeeRegistry.RegisterListener(this);
            UpdatePanel();
        }


        bool TryDetectRequestingAttendeeChange()
        {
            foreach (var changedNetworkedVarName in renderChangeDetector.DetectChanges(this))
            {
                if (changedNetworkedVarName == nameof(SelectedRequestingAttendeeInfo))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Spawned()
        {
            base.Spawned();
            Debug.Log($"[AttendeeRequestHandler] Spawned called. HasStateAuthority: {Object.HasStateAuthority}");
            DidChangeSelectedRequestingAttendee();
            renderChangeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
        }

        public override void Render()
        {
            if (renderChangeDetector == null)
            {
                Debug.LogWarning("[AttendeeRequestHandler] Render called but renderChangeDetector is NULL!");
                return;
            }

            // Check if the SelectedRequestingAttendeeInfo changed
            if (TryDetectRequestingAttendeeChange())
            {
                DidChangeSelectedRequestingAttendee();
            }
        }

        public void DidChangeSelectedRequestingAttendee()
        {
            Debug.Log($"[AttendeeRequestHandler] DidChangeSelectedRequestingAttendee called. Status: {SelectedRequestingAttendeeInfo.attendeeStatus}, Player: {SelectedRequestingAttendeeInfo.attendeePlayer}");

            if (SelectedRequestingAttendeeInfo.attendeePlayer == PlayerRef.None)
            {
                selectedRequestingAttendee = null;
            }
            else
            {
                Debug.Log($"[AttendeeRequestHandler] Looking for attendee. Registry count: {attendeeRegistry.attendees.Count}, LocalPlayer: {Runner.LocalPlayer}");

                foreach (var attendee in attendeeRegistry.attendees)
                {
                    if (attendee.AttendeePlayer == SelectedRequestingAttendeeInfo.attendeePlayer)
                    {
                        selectedRequestingAttendee = attendee;
                        Debug.Log($"[AttendeeRequestHandler] Found attendee. AttendeePlayer: {selectedRequestingAttendee.AttendeePlayer}, LocalPlayer: {Runner.LocalPlayer}, IsMatch: {selectedRequestingAttendee.AttendeePlayer == Runner.LocalPlayer}");

                        if (selectedRequestingAttendee.AttendeePlayer == Runner.LocalPlayer)
                        {
                            // We can only change the [Networked] var AttendeeStatus on the state authority of its NetworkObject,
                            //  so while we are broadcasting the change, we find the player that can actually change the AttendeeStatus
                            Debug.Log($"[AttendeeRequestHandler] Calling ChangeAttendeeStatus with status: {SelectedRequestingAttendeeInfo.attendeeStatus}");
                            selectedRequestingAttendee.ChangeAttendeeStatus(SelectedRequestingAttendeeInfo.attendeeStatus);
                        }
                        break;
                    }
                }
            }

            UpdatePanel();
        }

        #region IAttendeeRegistryListener
        public async void OnAttendeeUpdate(AttendeeRegistry registry, IAttendee attendee)
        {
            bool isStatusDisplayedOnUI = attendee.AttendeeStatus == AttendeeStatus.VoiceRequestingSpectator || attendee.AttendeeStatus == AttendeeStatus.VoicedSpectator || attendee.AttendeeStatus == AttendeeStatus.MutedSpectator;
            if (isStatusDisplayedOnUI)
            {
                if (requestingAttendees.Contains(attendee) == false)
                {
                    requestingAttendees.Add(attendee);
                    await UpdateSelectedAttendee();
                    //TODO Update regularly to update name / seat if needed
                }
            }
            else if (requestingAttendees.Contains(attendee))
            {
                requestingAttendees.Remove(attendee);
                await UpdateSelectedAttendee();
            }
            UpdatePanel();
        }

        public async void OnAttendeeUnregister(AttendeeRegistry registry, IAttendee attendee)
        {
            if (requestingAttendees.Contains(attendee)) requestingAttendees.Remove(attendee);
            await UpdateSelectedAttendee();
            UpdatePanel();
        }

        public void OnAttendeeRegister(AttendeeRegistry registry, IAttendee attendee) { }
        #endregion

        async Task UpdateSelectedAttendee()
        {
            IAttendee attendee = null;
            if (requestingAttendees.Count != 0) {
                attendee = requestingAttendees[0];
            }
            selectedRequestingAttendee = attendee;

            await Object.EnsureHasStateAuthority();
            if (!Object.HasStateAuthority) return;
            if (selectedRequestingAttendee == null)
            {
                SelectedRequestingAttendeeInfo = new AttendeeInfo { attendeePlayer = PlayerRef.None };
            }
            else
            {
                SelectedRequestingAttendeeInfo = new AttendeeInfo { attendeeStatus = attendee.AttendeeStatus, attendeePlayer = attendee.AttendeePlayer };
            }
        }

        void UpdatePanel()
        {
            headline.text = $"Discussion Requests : {requestingAttendees.Count}";
            Debug.Log("requestingAttendees: "+ requestingAttendees.Count);
            if(requestingAttendees.Count == 0 || selectedRequestingAttendee == null)
            {
                requestPanel.SetActive(false);
                headline.gameObject.SetActive(false);
                return;
            }
            requestPanel.SetActive(true);
            if(selectedRequestingAttendee.AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                okButtonText.text = "MUTE";
            }
            else
            {
                okButtonText.text = "OK";
            }
            headline.gameObject.SetActive(true);
            username.text = selectedRequestingAttendee.AttendeeName;
        }

        public async void OnOk()
        {
            if (!Object.HasStateAuthority &&!await Object.WaitForStateAuthority()) return;
            await UpdateSelectedAttendee();// Not really needed

            if(selectedRequestingAttendee.AttendeeStatus == AttendeeStatus.VoiceRequestingSpectator || selectedRequestingAttendee.AttendeeStatus == AttendeeStatus.MutedSpectator)
            {
                ChangeSelectAttendeeStatus(AttendeeStatus.VoicedSpectator);
            }
            else if (selectedRequestingAttendee.AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                // Disable the voiced attendee (but keep their request)
                ChangeSelectAttendeeStatus(AttendeeStatus.MutedSpectator);
            }
            UpdatePanel();
        }

        public async void OnRejected()
        {
            if (!Object.HasStateAuthority && !await Object.WaitForStateAuthority()) return;
            await UpdateSelectedAttendee();// Not really needed

            ChangeSelectAttendeeStatus(AttendeeStatus.RejectedVoiceSpectator);
            requestingAttendees.Remove(selectedRequestingAttendee);
            UpdatePanel();
        }

        void ChangeSelectAttendeeStatus(AttendeeStatus status)
        {
            if (!Object.HasStateAuthority || selectedRequestingAttendee == null)
            {
                Debug.LogError("Unable to change selected requesting attendee status");
                return;
            }
            // We are not necessarily the Attendee state authority (in fact, as the presetner, we most probably did not request the voice ...)
            //  so we cannot directly change its [Networked] var AttendeeStatus. 
            // Our local SelectedRequestingAttendeeInfo OnChanged callback will broadcast this change request and find the actual state authority of the Attendee,
            //  and this player will eventually be able to change the AttendeeStatus.
            SelectedRequestingAttendeeInfo = new AttendeeInfo { attendeeStatus = status, attendeePlayer = selectedRequestingAttendee.AttendeePlayer };
        }
    }
}

