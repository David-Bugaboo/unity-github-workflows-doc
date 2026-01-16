#if PHOTON_VOICE_VIDEO_ENABLE
using Fusion.Addons.ScreenSharing;
using Photon.Voice;
using Photon.Voice.Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;


/***
 * 
 * ScreensharingReceiver manages the reception of screen sharing streams.
 * It watchs for new voice connections, with the VoiceClient.OnRemoteVoiceInfoAction callback.
 * Upon such a connection, it creates a video player with Platform.CreateVideoPlayerUnityTexture.
 * Then, when this video player is ready (OnVideoPlayerReady), it creates a material containing the video player texture, 
 * and pass it to the ScreenSharingScreen with EnablePlayback: the screen will then change its renderer material to this new one.
 * 
 ***/
public class ScreensharingReceiver : MonoBehaviour
{
    public FusionVoiceClient fusionVoiceClient;

    public ScreenSharingScreen defaultRemoteScreen;
    Dictionary<int, IVideoPlayer> videoPlayerByPlayerIds = new Dictionary<int, IVideoPlayer>();
    public Dictionary<int, ScreenSharingScreen> screenByPlayerIds = new Dictionary<int, ScreenSharingScreen>();
    public Dictionary<IVideoPlayer, object> userDataForPlayer = new Dictionary<IVideoPlayer, object>();
    private Photon.Voice.ILogger logger;

    // Set it to true if you target Oculus Quest.
    public bool useCustomQuestScreenShader = true;
    string customQuestScreenShaderName = "QuestVideoTextureExt3D";
    
    [Header("URP Shader Settings")]
    [Tooltip("Use URP/Unlit shader instead of custom Quest shader. Set to true to use URP Unlit shader.")]
    public bool useURPUnlitShader = false;
    [Tooltip("Name of the URP Unlit shader. Default: 'Universal Render Pipeline/Unlit' or 'Shader Graphs/Unlit'")]
    public string urpUnlitShaderName = "Universal Render Pipeline/Unlit";

    private bool voiceClientRegistrationDone = false;

    private void Start()
    {
        if (fusionVoiceClient == null)
        {
            Debug.Log("videoConnection not set: searching it");
            fusionVoiceClient = FindObjectOfType<FusionVoiceClient>(true);
        }

        RegisterVoiceClient();

        logger = new Photon.Voice.Unity.Logger();
        if (defaultRemoteScreen) defaultRemoteScreen.ToggleScreenVisibility(false);

    }


    private void RegisterVoiceClient()
    {
        if (voiceClientRegistrationDone == true || fusionVoiceClient == null || fusionVoiceClient.Client == null || fusionVoiceClient.VoiceClient == null)
            return;

        fusionVoiceClient.VoiceClient.OnRemoteVoiceInfoAction += OnRemoteVoiceInfoAction;
        voiceClientRegistrationDone = true;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        foreach (var v in videoPlayerByPlayerIds)
        {
            if (v.Value != null) v.Value.Dispose();
        }
        videoPlayerByPlayerIds.Clear();
    }

    private void Update()
    {
        RegisterVoiceClient();
    }
    ScreenSharingScreen ScreenForPlayerId(int playerId)
    {
        if (screenByPlayerIds.ContainsKey(playerId))
        {
            return screenByPlayerIds[playerId];
        }
        return defaultRemoteScreen;
    }

    // Called when a video playing stream is detected
    private void OnRemoteVoiceInfoAction(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo, ref RemoteVoiceOptions options)
    {
        Debug.Log($"OnRemoteVoiceInfoAction {channelId} {playerId} {voiceId}");
        switch (voiceInfo.Codec)
        {
            case Codec.VideoVP8:
            case Codec.VideoVP9:
            case Codec.VideoH264:
                if (videoPlayerByPlayerIds.ContainsKey(playerId))
                {
                    Debug.LogError($"Error: This player {playerId} is already sending a stream");
                    return;
                }
                IVideoPlayer videoPlayer = Platform.CreateVideoPlayerUnityTexture(logger, voiceInfo, (player) => {
                    videoPlayerByPlayerIds.Add(playerId, player);
                    userDataForPlayer[player] = voiceInfo.UserData;

                    OnVideoPlayerReady(player);
                });

                Debug.Log("ScreenSharingReceiver.OnRemoteVoiceInfoAction: Decoder: " + videoPlayer.Decoder + " / UserData: " + voiceInfo.UserData);
                options.Decoder = videoPlayer.Decoder;


                options.OnRemoteVoiceRemoveAction += () =>
                {
                    Debug.Log($"OnRemoteVoiceRemoveAction playerId:{playerId} videoPlayer:{videoPlayer}");
                    videoPlayer.Dispose();
                    videoPlayerByPlayerIds.Remove(playerId);
                    userDataForPlayer.Remove(videoPlayer);
                    var screen = ScreenForPlayerId(playerId);
                    if (screen)
                    {
                        screen.DisablePlayback(videoPlayer);
                    }
                };

                break;
            default:
                Debug.Log($"Voice Info: {voiceInfo.Codec} {voiceInfo}");
                break;
        }
    }

