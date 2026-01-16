using Fusion.Addons.ScreenSharing;
using Fusion.Samples.Stage;
using Fusion.XR.Shared.Rig;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;

public class SimpleEmitterMenu : MonoBehaviour
{
    [SerializeField] Button selectDesktop0Button;
    [SerializeField] Button selectDesktop1Button;
    [SerializeField] TMPro.TextMeshProUGUI selectDesktop0Label;
    [SerializeField] TMPro.TextMeshProUGUI selectDesktop1Label;

    [SerializeField] private GameObject desktop0RecordPanel;
    [SerializeField] private GameObject desktop0PreviewImage;
    [SerializeField] private GameObject desktop1RecordPanel;
    [SerializeField] private GameObject desktop1PreviewImage;

    [SerializeField] ScreenSharingEmitter screenSharingEmitter;
    [SerializeField] EmissionOrchestratorWithAuthorizationManagement emissionOrchestrator;
    [SerializeField] TMPro.TextMeshProUGUI statusLabel;
    [SerializeField] TMPro.TMP_InputField nameField;

    bool isScreenSharingUIDisplayed = false;

    RigInfo rigInfo;
    ScreenShareAttendee screenShareAttendee;
    private int currentDesktopIndex = -1;
    private int screenNumber;

    const string STOP_SCREENSHARING_TEXT = "Stop screen Sharing";
    const string START_SCREENSHARING_TEXT = "Share this screen";
    const string CANCEL_SCREENSHARING_TEXT = "Cancel request";

    private void Awake()
    {

        if (rigInfo == null) rigInfo = RigInfo.FindRigInfo(allowSceneSearch: true);

        if (screenSharingEmitter == null) screenSharingEmitter = FindObjectOfType<ScreenSharingEmitter>(true);
        if (emissionOrchestrator == null) emissionOrchestrator = FindObjectOfType<EmissionOrchestratorWithAuthorizationManagement>(true);
        ConfigureButtons();
        desktop0RecordPanel.SetActive(false);
        desktop1RecordPanel.SetActive(false);
    }


    void ConfigureButtons()
    {
        if (selectDesktop0Button == null || selectDesktop1Button == null)
        {
            foreach (var uwcImage in GetComponentsInChildren<UwcImage>(true))
            {
                if (uwcImage.desktopIndex == 0 && selectDesktop0Button == null)
                {
                    selectDesktop0Button = uwcImage.GetComponentInParent<Button>();
                }
                if (uwcImage.desktopIndex == 1 && selectDesktop1Button == null)
                {
                    selectDesktop1Button = uwcImage.GetComponentInParent<Button>();
                }
            }
        }
        if (selectDesktop0Button) selectDesktop0Button.onClick.AddListener(() => { ToggleDesktopConnection(0); });
        if (selectDesktop1Button) selectDesktop1Button.onClick.AddListener(() => { ToggleDesktopConnection(1); });
    }


    void ToggleDesktopConnection(int desktopIndex)
    {

        UnityEngine.Debug.LogError("OnRequestScreenShare : " + desktopIndex);
        FindScreenShareAttendee();
        if (screenShareAttendee == null)
        {
            UnityEngine.Debug.LogError("screenShareAttendee not found");
            return;
        }

        if (desktopIndex == currentDesktopIndex)
        {
            if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
            {
                // Stop the screen sharing
                screenShareAttendee.ScreenShareStatus = ScreenShareStatus.ScreenShareStopped;
            }
            else
            {
                OnSelectDesktop(desktopIndex);
                screenShareAttendee.ScreenShareStatus = ScreenShareStatus.ScreenShareInProgress;
            }
        }
        else
        {
            if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
            {
                OnSelectDesktop(desktopIndex);
            }
            else
            {
                OnSelectDesktop(desktopIndex);
                screenShareAttendee.ScreenShareStatus = ScreenShareStatus.ScreenShareInProgress;
            }

        }
        currentDesktopIndex = desktopIndex;
        screenNumber = desktopIndex + 1;
        UpdateStatus();
    }

    public void OnSelectDesktop(int desktopID)
    {
        Debug.Log($"Desktop {desktopID} selected");
        if (screenSharingEmitter) screenSharingEmitter.SelectDesktop(desktopID);
    }

