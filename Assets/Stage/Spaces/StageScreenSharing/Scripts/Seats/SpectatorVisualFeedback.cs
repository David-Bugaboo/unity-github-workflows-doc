using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Indicator over the head of an attendee explaining they are requesting (or having received) voice access to the stage
     */
    public class SpectatorVisualFeedback : MonoBehaviour
    {
        [SerializeField] private float verticalOffset = 1f;
        [SerializeField] private float verticalMovement = 1f;
        [SerializeField] private float speedReduction = 15f;

        private NetworkRig networkRig;
        private RigInfo riginfo;
        private NetworkHeadset networkHeadset;

        private void Awake()
        {
            networkHeadset = GetComponentInParent<NetworkHeadset>();
            networkRig = GetComponentInParent<NetworkRig>();
            riginfo = RigInfo.FindRigInfo(networkRig.Object.Runner);
        }


        //public override void Render()
        private void Update()
        {
            if (networkHeadset != null)
            {
                var feedbackLocalPosition = networkRig.transform.InverseTransformPoint(networkHeadset.networkTransform.transform.position);
                feedbackLocalPosition.y += verticalOffset + Mathf.PingPong(Time.time / speedReduction, verticalMovement);   
                var feedbackPosition = networkRig.transform.TransformPoint(feedbackLocalPosition);
                transform.position = feedbackPosition;
                Debug.LogError("NotNull");
            }

            // Look toward local hardware rig        
            var direction = riginfo.localHardwareRig.headset.transform.position - transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                var targetRot = Quaternion.LookRotation(direction);
                targetRot = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);
                transform.rotation = targetRot;
            }
        }
    }
}
