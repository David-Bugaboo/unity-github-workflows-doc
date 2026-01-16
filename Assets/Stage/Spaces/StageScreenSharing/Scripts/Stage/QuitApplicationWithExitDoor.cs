using Fusion.Samples.IndustriesComponents;
using Fusion.XR.Shared.Locomotion;
using Fusion.XR.Shared.Rig;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    public class QuitApplicationWithExitDoor : MonoBehaviour
    {
        private Managers managers;
        private NetworkRunner runner;
        private ApplicationManager applicationManager;
        private Fader desktopCameraFader;
        private Fader hardwareRigFader;
        private RigLocomotion localRigLocomotion;
        private RigInfo rigInfo;

        private bool exitInProgress = false;
        bool waitForFadeIn = false;
        private void Start()
        {
            if (managers == null) managers = Managers.FindInstance();

            if (runner == null)
                runner = managers.runner;
            if (runner == null)
                Debug.LogError("Runner not found !");
            else
            {
                rigInfo = RigInfo.FindRigInfo(runner);
                if (rigInfo != null)
                {
                    localRigLocomotion = rigInfo.localHardwareRig.GetComponent<RigLocomotion>();
                }
            }

            if (applicationManager == null)
                applicationManager = runner.gameObject.GetComponentInChildren<ApplicationManager>();
            if (applicationManager == null)
                Debug.LogError("Application Manager not found !");
            else
            {
                if (rigInfo.localHardwareRigKind == RigInfo.RigKind.Desktop)
                {
                    desktopCameraFader = rigInfo.localHardwareRig.headset.fader;
                }
                if (rigInfo.localHardwareRigKind == RigInfo.RigKind.VR)
                {
                    hardwareRigFader = rigInfo.localHardwareRig.headset.fader;
                }
            }


        }

        [ContextMenu("QuitApplication")]
        private async void QuitApplication()
        {

            // Application manager must be destroyed first to avoid it displaying message and fader issue when the runner is shutdown
            Destroy(applicationManager.gameObject);
            // RigLocomotion must also be destroyed to avoid Fadeout
            Destroy(localRigLocomotion);

            waitForFadeIn = true;
            StartCoroutine(DisplayFaderScreen());
            while (waitForFadeIn) await Task.Delay(10);

            await runner.Shutdown(true);
            applicationManager.QuitApplication();

        }


        private IEnumerator DisplayFaderScreen()
        {
            waitForFadeIn = true;
            if (desktopCameraFader && desktopCameraFader.gameObject.activeInHierarchy)
            {
                yield return Fading(desktopCameraFader);
            }
            if (hardwareRigFader && hardwareRigFader.gameObject.activeInHierarchy)
            {
                yield return Fading(hardwareRigFader);
            }
            waitForFadeIn = false;
        }

        private IEnumerator Fading(Fader fader)
        {
            yield return fader.FadeIn(1);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!exitInProgress && other.GetComponentInParent<HardwareHand>())
            {
                exitInProgress = true;
                Debug.Log($"Quitting the application...");
                QuitApplication();
            }
        }
    }
}
