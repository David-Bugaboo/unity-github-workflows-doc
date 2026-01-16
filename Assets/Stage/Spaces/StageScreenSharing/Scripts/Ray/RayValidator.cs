using Fusion.Addons.AudioChatBubble;
using Fusion.Addons.AudioRoomAddon;
using Fusion.Samples.IndustriesComponents;
using Fusion.XR.Shared.Locomotion;
using Fusion.XR.Shared.Rig;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Display an error message when the user can't teleport due to the stage being already occupied
     */
    public class RayValidator : MonoBehaviour, IRayValidator
    {
        public ChatBubble stage;
        public List<Collider> stageColliders = new List<Collider>();
        public Canvas validationLimitCanvas;
        public TextMeshProUGUI validationLimitText;
        public bool textShouldFollowPointerOrigin = true;


        [Header("Automatically set")]
        [SerializeField] private Managers managers;
        [SerializeField] private NetworkRunner runner;
        [SerializeField] private RigInfo rigInfo;
        [SerializeField] private HardwareRig hardwareRig;
        [SerializeField] private NetworkRig networkRig;
        [SerializeField] private AudioRoomMember audioRoomMember;

        bool beamRestrictionInPlace = false;
        float lastTextDisplayTime = 0f;
        private void Awake()
        {
            managers = Managers.FindInstance();
            if (managers == null)
                Debug.LogError("Managers not found !");
            runner = managers.runner;
            if (runner == null)
                Debug.LogError("Runner not found !");
            else
                rigInfo = RigInfo.FindRigInfo(runner);

            if (rigInfo == null)
                Debug.LogError("RigInfo not found !");

                
            foreach (var beam in GetComponentsInChildren<RayBeamer>())
            {
                beam.rayValidator = this;
            }

            if (stageColliders.Count == 0) stageColliders = new List<Collider>(stage.GetComponentsInChildren<Collider>());

            validationLimitCanvas.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (beamRestrictionInPlace && Time.time > lastTextDisplayTime)
                validationLimitCanvas.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (var beam in GetComponentsInChildren<RayBeamer>())
            {
                if ((Object) beam.rayValidator == this)
                    beam.rayValidator = null;
            }
        }

        #region IRayValidator
        public void ValidateOnBeamerHit(RayBeamer beamer, RaycastHit hit)
        {
            if (hardwareRig == null || networkRig == null)
                SetRigInfo();
        
            bool beamEnabled = true;

            if (stageColliders.Contains(hit.collider))
            {
                // Ray to stage
                if (stage.AcceptMoreMembers == false && stage.members.Contains(audioRoomMember) == false)
                {
                    // Stage occupied by someone else
                    beamer.ray.color = beamer.noHitColor;
                    beamer.status = RayBeamer.Status.BeamNoHit;
                    validationLimitText.text = "Not possible to go on stage as presenter limit reached";
                    beamRestrictionInPlace = true;
                    beamEnabled = false;
                    lastTextDisplayTime = Time.time + 0.5f;
                    validationLimitCanvas.gameObject.SetActive(true);
                    if (textShouldFollowPointerOrigin)
                    {
                        validationLimitCanvas.transform.position = beamer.ray.origin + hardwareRig.transform.up * 0.2f;
                        validationLimitCanvas.transform.LookAt(hardwareRig.headset.transform.position + 10 * (validationLimitCanvas.transform.position - hardwareRig.headset.transform.position));
                    }
                }
            }
            if (beamEnabled)
            {
                if (beamRestrictionInPlace)
                    validationLimitCanvas.gameObject.SetActive(false);
            }
        }
        #endregion



        private void SetRigInfo()
        {
            if (hardwareRig == null)
            {
                hardwareRig = rigInfo.localHardwareRig;
                if (hardwareRig == null)
                    Debug.LogError("hardwareRig not found");
            }

            if (networkRig == null)
            {
                networkRig = rigInfo.localNetworkedRig;
                if (networkRig == null)
                    Debug.LogError("NetworkRig not found");
                else
                {
                    audioRoomMember = networkRig.GetComponent<AudioRoomMember>();
                    if (audioRoomMember == null)
                        Debug.LogError("AudioRoomMember not found");
                }
            }
        }
    }
}
