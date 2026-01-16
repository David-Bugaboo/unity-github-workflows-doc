// Uncomment next line if a Photon video SDK version earlier than 2.52 is used
//#define VIDEOSDK_251

#if PHOTON_VOICE_VIDEO_ENABLE

// --- Ativa integração UWC apenas em Windows (Editor/Standalone) quando o define do plugin estiver presente ---
#if U_WINDOW_CAPTURE_RECORDER_ENABLE
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define UWC_EMITTER_ENABLED
#endif
#endif

using UnityEngine;
using UnityEngine.UI;
using Photon.Voice;
using Photon.Voice.Fusion;
using Photon.Voice.Unity;

#if UWC_EMITTER_ENABLED
using uWindowCapture;
#endif

/**
 * ScreenSharingEmitter usa o canal do Photon Voice para transmitir imagens de screen sharing.
 *
 * Fluxo (via UI/EmitterMenu -> ConnectScreenSharing()):
 *  - Aguarda a conexão do Photon Voice,
 *  - Aguarda o uWindowCapture ficar pronto (uWindowCaptureRecorder implementa IVideoRecorderPusher),
 *  - Cria uma "voice" via VoiceClient.CreateLocalVoiceVideo no FusionVoiceClient,
 *  - Se houver IEmitterListener, chama OnStartEmitting/OnStopEmitting nos momentos adequados.
 *
 * Observação: se não houver uWindowCaptureHost na cena, uWindowCaptureRecorder cria um automaticamente
 * e este script configura-o.
 */
public class ScreenSharingEmitter : MonoBehaviour
{
    public interface IEmitterListener
    {
        void OnStartEmitting(ScreenSharingEmitter emitter);
        void OnStopEmitting(ScreenSharingEmitter emitter);
    }
    
    public IEmitterListener listener; 

    [Header("Geral")]
    public bool startSharingOnVoiceConnectionAvailable = false;

#if UWC_EMITTER_ENABLED
    [Header("uWindowCapture (Windows)")]
    [HideInInspector] public uWindowCaptureRecorder screenRecorder;
    [HideInInspector] public uWindowCaptureHost captureHost;
    [Tooltip("GameObject exibido quando offline (pode conter UWC texture para pré-visualização).")]
    public UwcWindowTexture offlinePreview;
#endif

    [Header("Preview")]
    public Image previewImage;
    public Renderer previewRenderer;

#if !VIDEOSDK_251
    [Header("Photon Video")]
    [Tooltip("Canal separado para vídeo (melhor desempenho no transporte Photon).")]
    public int videoChannel = 3;
#endif

    public enum Status
    {
        NotEmitting,
        WaitingVoiceConnection,
        WaitingScreenCaptureAvailability,
        Emitting
    }

    [Header("Status")]
    public Status status = Status.NotEmitting;

    [System.Serializable]
    public struct ScreenSharingSettings
    {
        public Photon.Voice.Codec VideoCodec;
        public bool UseScreenshareResolution;
        public int VideoWidth;
        public int VideoHeight;
        public int VideoBitrate;
        public int AudioBitrate;
        public int VideoFPS;
        public int CaptureFPS;
        public int VideoKeyFrameInt;
        public int videoDelayFrames;
        // Split frames into fragments according to the size provided by the Transport
        public bool fragment;   // (somente VideoSDK >= 2.52)
        public bool reliable;   // enviar confiável
    }

    [SerializeField]
    private ScreenSharingSettings settings = new ScreenSharingSettings
    {
        VideoCodec = Photon.Voice.Codec.VideoVP8,
        UseScreenshareResolution = true,
        VideoWidth = 1920,
        VideoHeight = 1080,
        VideoBitrate = 10_000_000,
        AudioBitrate = 30_000,
        VideoFPS = 3,
        CaptureFPS = 3,
        VideoKeyFrameInt = 180,
        videoDelayFrames = 0,
        reliable = false,
#if !VIDEOSDK_251
        fragment = false,
#endif
    };

