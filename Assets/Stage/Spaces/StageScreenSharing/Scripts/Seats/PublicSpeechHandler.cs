using Fusion.Addons.Avatar;
using Fusion.Addons.DynamicAudioGroup;
using Fusion.Samples.IndustriesComponents;
using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /***
     * 
     * PublicSpeechHandler is in charge of adapting the user settings if he is on stage or if the speaker allow him to speak when seated :
     *  - the avatar LOD is disabled so all users can see speaking people properly
     *  - the DynamicAudioGroup white list of all members is updated and spatial blend in disabled so that everyone can listen this user
     * 
     ***/

    public class PublicSpeechHandler : MonoBehaviour, IAttendeeRegistryListener
    {
        AvatarRepresentation avatarRepresentation;
        DynamicAudioGroupMember audioGroupMember;
        AudioSource audioSource;
        AttendeeRegistry attendeeRegistry;

        bool onStage = false;
        bool onAttendeeMicrophone = false;

        [SerializeField]
        bool isSpeakingToPublic = false;

        void Awake()
        {
            avatarRepresentation = GetComponent<AvatarRepresentation>();
            audioGroupMember = GetComponent<DynamicAudioGroupMember>();
            if (audioGroupMember == null)
            {
                Debug.LogError("Missing dynamic audio group member");
            }
            var networkHeadset = GetComponentInChildren<NetworkHeadset>();
            if (networkHeadset == null)
            {
                Debug.LogError("Bad configuration");
            }
            audioSource = networkHeadset.GetComponentInChildren<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("Bad configuration");
            }
            attendeeRegistry = FindObjectOfType<AttendeeRegistry>(true);
            attendeeRegistry.RegisterListener(this);
        }

        public void SetIsOnStage(bool isOnStage)
        {
            onStage = isOnStage;
            UpdatePublicSpeakingStatus();
        }
        public void SetIsOnMic(bool isOnAttendeeMic)
        {
            onAttendeeMicrophone = isOnAttendeeMic;
            UpdatePublicSpeakingStatus();
        }

        void UpdatePublicSpeakingStatus()
        {
            isSpeakingToPublic = onAttendeeMicrophone || onStage;
            avatarRepresentation.IgnoreDistance(isSpeakingToPublic);
            ChangeDynamicAudioGroupMembersWhitelist(shouldListenToUs: isSpeakingToPublic);
            audioSource.spatialBlend = isSpeakingToPublic ? 0 : 1;
        }

        void ChangeDynamicAudioGroupMembersWhitelist(bool shouldListenToUs)
        {
            foreach (var member in DynamicAudioGroupMember.AllMembers)
            {
                if (member != audioGroupMember)
                {
                    if (shouldListenToUs && member.alwaysListenedMembers.Contains(audioGroupMember) == false)
                    {
                        member.alwaysListenedMembers.Add(audioGroupMember);
                    }
                    if (shouldListenToUs == false && member.alwaysListenedMembers.Contains(audioGroupMember))
                    {
                        member.alwaysListenedMembers.Remove(audioGroupMember);
                    }
                }
            }
        }

        #region IAttendeeRegistryListener
        public void OnAttendeeUpdate(AttendeeRegistry registry, IAttendee attendee)
        {
        }

        public void OnAttendeeRegister(AttendeeRegistry registry, IAttendee attendee)
        {
            if (isSpeakingToPublic)
            {
                // We want to make sure that we are registered in this new attendee DynamicAudioGroupMember whitelist
                ChangeDynamicAudioGroupMembersWhitelist(isSpeakingToPublic);
            }
        }

        public void OnAttendeeUnregister(AttendeeRegistry registry, IAttendee attendee)
        {
        }
        #endregion
    }
}
