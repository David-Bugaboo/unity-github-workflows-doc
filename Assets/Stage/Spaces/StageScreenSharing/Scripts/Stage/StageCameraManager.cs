using Fusion.Addons.AudioRoomAddon;
using Fusion.XR.Shared.Rig;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Detect when an user enter the stage.
     * Ensure that their avatar has their higher LOD activated.
     * Select the closest StageCamera and enable it
     */
    public class StageCameraManager : MonoBehaviour, IAudioRoomListener
    {
        public List<StageCamera> stageCameras = new List<StageCamera>();
        [SerializeField] private GameObject defaultCameraTarget;
        [SerializeField] private GameObject cameraTarget;
        public List<AudioRoomMember> memberList = new List<AudioRoomMember>();


        private AudioRoomMember speaker;
         
        private void Update()
        {
            
            if (speaker)
            {
                var speakerRig = speaker.GetComponentInParent<NetworkRig>();
                cameraTarget = speakerRig.headset.networkTransform.gameObject;
            }
            else
            {
                cameraTarget = defaultCameraTarget;
            }

            float minDistance = float.PositiveInfinity;
            StageCamera closestDistanceCamera = null;
            foreach (var cam in stageCameras)
            {
                cam.Track(cameraTarget);
                var dist = (cameraTarget.transform.position - cam.transform.position).sqrMagnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestDistanceCamera = cam;
                }
            }
            foreach(var cam in stageCameras)
            {
                cam.Record(cam == closestDistanceCamera);
            }
            
        }

        
        void ForceUserRepresentation(AudioRoomMember audioRoomMember, bool ignoreDistance)
        {
            var publicSpeechHandler = audioRoomMember.GetComponent<PublicSpeechHandler>();
            if (!publicSpeechHandler) return;

            publicSpeechHandler.SetIsOnStage(ignoreDistance);
        }


        #region IAudioRoomListener
        public void OnIsInRoom(IAudioRoomMember member, IAudioRoom room)
        {
            AudioRoomMember audioRoomMember = member as AudioRoomMember;

            if (room != null)
            {
                // player enter the stage
                ForceUserRepresentation(audioRoomMember, ignoreDistance: true);
                speaker = audioRoomMember;
                
                if(memberList.Contains(audioRoomMember) == false) 
                {
                    memberList.Add(audioRoomMember);
                }
            }
            else
            {
                // player exit the stage
                ForceUserRepresentation(audioRoomMember, ignoreDistance: false);

                if (memberList.Contains(audioRoomMember))
                {
                    memberList.Remove(audioRoomMember);
                    if (memberList.Count > 0)
                    {
                        speaker = memberList[Random.Range(0, memberList.Count)];   
                    }
                    else
                        speaker = null;
                }
            }
        }
        #endregion


    }
}