    void EnableScreenSharingUI(int index)
    {
        isScreenSharingUIDisplayed = true;
        if (index == 0)
        {
            ChangeScreenUIVisibility(0, true);
            ChangeScreenUIVisibility(1, false);
            if (screenShareAttendee != null)
            {
                if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareRequested)
                {
                    selectDesktop0Label.text = CANCEL_SCREENSHARING_TEXT;
                }
                else if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
                {
                    selectDesktop0Label.text = STOP_SCREENSHARING_TEXT;
                    desktop0RecordPanel.SetActive(true);
                }
            }
        }
        else if (index == 1)
        {
            ChangeScreenUIVisibility(1, true);
            ChangeScreenUIVisibility(0, false);
            if (screenShareAttendee != null)
            {
                if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareRequested)
                    selectDesktop1Label.text = CANCEL_SCREENSHARING_TEXT;
                else if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
                {
                    selectDesktop1Label.text = STOP_SCREENSHARING_TEXT;
                    desktop1RecordPanel.SetActive(true);
                }
                    
            }
        }
    }

    void DisableScreenSharingUI()
    {
        if (isScreenSharingUIDisplayed)
        {
            isScreenSharingUIDisplayed = false;
            bool hasSecondScreen = UwcManager.desktopCount > 1;
            ChangeScreenUIVisibility(0, true);
            ChangeScreenUIVisibility(1, hasSecondScreen);
            selectDesktop0Label.text = START_SCREENSHARING_TEXT;
            selectDesktop1Label.text = START_SCREENSHARING_TEXT;
            desktop0RecordPanel.SetActive(false);
            desktop1RecordPanel.SetActive(false);
            if (screenSharingEmitter.status != ScreenSharingEmitter.Status.NotEmitting)
            {
                screenSharingEmitter.DisconnectScreenSharing();
            }
        }
    }


    void FindScreenShareAttendee()
    {
        if (screenShareAttendee == null && rigInfo != null && rigInfo.localNetworkedRig != null)
            screenShareAttendee = rigInfo.localNetworkedRig.GetComponent<ScreenShareAttendee>();
    }

    private void Update()
    {
        CheckScreenCount();
        UpdateStatus();

        if (isScreenSharingUIDisplayed && screenSharingEmitter.status == ScreenSharingEmitter.Status.NotEmitting && screenShareAttendee.ScreenShareStatus != ScreenShareStatus.ScreenShareRequested && screenShareAttendee.ScreenShareStatus != ScreenShareStatus.ScreenShareInProgress)
            DisableScreenSharingUI();

    }

    void UpdateStatus()
    {
        FindScreenShareAttendee();
        if (screenShareAttendee == null) return;


#if UWC_EMITTER_ENABLED
        if (screenShareAttendee != null)
        {
            if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareRequested)
            {
                statusLabel.text = "Waiting for authorization...";

                if (currentDesktopIndex == 0)
                    EnableScreenSharingUI(0);
                else
                    EnableScreenSharingUI(1);
            }
            else if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareRejected)
            {
                statusLabel.text = "Screen share rejected by the presenter";
                DisableScreenSharingUI();
            }
            else if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
            {
                statusLabel.text = "Everybody can see your screen " + screenNumber;

                if (currentDesktopIndex == 0)
                    EnableScreenSharingUI(0);
                else
                    EnableScreenSharingUI(1);
            }
            else if (screenShareAttendee.ScreenShareStatus == ScreenShareStatus.ScreenShareStopped)
            {
                statusLabel.text = "Screen share stopped by the presenter";
                DisableScreenSharingUI();
            }
            else
            {
                statusLabel.text = "Connected. Click on a preview to request screen sharing.";
            }
        }
#endif
    }

    void CheckScreenCount()
    {

#if UWC_EMITTER_ENABLED
        bool hasSecondScreen = UwcManager.desktopCount > 1;
        bool secondScreenVisible = hasSecondScreen;
        if (hasSecondScreen && screenSharingEmitter.status != ScreenSharingEmitter.Status.NotEmitting)
        {
            secondScreenVisible = screenSharingEmitter.DesktopIndex == 1;
        }
        ChangeScreenUIVisibility(1, secondScreenVisible);
        if (hasSecondScreen == false && screenSharingEmitter.status != ScreenSharingEmitter.Status.NotEmitting && screenSharingEmitter.DesktopIndex == 1)
        {
            // Second screen has been lost will we were sharing it
            screenSharingEmitter.DisconnectScreenSharing();
        }
#endif
    }

    void ChangeScreenUIVisibility(int index, bool visible)
    {
        if (index == 0)
        {
            selectDesktop0Button.gameObject.SetActive(visible);
            if (selectDesktop0Label) selectDesktop0Label.gameObject.SetActive(visible);
        }
        if (index == 1)
        {
            selectDesktop1Button.gameObject.SetActive(visible);
            if (selectDesktop1Label) selectDesktop1Label.gameObject.SetActive(visible);
        }
    }

}
