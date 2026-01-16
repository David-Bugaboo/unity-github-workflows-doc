using Fusion.XR.Shared.Desktop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.Samples.Stage
{
    /**
     *
     * ZoomManager is in charge to manage zoom in & zoom out when the user clicks on the video screen
     * During the animation the user can not teleport
     **/

    public class ZoomManager : MonoBehaviour
    {
        [SerializeField] private DesktopController desktopController;
        private MouseCamera mouseCamera;
        private MouseTeleport mouseTeleport;
        private GameObject desktopRigCameraParent;
        [SerializeField] private GameObject desktopRigCameraGO;
        [SerializeField] private GameObject zoomCamera;
        [SerializeField] private GameObject screen;
        [SerializeField] private float cameraMoveSpeed = 4f;
        [SerializeField] private float gazingRotationSpeed = 4f;
        [SerializeField] private float animationDuration = 2f;

        private Camera desktopRigCamera;
        private Vector3 initialZoomCameraPosition;
        private Vector3 targetAnimationPosition;
        private Vector3 targetAnimationDirection;

        private Quaternion desktopRigCameraRotation;

        private bool moveCamera = false;
        private bool zoomEnable = false;

        private float animationStartTime = -1f;
        private float lastZoomOutTime;
        private float coolDown = 0.1f;

        private void Awake()
        {
            initialZoomCameraPosition = zoomCamera.transform.position;
            zoomCamera.SetActive(false);
            desktopRigCameraParent = desktopRigCameraGO.transform.parent.gameObject;
            desktopRigCamera = desktopRigCameraGO.GetComponent<Camera>();
            mouseCamera = desktopController.GetComponent<MouseCamera>();
            mouseTeleport = desktopController.GetComponent<MouseTeleport>();
        }

        public void OnScreenClick()
        {
            if (!IsScreenZoomEnabled()) return;

            if (zoomEnable)
                ZoomOut();
            else
                ZoomIn();
        }

        bool IsScreenZoomEnabled()
        {
            // Only available in desktop mode
            return desktopController != null && desktopController.gameObject.activeSelf;
        }

        void DisableDesktopLocomotion()
        {
            if (!IsScreenZoomEnabled()) return;
            desktopController.enabled = false;
            mouseCamera.enabled = false;
            mouseTeleport.enabled = false;
            mouseTeleport.rayBeamer.isRayEnabled = false;
        }

        void EnableDesktopLocomotion()
        {
            if (!IsScreenZoomEnabled()) return;
            desktopController.enabled = true;
            mouseCamera.enabled = true;
            mouseTeleport.enabled = true;
            // If the fader was active when clicking on the screen (hence switching camera), we reset the fade
            desktopController.rig.headset.fader.SetFade(0);
        }

        private void Update()
        {
            if (!IsScreenZoomEnabled()) return;

            // Check is camera is moving
            if (moveCamera)
            {
                // Camera animation
                MoveCamera();

                // Check if animation must be stopped 
                if (animationStartTime + animationDuration < Time.time)
                {
                    // Camera animation must be stopped
                    moveCamera = false;
                    animationStartTime = -1;

                    // Replace the zoom camera by the desktop rig camera if camera is in zoom out position
                    if (!zoomEnable)
                    {
                        zoomCamera.SetActive(false);
                        desktopRigCamera.enabled = true;
                        // Enable the locomotion
                        EnableDesktopLocomotion();
                    }
                }
            }
            else
            {
                // zoom out if we are in zoom in position and the user click on the mouse
                if (zoomEnable && Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    ZoomOut();
                }

            }
        }

        private void ZoomIn()
        {
            // anti bounce is required to avoid conflict between mouse click detection & click on the screen to zoom out
            if (lastZoomOutTime + coolDown < Time.time)
            {
                // backup desktop rig camera 
                desktopRigCameraRotation = desktopRigCameraGO.transform.rotation;

                // move zoom camera to desktopRig camera position
                zoomCamera.transform.SetParent(desktopRigCameraParent.transform);
                zoomCamera.transform.localPosition = Vector3.zero;
                zoomCamera.transform.rotation = desktopRigCameraRotation;

                // replace desktop rig camera by the zoom camera
                desktopRigCamera.enabled = false;
                zoomCamera.SetActive(true);

                // disable locomotion during animation
                DisableDesktopLocomotion();

                // setup & start camera animation
                targetAnimationPosition = initialZoomCameraPosition;
                targetAnimationDirection = screen.transform.position;
                moveCamera = true;
                animationStartTime = Time.time;

                // zoom in position
                zoomEnable = !zoomEnable;
            }
        }

        private void ZoomOut()
        {
            // restore cameras settings
            zoomCamera.transform.SetParent(this.gameObject.transform);
            desktopRigCameraGO.transform.rotation = desktopRigCameraRotation;

            // disable locomotion during animation
            DisableDesktopLocomotion();

            // setup & start camera animation
            targetAnimationPosition = desktopRigCameraGO.transform.position;
            moveCamera = true;
            animationStartTime = Time.time;

            // zoom out position
            zoomEnable = !zoomEnable;

            // set anti-bounce timer
            lastZoomOutTime = Time.time;
        }



        private void MoveCamera()
        {
            zoomCamera.transform.position = Vector3.Lerp(zoomCamera.transform.position, targetAnimationPosition, cameraMoveSpeed * Time.deltaTime);
            if (zoomEnable)
                LookInDirection(targetAnimationDirection);
            else
                LookInDirection(desktopRigCameraRotation);
        }

        public void LookInDirection(Vector3 targetDirection)
        {
            var gazeDirection = targetDirection - zoomCamera.transform.position;
            Quaternion gazeRotation = Quaternion.LookRotation(gazeDirection);
            zoomCamera.transform.rotation = Quaternion.Slerp(zoomCamera.transform.rotation, gazeRotation, Time.deltaTime * gazingRotationSpeed);
        }

        public void LookInDirection(Quaternion targetRotation)
        {
            zoomCamera.transform.rotation = Quaternion.Slerp(zoomCamera.transform.rotation, targetRotation, Time.deltaTime * gazingRotationSpeed);
        }
    }
}
