using Fusion.XR.Shared.Rig;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Samples.Stage
{
    /**
     * Button to request voice while seated
     */
    public class RequestVoice : MonoBehaviour
    {
        RigInfo rigInfo;
        VoiceableAttendee voiceableAttendee;
        StageHardwareRig hardwareRig;
        [SerializeField] private AttendeeRequestHandler attendeeRequestHandler;
        [SerializeField] private TMPro.TextMeshProUGUI numberOfAttendeesWaiting;
        [SerializeField] private TMPro.TextMeshProUGUI buttonText;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite spriteAsk;
        [SerializeField] private Sprite spriteCancel;
        [SerializeField] private Sprite spriteQuit;
        [SerializeField] private TMPro.TextMeshProUGUI statusText;
        [SerializeField] private GameObject visibleButton;
        private const string yourStatus = "Seu status : ";
        private bool inTalk = false;

        private void Awake()
        {
            hardwareRig = GetComponentInParent<StageHardwareRig>();
            hardwareRig.onSeat.AddListener(OnDidSeat);
            hardwareRig.onUnseat.AddListener(OnDidUnseat);
            if (rigInfo == null) rigInfo = RigInfo.FindRigInfo(allowSceneSearch: true);
            UpdatePanel();
            visibleButton.SetActive(false);
        }

        void OnDidSeat(Seat seat)
        {
            visibleButton.SetActive(true);
            var position = seat.transform.position + seat.transform.forward * 0.5f;
            position.y = hardwareRig.headset.transform.position.y - 0.3f;
            transform.position = position;
            transform.rotation = seat.transform.rotation;
            UpdateUI();
        }

        void OnDidUnseat()
        {
            visibleButton.SetActive(false);
            // Cancel any pending request
            voiceableAttendee.AttendeeStatus = AttendeeStatus.Spectator;
        }

        void PrepareNetworkRig()
        {
            if (voiceableAttendee == null && rigInfo != null && rigInfo.localNetworkedRig != null) voiceableAttendee = rigInfo.localNetworkedRig.GetComponent<VoiceableAttendee>();
        }

        public void OnRequestVoice()
        {
            PrepareNetworkRig();
            if (voiceableAttendee == null) return;

            if (voiceableAttendee.AttendeeStatus == AttendeeStatus.VoiceRequestingSpectator || voiceableAttendee.AttendeeStatus == AttendeeStatus.MutedSpectator || voiceableAttendee.AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                // Cancel the request
                voiceableAttendee.AttendeeStatus = AttendeeStatus.Spectator;
            }
            else
            {
                // Request the voice access
                voiceableAttendee.AttendeeStatus = AttendeeStatus.VoiceRequestingSpectator;
            }
            UpdateUI();
        }

        void UpdateUI()
        {
            PrepareNetworkRig();
            if (voiceableAttendee == null) return;

            if (voiceableAttendee != null && voiceableAttendee.AttendeeStatus == AttendeeStatus.VoiceRequestingSpectator)
            {
                statusText.text = yourStatus + "aguardando...";
                buttonText.text = "Cancelar pedido";
                buttonImage.sprite = spriteCancel;
            }
            else if (voiceableAttendee.AttendeeStatus == AttendeeStatus.RejectedVoiceSpectator)
            {
                if (inTalk)
                    statusText.text = yourStatus + "fala interrompida";
                else
                    statusText.text = yourStatus + "pedido recusado";
                buttonText.text = "Pedir para falar";
                buttonImage.sprite = spriteAsk;
            }
            else if (voiceableAttendee.AttendeeStatus == AttendeeStatus.VoicedSpectator)
            {
                statusText.text = yourStatus + "Todos estão te ouvindo";
                buttonText.text = "Parar";
                buttonImage.sprite = spriteQuit;
                inTalk = true;
            }
            else if (voiceableAttendee.AttendeeStatus == AttendeeStatus.MutedSpectator)
            {
                statusText.text = yourStatus + "focê foi mutado";
                buttonText.text = "Parar";
                buttonImage.sprite = spriteQuit;
            }
            else
            {
                statusText.text = yourStatus + "sem pedido";
                buttonText.text = "Pedir para falar";
                buttonImage.sprite = spriteAsk;
                inTalk = false;
            }
        }

        private void Update()
        {
            if (visibleButton.activeSelf)
            {
                UpdateUI();
                UpdatePanel();
            }
        }

        private void UpdatePanel()
        {
            if (attendeeRequestHandler) numberOfAttendeesWaiting.text = "People waiting to talk : " + attendeeRequestHandler.requestingAttendees.Count.ToString();

        }
    }
}
