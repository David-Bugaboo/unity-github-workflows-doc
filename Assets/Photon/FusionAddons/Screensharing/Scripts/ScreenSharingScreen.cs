using Photon.Voice;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.Addons.ScreenSharing
{
    /***
     * 
     * ScreenSharingScreen manages the screen sharing renderer visibility :
     * When a screensharing is in progress : 
     *          - the screen renderer is enabled and the material is set with the one provided by the ScreensharingEmitter
     *          - the material shader matrix is updated every frame (required for URP in VR)
     *          - the "notPlayingObject" game object is disabled
     *          
     * When the screensharing is stopped : 
     *          - the screen renderer is disabled and the material is restored with the initial one
     *          - the "notPlayingObject" game object is enabled according to the VisibilityBehaviour settings
     * 
     ***/
    public class ScreenSharingScreen : MonoBehaviour
    {
        public Renderer screenRenderer;
        public UnityEvent<bool> onScreensharingScreenVisibility = new UnityEvent<bool>();
        Material initialMaterial;
        public bool isRendering = false;
        // Needed for PhotonVoiceApi/GLES3/QuestVideoTextureExt3D shader
        public bool usingShaderRequiringMatrix = true;
        private IVideoPlayer currentVideoPlayer;
        [System.Flags]
        public enum VisibilityBehaviour
        {
            None = 0,
            HideScreenRendererWhenNotPlaying = 1,
            DisplayNotPlayingObjectWhenNotPlaying = 2
        }
        public VisibilityBehaviour visibilityBehaviour = VisibilityBehaviour.None;
 
        public GameObject notPlayingObject;

        private void Awake()
        {
            if (screenRenderer == null) screenRenderer = GetComponentInChildren<Renderer>();
            if (screenRenderer)
                initialMaterial = screenRenderer.material;
            if (notPlayingObject && (visibilityBehaviour & VisibilityBehaviour.DisplayNotPlayingObjectWhenNotPlaying) != VisibilityBehaviour.DisplayNotPlayingObjectWhenNotPlaying)
            {
                Debug.LogError("A notPlayingObject is set, but DisplayNotPlayingObjectWhenNotPlaying option is not choosen: the object won't be used");
            }
        }

        private float lastTextureUpdateTime = 0f;
        private const float TEXTURE_UPDATE_FORCE_INTERVAL = 0.033f; // Force update every ~33ms (30 FPS) on Android
        private float lastDecoderCheckTime = 0f;
        private const float DECODER_CHECK_INTERVAL = 2f; // Check decoder status every 2 seconds
        private int frameUpdateCounter = 0;

        private void Update()
        {
            if (!isRendering || currentVideoPlayer == null || screenRenderer == null)
                return;

            // Needed for the URP VR shader
            if (usingShaderRequiringMatrix)
            {
                screenRenderer.material.SetMatrix("_localToWorldMatrix", screenRenderer.transform.localToWorldMatrix);
            }

            // Android: Force texture updates for external textures (samplerExternalOES)
            // External textures on Android update via UpdateExternalTexture from native decoder.
            // The texture updates in-place, but Unity may not detect these updates automatically.
            // We need to periodically re-assign the texture to force the GPU to refresh.
            if (Application.platform == RuntimePlatform.Android && currentVideoPlayer.PlatformView is Texture)
            {
                var currentTime = Time.time;
                var videoTexture = currentVideoPlayer.PlatformView as Texture;
                
                if (videoTexture != null && screenRenderer.material != null)
                {
                    // Check decoder status periodically (not every frame to avoid performance impact)
                    if (currentTime - lastDecoderCheckTime >= DECODER_CHECK_INTERVAL)
                    {
                        lastDecoderCheckTime = currentTime;
                        
                        // Check for decoder errors (if decoder has Error property)
                        try
                        {
                            if (currentVideoPlayer.Decoder != null)
                            {
                                var decoderType = currentVideoPlayer.Decoder.GetType();
                                var errorProperty = decoderType.GetProperty("Error");
                                if (errorProperty != null)
                                {
                                    var error = errorProperty.GetValue(currentVideoPlayer.Decoder) as string;
                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        Debug.LogError($"[ScreenSharingScreen] Decoder error detected: {error}");
                                    }
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            // Ignore reflection errors
                        }
                        
                        // Log texture update status for debugging
                        frameUpdateCounter++;
                        if (frameUpdateCounter % 30 == 0) // Log every ~1 second at 30 FPS
                        {
                            var decoderInfo = currentVideoPlayer.Decoder != null ? $"Decoder: {currentVideoPlayer.Decoder.GetType().Name}" : "Decoder: NULL";
                            Debug.Log($"[ScreenSharingScreen] Video texture active. Size: {videoTexture.width}x{videoTexture.height}, Material: {screenRenderer.material.name}, {decoderInfo}");
                            
                            // Check if texture dimensions changed (may indicate decoder format change)
                            if (videoTexture.width != 1280 || videoTexture.height != 720)
                            {
                                Debug.LogWarning($"[ScreenSharingScreen] Texture size changed from expected 1280x720 to {videoTexture.width}x{videoTexture.height}");
                            }
                        }
                    }

                    // Force texture update periodically to ensure frames keep updating
                    // This is critical for Android external textures that update in-place
                    if (currentTime - lastTextureUpdateTime >= TEXTURE_UPDATE_FORCE_INTERVAL)
                    {
                        lastTextureUpdateTime = currentTime;
                        
                        string textureProperty = null;
                        if (screenRenderer.material.HasProperty("_MainTex"))
                        {
                            textureProperty = "_MainTex";
                        }
                        else if (screenRenderer.material.HasProperty("_BaseMap"))
                        {
                            textureProperty = "_BaseMap";
                        }
                        
                        if (textureProperty != null)
                        {
                            // Re-assign texture to force GPU refresh on Android
                            // This ensures external texture updates are rendered
                            // Note: On Android, external textures update in-place via native decoder,
                            // but Unity may not automatically detect these updates
                            screenRenderer.material.SetTexture(textureProperty, videoTexture);
                        }
                    }
                }
            }
        }

        public void EnablePlayback(Material videoMaterial, IVideoPlayer videoPlayer)
        {

            if (currentVideoPlayer != null)
            {
                Debug.Log($"Screen reused by another player {videoPlayer}. Note: make sure that the initial player is disposed by orchestration logic.");
            }
            else
                Debug.Log("Playback started on screen for videoPlayer " + videoPlayer);

            currentVideoPlayer = videoPlayer;
            ToggleScreenVisibility(true);
            screenRenderer.material = videoMaterial;
        }

        public void DisablePlayback(IVideoPlayer videoPlayer)
        {
            if (videoPlayer != currentVideoPlayer)
            {
                Debug.Log("Not stopping playback because videoPlayer hasbeen reused by another player");
                return;
            }
            else
                Debug.Log("Playback stopped for videoPlayer " + videoPlayer);

            currentVideoPlayer = null;
            ToggleScreenVisibility(false);
            screenRenderer.material = initialMaterial;
        }

        public virtual void ToggleScreenVisibility(bool ShouldScreenBeDisplayed)
        {
            isRendering = ShouldScreenBeDisplayed;
            if ((visibilityBehaviour & VisibilityBehaviour.HideScreenRendererWhenNotPlaying) == VisibilityBehaviour.HideScreenRendererWhenNotPlaying)
            {
                screenRenderer.enabled = ShouldScreenBeDisplayed;
            }
            if (notPlayingObject && (visibilityBehaviour & VisibilityBehaviour.DisplayNotPlayingObjectWhenNotPlaying) == VisibilityBehaviour.DisplayNotPlayingObjectWhenNotPlaying)
            {
                notPlayingObject.SetActive(!ShouldScreenBeDisplayed);
            }
            if (onScreensharingScreenVisibility != null) onScreensharingScreenVisibility?.Invoke(ShouldScreenBeDisplayed);
        }
    }
}