    private void OnApplicationQuit()
    {
        Cleanup();
    }

    private void OnVideoPlayerReady(IVideoPlayer videoPlayer)
    {
        Debug.Log($"OnVideoPlayerReady videoPlayer");
        
        // Validate videoPlayer for iOS
        if (videoPlayer == null)
        {
            Debug.LogError("OnVideoPlayerReady: videoPlayer is null");
            return;
        }

        // Validate PlatformView for iOS
        if (videoPlayer.PlatformView == null)
        {
            Debug.LogError("OnVideoPlayerReady: videoPlayer.PlatformView is null");
            return;
        }

        ScreenSharingScreen screen = null;
        foreach (var entry in videoPlayerByPlayerIds)
        {
            if (videoPlayer == entry.Value)
            {
                screen = ScreenForPlayerId(entry.Key);
                break;
            }
        }

        if (screen == null)
        {
            Debug.LogError("OnVideoPlayerReady: screen is null. No screen found for this video player.");
            return;
        }

        if (videoPlayer.PlatformView is Texture)
        {
            try
            {
                var flip = videoPlayer.Flip;
                var screenTexture = videoPlayer.PlatformView as Texture;

                // Validate screenTexture for iOS
                if (screenTexture == null)
                {
                    Debug.LogError("OnVideoPlayerReady: screenTexture is null after cast");
                    return;
                }

                Material material = null;

                // Priority: URP Unlit > Custom Quest Shader > Default VideoTexture Shader
                if (useURPUnlitShader)
                {
                    // Use URP/Unlit shader
                    var shader = Shader.Find(urpUnlitShaderName);
                    if (shader == null)
                    {
                        Debug.LogError($"URP Unlit shader '{urpUnlitShaderName}' not found. Trying alternative names...");
                        // Try alternative URP shader names
                        shader = Shader.Find("Universal Render Pipeline/Unlit") ?? 
                                 Shader.Find("Shader Graphs/Unlit") ?? 
                                 Shader.Find("Unlit/Texture");
                        
                        if (shader == null)
                        {
                            throw new Exception($"URP Unlit shader not found. Make sure URP is installed and shader name is correct.");
                        }
                    }
                    
                    // Validate shader before creating material (for iOS)
                    if (shader == null)
                    {
                        throw new Exception("Shader is null after search");
                    }

                    material = new Material(shader);
                    if (material == null)
                    {
                        throw new Exception("Failed to create Material with URP Unlit shader");
                    }

                    // URP Unlit uses _BaseMap in newer versions, but _MainTex also works
                    if (material.HasProperty("_BaseMap"))
                    {
                        material.SetTexture("_BaseMap", screenTexture);
                    }
                    else if (material.HasProperty("_MainTex"))
                    {
                        material.SetTexture("_MainTex", screenTexture);
                    }
                    else
                    {
                        Debug.LogWarning("URP Unlit shader doesn't have _BaseMap or _MainTex property. Using mainTexture.");
                        material.mainTexture = screenTexture;
                    }
                    
                    // Note: URP Unlit doesn't have _Flip property, so flip is handled by the video player itself
                    // If you need to flip the texture, you can modify the UV coordinates or use a custom shader graph
                    // For most cases, the video player handles the flip automatically
                    screen.usingShaderRequiringMatrix = false;
                }
                else if (useCustomQuestScreenShader && Application.platform == RuntimePlatform.Android)
                {
                    // Use custom Quest shader - Android code remains unchanged
                    var shader = Resources.Load<Shader>(customQuestScreenShaderName);
                    if (shader == null)
                    {
                        throw new Exception("Shader resource " + customQuestScreenShaderName + " fails to load");
                    }
                    material = new Material(shader);
                    material.SetTexture("_MainTex", screenTexture);
                    material.SetVector("_Flip", new Vector4(flip.IsHorizontal ? -1 : 1, flip.IsVertical ? -1 : 1, 0, 0));
                    screen.usingShaderRequiringMatrix = true;
                }
                else
                {
                    // Use default VideoTexture shader - iOS and other platforms
                    try
                    {
                        material = Photon.Voice.Unity.VideoTexture.Shader3D.MakeMaterial(screenTexture, flip);
                        if (material == null)
                        {
                            throw new Exception("Photon.Voice.Unity.VideoTexture.Shader3D.MakeMaterial returned null");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to create material with default VideoTexture shader: {ex.Message}", ex);
                    }
                    screen.usingShaderRequiringMatrix = false;
                }

                if (material != null)
                {
                    screen.EnablePlayback(material, videoPlayer);
                }
                else
                {
                    throw new Exception("Failed to create material for video playback - material is null");
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Error while creating video material: {0}. StackTrace: {1}", e.Message, e.StackTrace);
            }
        }
        else
        {
            Debug.LogWarning($"OnVideoPlayerReady: PlatformView is not a Texture. Type: {videoPlayer.PlatformView?.GetType()?.Name ?? "null"}");
        }
    }
}
#endif