    private Photon.Voice.ILogger logger;
    public FusionVoiceClient fusionVoiceClient;

    private bool didVoiceConnectionJoined = false;
    private LocalVoiceVideo localVoiceVideo;
    public bool screenSharingInProgress = false;

    private object emitterUserData = null;

#if UWC_EMITTER_ENABLED
    private int _desktopIndex = 0;
    public int DesktopIndex
    {
        get => _desktopIndex;
        set
        {
            _desktopIndex = value;
            if (captureHost)
                captureHost.DesktopIndex = _desktopIndex;
        }
    }
#endif

    private void Awake()
    {
        if (fusionVoiceClient == null)
        {
            Debug.LogError("ScreenSharingEmitter: FusionVoiceClient não definido. Buscando automaticamente na cena...");
            fusionVoiceClient = FindObjectOfType<FusionVoiceClient>(true);
        }

        if (previewRenderer) previewRenderer.enabled = false;
        if (previewImage) previewImage.enabled = false;

#if UWC_EMITTER_ENABLED
        if (offlinePreview) offlinePreview.gameObject.SetActive(true);
#endif
    }

    private void Start()
    {
        logger = new Photon.Voice.Unity.Logger();
    }

    private void Update()
    {
        if (!didVoiceConnectionJoined && fusionVoiceClient && fusionVoiceClient.ClientState == Photon.Realtime.ClientState.Joined)
        {
            didVoiceConnectionJoined = true;
            OnVoiceJoined();
        }
    }

    public void OnVoiceJoined()
    {
        if (!enabled) return;
        if (startSharingOnVoiceConnectionAvailable)
            ConnectScreenSharing();
    }

#if UWC_EMITTER_ENABLED
    // ---------- IMPLEMENTAÇÃO WINDOWS / UWC ----------

    private void AddCameraScreensharing(object userData = null)
    {
        status = Status.WaitingScreenCaptureAvailability;
        emitterUserData = userData;

        if (screenRecorder == null)
        {
            screenRecorder = new Photon.Voice.Unity.uWindowCaptureRecorder(gameObject);
        }

        if (captureHost == null)
        {
            // uWindowCaptureRecorder cria automaticamente um host se não houver um na cena
            captureHost = GameObject.FindObjectOfType<Photon.Voice.Unity.uWindowCaptureHost>();
            if (captureHost != null)
            {
                captureHost.Type = global::uWindowCapture.WindowTextureType.Desktop;
            }
        }

        if (captureHost != null)
        {
            captureHost.DesktopIndex = DesktopIndex;
        }

        if (screenRecorder != null)
        {
            screenRecorder.OnReady += UWCRecorderReady;
        }
    }

