using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Control a camera focusing on the presenter on stage. 
     * Follow an optional rig transform (if not chosen, a rig is created with the same orientation) and move on the x axis of the camera rig
     * Handle traveling, orinetation, and zoom (FOV)
     * Also limit the frame rate of the camera capture (does not need to be as high as the actual refresh rate of the app)
     */
    [RequireComponent(typeof(Camera))]
    public class StageCamera : MonoBehaviour
    {

        public int cameraFps = 24;
        float delayBetweenRecord;
        float nextRecord = -1;
        [Header("Camera speaker tracking config")]
        [SerializeField] private float followSpeed = 0.4f;
        [SerializeField] private float gazingRotationSpeed = 10;
        [SerializeField] private float camTravelingCourse = 3;

        public Transform cameraRigCenter;
        public bool parentCameraOnRig = true;


        Camera stageCamera;

        GameObject cameraTarget;
        bool isRecording = false;

        private void Awake()
        {
            stageCamera = GetComponent<Camera>();
            delayBetweenRecord = 1f / (float)cameraFps;
            stageCamera.enabled = false;
            if(cameraRigCenter == null)
            {
                var rig = new GameObject($"{name} - Rig");
                rig.transform.position = transform.position;
                rig.transform.rotation = transform.rotation;
                if (transform.parent) rig.transform.parent = transform.parent;
                cameraRigCenter = rig.transform;
            }
            if (parentCameraOnRig)
            {
                transform.parent = cameraRigCenter;
            }
        }

        public void Track(GameObject target)
        {
            cameraTarget = target;
        }

        public void Record(bool record)
        {
            isRecording = record;
        }

        private void Update()
        {
            if (cameraTarget)
            {
                Tracking();
            }

            if (isRecording)
            {
                if(Time.time > nextRecord)
                {
                    stageCamera.Render();
                    nextRecord = Time.time + delayBetweenRecord;
                }
            }
        }

        public void Tracking(){
            // move camera
            LookInDirection();

            Travelling();

            // update camera FOV
            AdaptFOV();
        }

        public void LookInDirection()
        {
            var target = cameraTarget.transform;
            var gazeDirection = target.position - stageCamera.transform.position;
            Quaternion gazeRotation = Quaternion.LookRotation(gazeDirection);
            stageCamera.transform.rotation = Quaternion.Slerp(stageCamera.transform.rotation, gazeRotation, Time.deltaTime * gazingRotationSpeed);
        }

        void Travelling()
        {
            //TODO Add minimal distance before travelling

            var newCameraPositionInRigTransform = cameraRigCenter.InverseTransformPoint(stageCamera.transform.position);
            var targetPositionInRigTransform = cameraRigCenter.InverseTransformPoint(cameraTarget.transform.position);

            var targetAxisValue = targetPositionInRigTransform.x;
            var currentAxisValue = newCameraPositionInRigTransform.x;

            float newAxisValue = Mathf.Lerp(currentAxisValue, targetAxisValue, followSpeed * Time.deltaTime);
            newAxisValue = Mathf.Clamp(newAxisValue, -camTravelingCourse, camTravelingCourse);

            newCameraPositionInRigTransform.x = newAxisValue;

            stageCamera.transform.position = cameraRigCenter.TransformPoint(newCameraPositionInRigTransform);
        }

        void AdaptFOV()
        {
            var distanceBetweenSpeakerAndCamera = Vector3.Distance(stageCamera.transform.position, cameraTarget.transform.position);
            stageCamera.fieldOfView = (distanceBetweenSpeakerAndCamera * -0.3f) + 13f;
        }
    }

}
