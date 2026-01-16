using Fusion;
using Fusion.Samples.Stage;
using Fusion.XR.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct ScreenShareInfo : INetworkStruct
{
    public ScreenShareStatus screenShareStatus;
    public PlayerRef screenSharePlayer;
}


/***
 * 
 * EmissionOrchestratorWithAuthorizationManagement manages the screen sharing orchestration :
 *  
 *  - it maintains a list of screen sharing requester thanks to the IScreenShareRegistryListener interface
 *  - update the stage console UI to inform the presenter that a screen sharing is requested / canceled / stopped
 *  - update the networked var SelectedRequestingScreenShareInfo to trigger screeen sharing emiter.
 *  
 ***/
public class EmissionOrchestratorWithAuthorizationManagement : NetworkBehaviour, IScreenShareRegistryListener
{
    public List<IScreenShare> requestingScreenSharing = new List<IScreenShare>();
    [Networked]
    public ScreenShareInfo SelectedRequestingScreenShareInfo { get; set; }
    public IScreenShare selectedRequestingScreenShare;
    public GameObject requestPanel;
    public TMPro.TextMeshProUGUI username;
    public TMPro.TextMeshProUGUI headline;
    public TMPro.TextMeshProUGUI okButtonText;
    public ScreenShareRegistry screenShareRegistry;
    public GameObject screenShareRejectButton;

    ChangeDetector changeDetector;


    void Awake()
    {
        screenShareRegistry.RegisterListener(this);
        UpdatePanel();
    }

    public override void Spawned()
    {
        base.Spawned();
        changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
    }

    public override void Render()
    {
        base.Render();

        foreach (var changedVar in changeDetector.DetectChanges(this))
        {
            if (changedVar == nameof(SelectedRequestingScreenShareInfo))
            {
                DidChangeSelectedRequestingScreenSharing();
            }
        }
    }


    public void DidChangeSelectedRequestingScreenSharing()
    {
        if (SelectedRequestingScreenShareInfo.screenSharePlayer == PlayerRef.None)
        {
            selectedRequestingScreenShare = null;
        }
        else
        {
            foreach (var screenshare in screenShareRegistry.screenShares)
            {
                if (screenshare.ScreenSharePlayer == SelectedRequestingScreenShareInfo.screenSharePlayer)
                {
                    selectedRequestingScreenShare = screenshare;
                    if (selectedRequestingScreenShare.ScreenSharePlayer == Runner.LocalPlayer)
                    {
                        // We can only change the [Networked] var ScreenShareStatus on the state authority of its NetworkObject,
                        //  so while we are broadcasting the change, we find the player that can actually change the ScreenShareStatus
                        selectedRequestingScreenShare.ChangeScreenShareStatus(SelectedRequestingScreenShareInfo.screenShareStatus);
                    }
                    break;
                }
            }
        }

        UpdatePanel();
    }

    #region IScreenShareRegistryListener

    public async void OnScreenShareUpdate(ScreenShareRegistry registry, IScreenShare screenShare)
    {
        bool isStatusDisplayedOnUI = screenShare.ScreenShareStatus == ScreenShareStatus.ScreenShareRequested || screenShare.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress;
        if (isStatusDisplayedOnUI)
        {
            if (requestingScreenSharing.Contains(screenShare) == false)
            {
                requestingScreenSharing.Add(screenShare);
                await UpdateSelectedScreenShare();
            }
        }
        else if (requestingScreenSharing.Contains(screenShare))
        {
            requestingScreenSharing.Remove(screenShare);
            await UpdateSelectedScreenShare();
        }
        UpdatePanel();
    }

    public async void OnScreenShareUnregister(ScreenShareRegistry registry, IScreenShare screenShare)
    {
        if (requestingScreenSharing.Contains(screenShare)) requestingScreenSharing.Remove(screenShare);
        await UpdateSelectedScreenShare();
        UpdatePanel();
    }


    async Task UpdateSelectedScreenShare()
    {
        IScreenShare screenshare = null;
        if (requestingScreenSharing.Count != 0)
        {
            screenshare = requestingScreenSharing[0];
        }
        selectedRequestingScreenShare = screenshare;

        await Object.EnsureHasStateAuthority();
        if (!Object.HasStateAuthority) return;
        if (selectedRequestingScreenShare == null)
        {
            SelectedRequestingScreenShareInfo = new ScreenShareInfo { screenSharePlayer = PlayerRef.None };
        }
        else
        {
            SelectedRequestingScreenShareInfo = new ScreenShareInfo { screenShareStatus = screenshare.ScreenShareStatus, screenSharePlayer = screenshare.ScreenSharePlayer };

        }
    }

    void UpdatePanel()
    {
        headline.text = $"Screen Sharing Requests : {requestingScreenSharing.Count}";
        Debug.Log(" UpdatePanel : nb of requestingScreenSharing: " + requestingScreenSharing.Count);
        if (requestingScreenSharing.Count == 0 || selectedRequestingScreenShare == null)
        {
            requestPanel.SetActive(false);
            return;
        }
        requestPanel.SetActive(true);
        if (selectedRequestingScreenShare.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
        {
            Debug.Log("");
            okButtonText.text = "Stop";
            screenShareRejectButton.SetActive(false);
        }
        else
        {
            okButtonText.text = "OK";
            screenShareRejectButton.SetActive(true);
        }
        username.text = selectedRequestingScreenShare.ScreenShareName;
    }

    [ContextMenu("OKStop Button")]
    public async void OnOkStopButton()
    {
        if (!Object.HasStateAuthority && !await Object.WaitForStateAuthority()) return;
        await UpdateSelectedScreenShare();// Not really needed

        if (selectedRequestingScreenShare.ScreenShareStatus == ScreenShareStatus.ScreenShareRequested)
        {
            ChangeSelectScreenShareStatus(ScreenShareStatus.ScreenShareInProgress);
        }
        else if (selectedRequestingScreenShare.ScreenShareStatus == ScreenShareStatus.ScreenShareInProgress)
        {
            // Disable the screensharing(but keep their request)
            ChangeSelectScreenShareStatus(ScreenShareStatus.ScreenShareStopped);
        }
        UpdatePanel();
    }


    public async void OnRejected()
    {
        if (!Object.HasStateAuthority && !await Object.WaitForStateAuthority()) return;
        await UpdateSelectedScreenShare();// Not really needed

        ChangeSelectScreenShareStatus(ScreenShareStatus.ScreenShareRejected);
        requestingScreenSharing.Remove(selectedRequestingScreenShare);
        UpdatePanel();
    }

    void ChangeSelectScreenShareStatus(ScreenShareStatus status)
    {
        if (!Object.HasStateAuthority || selectedRequestingScreenShare == null)
        {
            Debug.LogError("Unable to change selected requesting screenshare status");
            return;
        }
        // We are not necessarily the Attendee state authority (in fact, as the presetner, we most probably did not request the voice ...)
        //  so we cannot directly change its [Networked] var AttendeeStatus. 
        // Our local SelectedRequestingAttendeeInfo OnChanged callback will broadcast this change request and find the actual state authority of the Attendee,
        //  and this player will eventually be able to change the AttendeeStatus.
        SelectedRequestingScreenShareInfo = new ScreenShareInfo { screenShareStatus = status, screenSharePlayer = selectedRequestingScreenShare.ScreenSharePlayer };
    }


    #endregion
}