    private void UWCRecorderReady(uWindowCaptureRecorder uwcRecorder)
    {
        if (status == Status.Emitting)
            return;

        status = Status.Emitting;
        Debug.Log("UWCRecorderReady");
        listener?.OnStartEmitting(this);

        // --- Preparar Encoder ---
        int width = settings.UseScreenshareResolution ? uwcRecorder.Width : settings.VideoWidth;
        int height = settings.UseScreenshareResolution ? uwcRecorder.Height : settings.VideoHeight;

        captureHost.encoderFPS = settings.CaptureFPS;

        var info = VoiceInfo.CreateVideo(
            settings.VideoCodec,
            settings.VideoBitrate,
            width,
            height,
            settings.VideoFPS,
            settings.VideoKeyFrameInt,
            emitterUserData
        );

        Debug.Log($"CreateVideo {settings.VideoCodec}, {settings.VideoBitrate}, {width}x{height}, {settings.VideoFPS}fps, keyInt {settings.VideoKeyFrameInt}");

        uwcRecorder.Encoder = Platform.CreateDefaultVideoEncoder(logger, info);

        // --- Preparar Voice ---
#if VIDEOSDK_251
        localVoiceVideo = fusionVoiceClient.VoiceClient.CreateLocalVoiceVideo(info, uwcRecorder);
#else
        localVoiceVideo = fusionVoiceClient.VoiceClient.CreateLocalVoiceVideo(info, uwcRecorder, videoChannel);
        localVoiceVideo.Fragment = settings.fragment;
#endif
        localVoiceVideo.Encrypt = false;
        localVoiceVideo.Reliable = settings.reliable;

        // --- Preview ---
        if (previewRenderer)
        {
            previewRenderer.enabled = true;
            previewRenderer.material = Photon.Voice.Unity.VideoTexture.Shader3D.MakeMaterial(
                uwcRecorder.PlatformView as Texture, Flip.None);
        }
        if (previewImage)
        {
            previewImage.enabled = true;
            previewImage.material = Photon.Voice.Unity.VideoTexture.Shader3D.MakeMaterial(
                uwcRecorder.PlatformView as Texture, Flip.Vertical);
            previewImage.SetAllDirty();
        }
        if (offlinePreview) offlinePreview.gameObject.SetActive(false);

        fusionVoiceClient.VoiceClient.SetRemoteVoiceDelayFrames(settings.VideoCodec, settings.videoDelayFrames);
    }

    /// <summary>Muda a tela capturada pelo uWindowCapture.</summary>
    /// <param name="desktopID">ID do monitor, começando em 0.</param>
    public void SelectDesktop(int desktopID)
    {
        Debug.Log($"Desktop {desktopID} selecionado");
        DesktopIndex = desktopID;
        if (offlinePreview) offlinePreview.desktopIndex = desktopID;
    }

    public async void ConnectScreenSharing()
    {
        Debug.Log("ConnectScreenSharing...");
        status = Status.WaitingVoiceConnection;

        while (this != null && !didVoiceConnectionJoined)
        {
            Debug.Log($"Aguardando conexão do Photon Voice... (estado: {(fusionVoiceClient ? fusionVoiceClient.ClientState.ToString() : "null")})");
            await System.Threading.Tasks.Task.Delay(1000);
        }

        screenSharingInProgress = true;
        AddCameraScreensharing();
    }

    public void DisconnectScreenSharing()
    {
        Debug.Log("DisconnectScreenSharing...");

        status = Status.NotEmitting;
        screenSharingInProgress = false;

        if (localVoiceVideo != null)
        {
            localVoiceVideo.RemoveSelf();
            localVoiceVideo = null;
        }

        if (screenRecorder?.OnReady != null)
            screenRecorder.OnReady -= UWCRecorderReady;

        listener?.OnStopEmitting(this);

        if (screenRecorder != null)
        {
            screenRecorder.Dispose();
            screenRecorder = null;
        }

        if (previewRenderer) previewRenderer.enabled = false;
        if (previewImage) previewImage.enabled = false;
        if (offlinePreview) offlinePreview.gameObject.SetActive(true);
    }

#else
    // ---------- STUBS (iOS / Plataformas sem UWC) ----------

    public void SelectDesktop(int desktopID)
    {
        Debug.LogError("ScreenSharingEmitter: Screen sharing de desktop requer Windows + U_WINDOW_CAPTURE_RECORDER_ENABLE.");
    }

    public async void ConnectScreenSharing()
    {
        Debug.LogError("ScreenSharingEmitter: Screen sharing de desktop requer Windows + U_WINDOW_CAPTURE_RECORDER_ENABLE.");
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public void DisconnectScreenSharing()
    {
        Debug.Log("ScreenSharingEmitter: nada a desconectar nesta plataforma (UWC desabilitado).");
        if (previewRenderer) previewRenderer.enabled = false;
        if (previewImage) previewImage.enabled = false;
    }
#endif
}

#endif // PHOTON_VOICE_VIDEO_ENABLE
