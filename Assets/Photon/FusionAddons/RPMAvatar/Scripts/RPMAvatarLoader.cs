using Fusion.XR.Shared;
using Fusion.XR.Shared.Rig;
using Newtonsoft.Json;
#if READY_PLAYER_ME
using ReadyPlayerMe;
using ReadyPlayerMe.Core;
#endif
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.EyeMovementSimulation;
using Fusion.Addons.HapticAndAudioFeedback;
using System.Threading.Tasks;
using Fusion.Samples.IndustriesComponents;

namespace Fusion.Addons.Avatar.ReadyPlayerMe
{
    public class RPMAvatarLoader : MonoBehaviour, IAvatar
    {
       public enum RPMAvatarKind
       {
          None,
          V1,
          V2,
          V3
       }
       [System.Serializable]
       public struct RPMAvatarInfo
       {
          public GameObject avatarGameObject;
          public string avatarURL;
          public RPMAvatarKind kind;
#if READY_PLAYER_ME
          public AvatarMetadata metadata;
#endif
          public List<string> handsPaths;
          public List<GameObject> handsGameObjects;
          public string facePath;
          public SkinnedMeshRenderer faceMeshRenderer;
          public string glassesPath;
          public Renderer glassesRenderer;
          public string headPath;
          public GameObject headGameObject;
          public List<string> eyesPaths;
          public List<GameObject> eyeGameObjects;
          public List<Renderer> avatarRenderers;
       }
       [System.Flags]
       public enum OptionalFeatures
       {
          None = 0,
          HideRPMHands = 1,
          OptimizeAvatarRenderers = 2,
          DetectColorForAvatarDescription = 4,
          EyeMovementSimulation = 8,
          LipSynchronisation = 16,
          LipSyncWeightPonderation = 256,
          EyeBlinking = 32,
          OnLoadedSoundEffect = 64,
          DownloadThrottling = 128,
          AllOptions = ~0,
       }
       bool ShouldApplyFeature(OptionalFeatures feature) => (feature & avatarOptionalFeatures) != 0;
       public OptionalFeatures avatarOptionalFeatures = OptionalFeatures.AllOptions;
       protected static readonly string readyPlayerMeHead = "Armature/Hips/Spine/Neck";
       protected static readonly string readyPlayerMeEyeLeft = "Head/LeftEye";
       protected static readonly string readyPlayerMeEyeRight = "Head/RightEye";
       protected static readonly string readyPlayerMeRightHand = "Armature/Hips/Spine/RightHand";
       protected static readonly string readyPlayerMeLeftHand = "Armature/Hips/Spine/LeftHand";
       protected static readonly string[] readyPlayerMeFaceRendererPaths = new string[] { "Avatar_Renderer_Head", "Avatar_Renderer_Avatar", "Renderer_Head", "Renderer_Avatar" };
       protected static readonly string[] readyPlayerMeHandsRendererPathsV1 = new string[] { "Avatar_Renderer_Hands", "Renderer_Hands" };
       protected static readonly string[] readyPlayerMeGlassesRendererPaths = new string[] { "Avatar_Renderer_Avatar_Transparent", "Renderer_Avatar_Transparent" };
       public string startingAvatarUrl = "";
       public RPMAvatarInfo avatarInfo = default;
       public float downloadProgress = 0;
#if READY_PLAYER_ME
       AvatarObjectLoader avatarLoader = null;
#endif
       public bool ignoreSkinToneMetadata = true;
       public bool avatarColorRequired = true;
       public bool loadLocalAvatar = true;
       public int lodLevel = 0;
       public float avatarRPMScalefactor = 1.2f;
       public Transform avatarParent = null;
       private Vector3 readyPlayerMeHeadOffset = new Vector3(0f, -0.28f, -0.079f);
       public string soundForAvatarLoaded = "OnAvatarLoaded";
       private NetworkRig networkRig;
       private SoundManager soundManager;
       private AvatarRepresentation avatarRepresentation;
       private PerformanceManager performanceManager;
       private PerformanceManager.TaskToken? avatarLoadingToken;
       static readonly List<string> facialAnimationBlendShapes = new List<string>() {
             "viseme_sil", "viseme_PP", "viseme_FF", "viseme_TH", "viseme_DD",
             "viseme_kk", "viseme_CH", "viseme_SS", "viseme_nn", "viseme_RR",
             "viseme_aa", "viseme_E", "viseme_I", "viseme_O", "viseme_U"
          };
       List<int> facialAnimationBlendShapesIndex = new List<int>();
       [SerializeField] float OVRLipSyncWeightPonderation = 100f;
       [SerializeField] float simpleLipSyncWeightFactor = 1f;
       [SerializeField] float lipSyncVolumeAmplification = 20f;
       [SerializeField] float maxSameDownloadWaitTime = 10f;
       [SerializeField] bool copyRPMLoaderAvatar = false;
       [SerializeField] Vector3 baseLeftEyeRotation = new Vector3(90, 0, 0);
       [SerializeField] Vector3 baseRightEyeRotation = new Vector3(90, 0, 0);
       [SerializeField] Vector3 baseLeftEyeRotationV3 = new Vector3(-90, 180, 0);
       [SerializeField] Vector3 baseRightEyeRotationV3 = new Vector3(-90, 180, 0);
#region Cache
       [System.Serializable]
       public struct RPMCachedAvatarInfo
       {
          public GameObject avatarGameObject;
          public string avatarURL;
#if READY_PLAYER_ME
          public AvatarMetadata metadata;
#endif
       }
       static Dictionary<string, RPMCachedAvatarInfo> SharedAvatarCacheByURL = new Dictionary<string, RPMCachedAvatarInfo>();
       static List<string> LoadingAvatarURLs = new List<string>();
#endregion
#region IAvatar
       [Header("IAvatar info")]
       [SerializeField]
       private AvatarStatus _avatarStatus = AvatarStatus.RepresentationLoading;
       public AvatarKind AvatarKind => AvatarKind.ReadyPlayerMe;
       public AvatarStatus AvatarStatus
       {
          get { return _avatarStatus; }
          set { _avatarStatus = value; }
       }
       public int TargetLODLevel => lodLevel;
       public AvatarUrlSupport SupportForURL(string url)
       {
          return AvatarUrlSupport.Maybe;
       }
       public string AvatarURL => avatarInfo.avatarURL;
       public void RemoveCurrentAvatar()
       {
           RemoveAvatar();
       }
       public GameObject AvatarGameObject => avatarInfo.avatarGameObject;
       public string RandomAvatar()
       {
           return LoadRandomAvatar();
       }
       public bool ShouldLoadLocalAvatar => loadLocalAvatar;
       RPMAvatarLibrary avatarLibrary;
       public string LoadRandomAvatar() {
          if(avatarLibrary == null) avatarLibrary = FindObjectOfType<RPMAvatarLibrary>();
          if (avatarLibrary == null) return null;
          return avatarLibrary.RandomAvatar(); 
       }
       public void ChangeAvatar(string avatarURL)
       {
#if READY_PLAYER_ME
          if (avatarURL == avatarInfo.avatarURL)
          {  
             return;
          }
          RemoveCurrentAvatarObject();
          if (string.IsNullOrEmpty(avatarURL))
          {
             UnableToLoadAvatar();
          }
          else
          {
             LoadAvatar(avatarURL);
          }
#else
          Debug.LogError("Ready Player Me package not installed (READY_PLAYER_ME not defined)");
#endif
       }
#endregion
#region MonoBehaviour
        void OnEnable()
       {
          networkRig = GetComponentInParent<NetworkRig>();
          avatarRepresentation = GetComponentInParent<AvatarRepresentation>();
#if READY_PLAYER_ME
#else
          Debug.LogError("Ready Player Me package not installed (READY_PLAYER_ME not defined)");
#endif
       }
       void Start()
       {
            if (string.IsNullOrEmpty(startingAvatarUrl) == false)
            {
             ChangeAvatar(startingAvatarUrl);
            }
       }
       private void OnDestroy()
       {
          RemoveCachedEntries();
#if READY_PLAYER_ME
            if (avatarLoader != null && string.IsNullOrEmpty(avatarInfo.avatarURL) == false)
            {
             UnregisterLoadingURL(avatarInfo.avatarURL);
            }
#endif
       }
       private void LateUpdate()
       {
          if (ShouldApplyFeature(OptionalFeatures.LipSyncWeightPonderation))
             AdaptOVRLipsyncWeights();
       }
#endregion
#region Avatar cleanup
        public void RemoveAvatar()
       {
          RemoveCurrentAvatarObject();
       }
       void RemoveCurrentAvatarObject()
       {
          if (avatarInfo.avatarGameObject)
          {
             RemoveCachedEntries();
             Debug.Log("[RPMAvatar] Remove CurrentRPMAvatar");
             Gazer gazer;
             if (networkRig)
                gazer = networkRig.headset.GetComponentInChildren<Gazer>();
             else
                gazer = GetComponentInChildren<Gazer>();
                if (gazer)
                {
                gazer.gazingTransforms = new List<Transform>();
                gazer.eyeRendererVisibility = null;
             }
             AvatarStatus = AvatarStatus.NotLoaded;
             if (avatarRepresentation) avatarRepresentation.RemoveRepresentation(this, avatarInfo.avatarRenderers);
             Destroy(avatarInfo.avatarGameObject);
          }
          avatarInfo = default;
       }
#endregion
#region Avatar loading
#if READY_PLAYER_ME
       private async void LoadAvatar(string avatarURL)
       {
          AvatarStatus = AvatarStatus.RepresentationLoading;
          if (avatarRepresentation) avatarRepresentation.LoadingRepresentation(this);
          if (await TryLoadCachedAvatar(avatarURL))
             return;
          if (avatarURL == avatarInfo.avatarURL)
          {
             return;
          }
          avatarInfo.avatarURL = avatarURL;
          if (ShouldApplyFeature(OptionalFeatures.DownloadThrottling))
            {
             await RequestDownloadAuthorizationToken();
             avatarURL = avatarInfo.avatarURL;
          }
          if (avatarLoader != null)
            {
             avatarLoader.Cancel();
          }
          avatarLoader = new AvatarObjectLoader();
          Debug.Log($"[RPMAvatar] Loading avatar url {avatarURL} ...");
          float time = Time.realtimeSinceStartup;
          avatarLoader.OnCompleted += (a, args) => {
             Debug.Log($"[RPMAvatar] Loaded avatar url ({(int)((Time.realtimeSinceStartup - time)*1000f)}ms): {avatarURL}");
             avatarLoader = null;
             AvatarLoadedCallback(args.Avatar, args.Metadata, avatarURL);
             UnregisterLoadingURL(avatarURL);
          };
          avatarLoader.OnFailed += (a, args) =>
          {
             avatarLoader = null;
             Debug.LogError($"[RPMAvatar] Unable to load RPM avatar for {avatarURL}: {args.Message}");
             UnableToLoadAvatar();
             UnregisterLoadingURL(avatarURL);
             if (ShouldApplyFeature(OptionalFeatures.DownloadThrottling))
                {
                FreeAuthorizationToken();
             }
          };
          avatarLoader.OnProgressChanged += (a, args) => {
             downloadProgress = args.Progress;
          };
          RegisterLoadingURL(avatarURL);
          avatarLoader.LoadAvatar(avatarURL);
    }
#endif
    void UnableToLoadAvatar()
    {
        AvatarStatus = AvatarStatus.RepresentationMissing;
        if (avatarRepresentation)
        {
            avatarRepresentation.RepresentationUnavailable(this);
        }
    }
#if READY_PLAYER_ME
       RPMAvatarInfo ParseAvatar(GameObject avatar, AvatarMetadata metadata, string avatarURL)
        {
          RPMAvatarInfo info = default;
          info.avatarURL = avatarURL;
          info.avatarGameObject = avatar;
          info.metadata = metadata;
          info.kind = RPMAvatarKind.None;
          info.handsPaths = new List<string>();
          info.handsGameObjects = new List<GameObject>();
          info.eyesPaths = new List<string>();
          info.eyeGameObjects = new List<GameObject>();
          info.avatarRenderers = new List<Renderer>();
          Transform handsRendererTransform = null;
          foreach(var handsPath in readyPlayerMeHandsRendererPathsV1)
          {
             handsRendererTransform = avatar.transform.Find(handsPath);
             if (handsRendererTransform)
             {
                info.kind = RPMAvatarKind.V1;
                info.handsPaths.Add(handsPath);
                info.handsGameObjects.Add(handsRendererTransform.gameObject);
                break;
             }
          }
          if (handsRendererTransform == null)
          {
             info.kind = RPMAvatarKind.V2;
             foreach (var handPath in new string[] { readyPlayerMeLeftHand, readyPlayerMeRightHand })
             {
                Transform hand = info.avatarGameObject.transform.Find(handPath);
                if (hand == null)
                {
                   Debug.LogError("Hand not found " + handPath);
                   continue;
                }
                info.handsPaths.Add(handPath);
                info.handsGameObjects.Add(hand.gameObject);
             }
          }
          if (info.kind == RPMAvatarKind.V2 && string.IsNullOrEmpty(metadata.SkinTone) == false)
          {
             info.kind = RPMAvatarKind.V3;
          }
          Transform headTransform = avatar.transform.Find(readyPlayerMeHead);
          if (headTransform)
          {
             info.headPath = readyPlayerMeHead;
             info.headGameObject = headTransform.gameObject;
          }
          else
          {
             Debug.LogError("[RPMAvatar] ReadyPlayerMe Head has not been found !");
          }
          foreach (var faceTransformPath in readyPlayerMeFaceRendererPaths)
          {
             Transform faceRendererTransform = avatar.transform.Find(faceTransformPath);
             if (faceRendererTransform)
             {
                info.facePath = faceTransformPath;
                info.faceMeshRenderer = faceRendererTransform.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                break;
             }
          }
          if (info.faceMeshRenderer == null)
          {
             Debug.LogError("Unable to find faceMeshRenderer");
          }
          foreach(var glassPath in readyPlayerMeGlassesRendererPaths)
          {
             Transform glasses = avatar.transform.Find(glassPath);
             if (glasses)
             {
                info.glassesPath = glassPath;
                info.glassesRenderer = glasses.GetComponentInChildren<SkinnedMeshRenderer>();
                break;
             }
          }
          if (info.headGameObject)
          {
             foreach (var eyePath in new string[] { readyPlayerMeEyeLeft, readyPlayerMeEyeRight })
             {
                var eye = info.headGameObject.transform.Find(eyePath);
                if (eye)
                {
                   info.eyeGameObjects.Add(eye.gameObject);
                }
                else
                {
                   Debug.LogError($"Eye {eyePath} not found !");
                }
             }
          }
          info.avatarRenderers = new List<Renderer>(info.avatarGameObject.GetComponentsInChildren<Renderer>());
          return info;
       }
#endif
#if READY_PLAYER_ME
       private void AvatarLoadedCallback(GameObject avatar, AvatarMetadata metaData, string avatarURL)
       {
            if (copyRPMLoaderAvatar)
            {
             GameObject avatarCopy = GameObject.Instantiate(avatar);
             Destroy(avatar);
             avatar = avatarCopy;
            }
          Debug.Log($"[RPMAvatar] Avatar Loaded. Metadata: {metaData.SkinTone}");
          RemoveCurrentAvatarObject();
          avatarInfo = ParseAvatar(avatar, metaData, avatarURL);
            if (ShouldApplyFeature(OptionalFeatures.HideRPMHands))
            {
             HideHands(avatarInfo);
          }
          if (avatarInfo.headGameObject != null)
          {
             if (avatarParent == null && networkRig) avatarParent = networkRig.headset.transform;
             if (avatarParent == null) avatarParent = transform;
             var headsetPosition = avatar.transform.InverseTransformPoint(avatarInfo.headGameObject.transform.position);
             avatar.transform.SetParent(avatarParent, false);
             avatar.transform.position = avatarParent.position;
             avatar.transform.rotation = avatarParent.rotation;
             avatar.transform.localPosition = -headsetPosition + readyPlayerMeHeadOffset;
             avatar.transform.localScale = avatarRPMScalefactor * Vector3.one;
          } 
          else
            {
             Debug.LogError("[RPMAvatar] Missing head gameobject");
            }
          if (ShouldApplyFeature(OptionalFeatures.OptimizeAvatarRenderers))
            {
             OptimizeAvatarRenderers(avatarInfo);
          }
          if (ShouldApplyFeature(OptionalFeatures.DetectColorForAvatarDescription))
            {
             Color skinColor = ExtractSkinColor(ref avatarInfo, ignoreSkinToneMetadata: ignoreSkinToneMetadata, avatarColorRequired: avatarColorRequired);
             if (avatarRepresentation)
             {
                avatarRepresentation.ChangeHandColor(skinColor);
             }
          }
          if (ShouldApplyFeature(OptionalFeatures.EyeMovementSimulation))
            {
             Gazer gazer = null;
             if (networkRig)
                gazer = networkRig.headset.GetComponentInChildren<Gazer>();
             else
                gazer = GetComponentInChildren<Gazer>();
             if (gazer)
             {
                Vector3[] eyeRotations;
                if (avatarInfo.kind == RPMAvatarKind.V1 || avatarInfo.kind == RPMAvatarKind.V2)
                {
                   eyeRotations = new Vector3[] {baseLeftEyeRotation, baseRightEyeRotation};
                }
                else
                    {
                   eyeRotations = new Vector3[] { baseLeftEyeRotationV3, baseRightEyeRotationV3 };
                }
                ActivateEyes(avatarInfo, gazer, eyeRotations);
             }
             else
             {
                Debug.LogWarning("[RPMAvatar] No gazer: eye movement simulation won't be activated");
             }
          }
          if (ShouldApplyFeature(OptionalFeatures.LipSynchronisation))
            {
             ConfigureLipSync();
          }
          if (ShouldApplyFeature(OptionalFeatures.EyeBlinking))
            {
             ConfigureEyeBlink(avatar);
          }
          CacheAvatar(avatarInfo);
          if (ShouldApplyFeature(OptionalFeatures.OnLoadedSoundEffect))
            {
             if (string.IsNullOrEmpty(soundForAvatarLoaded) == false)
             {
                if (soundManager == null) soundManager = SoundManager.FindInstance();
                if (soundManager) soundManager.PlayOneShot(soundForAvatarLoaded);
             }
          }
          if (ShouldApplyFeature(OptionalFeatures.DownloadThrottling))
            {
             FreeAuthorizationToken();
          }
          AvatarStatus = AvatarStatus.RepresentationAvailable;
          if (avatarRepresentation) avatarRepresentation.RepresentationAvailable(this, avatarInfo.avatarRenderers);
       }
#endif
#endregion
#region Avatar edits
        static void HideHands(RPMAvatarInfo info)
       {
          if (info.kind == RPMAvatarKind.V1)
          {
             foreach (var hand in info.handsGameObjects) hand.SetActive(false);
          }
          else
          {
             foreach (var hand in info.handsGameObjects)
             {
                hand.transform.localScale = Vector3.zero;
                if (info.headGameObject) hand.transform.position = info.headGameObject.transform.position;
             }
          }
       }
       private static void OptimizeAvatarRenderers(RPMAvatarInfo info)
       {
          foreach (Renderer avatarRenderer in info.avatarRenderers)
          {
             avatarRenderer.receiveShadows = false;
             avatarRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
             avatarRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                if (avatarRenderer.sharedMaterial == null)
                {
                Debug.LogError("[RPMAvatar] Missing avatar material (corrupted cache/library data, ...)");
                return;
                }
             avatarRenderer.sharedMaterial.SetFloat("_Roughness", 1);            
             avatarRenderer.sharedMaterial.SetFloat("roughnessFactor", 1);
             avatarRenderer.sharedMaterial.SetTexture("_BumpMap", null);
          }
       }
       private void ConfigureEyeBlink(GameObject avatar)
       {
#if READY_PLAYER_ME
          if (avatar.GetComponent<EyeAnimationHandler>() == null)
          {
             avatar.AddComponent<EyeAnimationHandler>();
          }
#endif
       }
       public static Texture2D FindFaceRendererMaterialTexture(SkinnedMeshRenderer faceMeshRenderer)
        {
          Texture2D skinTexture = null;
          Material skinMaterial = faceMeshRenderer.sharedMaterials[0];
          if (skinMaterial)
            {
             if (skinMaterial.HasTexture("_MainTex"))
             {
                skinTexture = (Texture2D)skinMaterial.GetTexture("_MainTex");
             }
             else if (skinMaterial.HasTexture("baseColorTexture"))
             {
                skinTexture = (Texture2D)skinMaterial.GetTexture("baseColorTexture");
             }
             if (skinTexture == null) skinTexture = (Texture2D)skinMaterial.mainTexture;
          } 
          else
            {
             Debug.LogError("Missing face material");
            }
          return skinTexture;
       }
       static Color ExtractSkinColor(ref RPMAvatarInfo info, bool ignoreSkinToneMetadata, bool avatarColorRequired)
       {
#if READY_PLAYER_ME
          if (info.faceMeshRenderer == null || info.faceMeshRenderer.sharedMaterials.Length == 0)
          {
              Debug.LogError("Skinned Mesh Renderer ou material não encontrado no avatar.");
              return Color.white;
          }
          Texture2D skinTexture = FindFaceRendererMaterialTexture(info.faceMeshRenderer);
          Color skinColor = Color.clear;
          bool isSkinToneValid = string.IsNullOrEmpty(info.metadata.SkinTone) == false && ColorUtility.TryParseHtmlString(info.metadata.SkinTone, out skinColor);
          bool shouldDestroySkinTexture = false;
          if(skinTexture != null && skinTexture.isReadable == false)
          {
              if (string.IsNullOrEmpty(info.metadata.SkinTone) || avatarColorRequired || ignoreSkinToneMetadata)
              {
                  RenderTexture copyRT = RenderTexture.GetTemporary(skinTexture.width, skinTexture.height);
                  Graphics.Blit(skinTexture, copyRT);
                  RenderTexture currentRT = RenderTexture.active;
                  RenderTexture.active = copyRT;
                  skinTexture = new Texture2D(skinTexture.width, skinTexture.height, skinTexture.format, false);
                  shouldDestroySkinTexture = true;
                  skinTexture.ReadPixels(new Rect(0, 0, copyRT.width, copyRT.height), 0, 0);
                  skinTexture.Apply();
                  RenderTexture.active = currentRT;
                  RenderTexture.ReleaseTemporary(copyRT);
              }
          }
          if(skinTexture != null && skinTexture.isReadable)
          {
                if (info.kind == RPMAvatarKind.V1 || info.kind == RPMAvatarKind.V2)
                {
                    skinColor = skinTexture.GetPixel(skinTexture.width - 1, (int)(skinTexture.height * 0.75f));
                }
                else if (info.kind == RPMAvatarKind.V3)
                {
                    if (ignoreSkinToneMetadata || !isSkinToneValid)
                    {
                        skinColor = skinTexture.GetPixel(0, (int)(skinTexture.height * 0.75f));
                    }
                }
          }
          if (shouldDestroySkinTexture)
          {
              Destroy(skinTexture);
          }
          return skinColor;
#else
          return Color.white;
#endif
       }
       static void ActivateEyes(RPMAvatarInfo info, Gazer gazer, Vector3[] eyeRotations)
       {
          RendererVisible rendererVisible = null;
          if (info.faceMeshRenderer)
          {
             rendererVisible = info.faceMeshRenderer.gameObject.GetComponent<RendererVisible>();
             if (rendererVisible == null) rendererVisible = info.faceMeshRenderer.gameObject.AddComponent<RendererVisible>();
          }
          else
          {
             Debug.LogError("faceMeshRenderer not found: unable to add RendererVisible to optimize gazer");
          }
          gazer.eyeRendererVisibility = rendererVisible;
          gazer.gazingTransformOffsets = new List<Vector3>(eyeRotations);
          gazer.gazingTransforms = new List<Transform>();
          foreach (var eye in info.eyeGameObjects)
            {
             gazer.gazingTransforms.Add(eye.transform);
          }
       }
       void ConfigureLipSync()
        {
          var audioRootGameObject = ((networkRig != null) ? networkRig.gameObject : avatarParent.gameObject);
#if UNITY_WEBGL && UNITY_2021_2_OR_NEWER && !UNITY_EDITOR
          ConfigureSimpleLipsync(avatarInfo, audioGameObject: audioRootGameObject, simpleLipSyncWeightFactor, lipSyncVolumeAmplification);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR || UNITY_EDITOR_OSX
          ConfigureSimpleLipsync(avatarInfo, audioGameObject: audioRootGameObject, simpleLipSyncWeightFactor,lipSyncVolumeAmplification);
#else
          ConfigureOculusLipsync(avatarInfo, audioGameObject: audioRootGameObject, ref facialAnimationBlendShapesIndex);
#endif
       }
       static void ConfigureSimpleLipsync(RPMAvatarInfo info, GameObject audioGameObject, float simpleLipSyncWeightFactor, float lipSyncVolumeAmplification = 1)
        {
          AudioSource audioSource = audioGameObject.GetComponentInChildren<AudioSource>();
          if (audioSource == null)
            {
             Debug.LogWarning("[RPMAvatar] No audio source: unable to configure lipsync");
          }
          RPMLipSync lipsync = info.avatarGameObject.GetComponent<RPMLipSync>();
          if (lipsync == null)
            {
             lipsync = info.avatarGameObject.AddComponent<RPMLipSync>();
          }
          lipsync.audioSource = audioSource;
          lipsync.lipSyncWeightFactor = simpleLipSyncWeightFactor;
          lipsync.amplituteMultiplier = lipSyncVolumeAmplification;
       }
       static void ConfigureOculusLipsync(RPMAvatarInfo info, GameObject audioGameObject, ref List<int> facialAnimationBlendShapesIndex)
       {
          if (info.faceMeshRenderer == null || info.headGameObject == null || audioGameObject == null)
          {
             Debug.LogError("[RPMAvatar] faceMeshRenderer, audioGameObject or head not found: unable to configure lipsync");
             return;
          }
          OVRLipSyncContext lipSyncContext = audioGameObject.GetComponentInChildren<OVRLipSyncContext>(true);
          OVRLipSyncContextMorphTarget lipSync = audioGameObject.GetComponentInChildren<OVRLipSyncContextMorphTarget>(true);
          AudioSource audioSource = audioGameObject.GetComponentInChildren<AudioSource>();
            if (audioSource == null) {
             Debug.LogWarning("[RPMAvatar] No audio source: unable to configure lipsync");
             return;
          }
            if (lipSyncContext == null)
            {
             lipSyncContext = audioSource.gameObject.AddComponent<OVRLipSyncContext>();
             lipSyncContext.audioSource = audioSource;
             lipSyncContext.provider = OVRLipSync.ContextProviders.Original;
             lipSyncContext.audioLoopback = true;
            }
          if (lipSync == null)
          {
             lipSync = audioSource.gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
          }
          lipSync.enabled = true;
          lipSync.enableVisemeTestKeys = false;
          int visemeCount = 0;
          lipSync.skinnedMeshRenderer = info.faceMeshRenderer;
          facialAnimationBlendShapesIndex = new List<int>();
          foreach (var facialAnimationBlendShape in facialAnimationBlendShapes)
          {
             int blendShapeIndex = info.faceMeshRenderer.sharedMesh.GetBlendShapeIndex(facialAnimationBlendShape);
             facialAnimationBlendShapesIndex.Add(blendShapeIndex);
             if (visemeCount < OVRLipSync.VisemeCount)
             {
                lipSync.visemeToBlendTargets[visemeCount] = blendShapeIndex;
             }
             visemeCount++;
          }
       }
       void AdaptOVRLipsyncWeights()
        {
          if (avatarInfo.faceMeshRenderer == null)
             return;
          foreach (var facialAnimationBlendShapeIndex in facialAnimationBlendShapesIndex)
          {
             avatarInfo.faceMeshRenderer.SetBlendShapeWeight(facialAnimationBlendShapeIndex, avatarInfo.faceMeshRenderer.GetBlendShapeWeight(facialAnimationBlendShapeIndex)/OVRLipSyncWeightPonderation);
          }
       }
#endregion
#region Performance manager
       async Task RequestDownloadAuthorizationToken()
       {
          if (!performanceManager)
          {
             if (networkRig) performanceManager = networkRig.Runner.GetComponentInChildren<PerformanceManager>();
             else performanceManager = FindObjectOfType<PerformanceManager>(true);
          }
          if (performanceManager)
          {
             if (avatarLoadingToken != null)
             {
                Debug.LogError("Cancelling previous loading request");
                performanceManager.TaskCompleted(avatarLoadingToken);
             }
             avatarLoadingToken = await performanceManager.RequestToStartTask(PerformanceManager.TaskKind.NetworkRequest);
             if (avatarLoadingToken == null)
             {
                Debug.LogError("Unable to load avatar: no time slot available");
             }
          }
          else
          {
             Debug.LogError("No PerformanceManager found !");
          }
       }
       void FreeAuthorizationToken()
       {
          if (performanceManager)
          {
             performanceManager.TaskCompleted(avatarLoadingToken);
             avatarLoadingToken = null;
          }
       }
#endregion
#region Cache
       private async Task<bool> TryLoadCachedAvatar(string avatarURL)
       {
          var cachedInfo = await TryFindCachedAvatar(avatarURL);
          if (cachedInfo.avatarURL == avatarURL)
          {
             if (avatarURL == avatarInfo.avatarURL)
             {
                return false;
             }
             GameObject avatar = GameObject.Instantiate(cachedInfo.avatarGameObject);
#if READY_PLAYER_ME
             AvatarLoadedCallback(avatar, cachedInfo.metadata, avatarURL);
#endif
             downloadProgress = 1;
             Debug.Log($"[RPMAvatar] Reusing avatar {avatarURL}");
             return true;
          }
          return false;
       }
       void RemoveCachedEntries()
       {
          UncacheAvatar(avatarInfo);
       }
       public static void CacheAvatar(RPMAvatarInfo info)
       {
#if READY_PLAYER_ME
          CacheAvatar(info.avatarURL, info.avatarGameObject, info.metadata);
#endif
       }
#if READY_PLAYER_ME
       public static void CacheAvatar(string avatarURL, GameObject avatarGameObject, AvatarMetadata metadata)
       {
          if (avatarGameObject == null) return;
          if (SharedAvatarCacheByURL.ContainsKey(avatarURL)) return;
          SharedAvatarCacheByURL[avatarURL] = new RPMCachedAvatarInfo
          {
             avatarGameObject = avatarGameObject,
             avatarURL = avatarURL,
             metadata = metadata
          };
       }
#endif
       public static void UncacheAvatar(RPMAvatarInfo info)
        {
          UncacheAvatar(info.avatarURL, info.avatarGameObject);
       }
       public static void UncacheAvatar(string avatarURL, GameObject avatarGameObject)
       {
          if (avatarURL == null) return;
          if (SharedAvatarCacheByURL.ContainsKey(avatarURL) && SharedAvatarCacheByURL[avatarURL].avatarGameObject == avatarGameObject)
          {
             SharedAvatarCacheByURL.Remove(avatarURL);
          }
       }
       public async Task<RPMCachedAvatarInfo> TryFindCachedAvatar(string avatarURL)
       {
          RPMCachedAvatarInfo info = default;
          await WaitForCurrentDownload(avatarURL);
          if (SharedAvatarCacheByURL.ContainsKey(avatarURL))
          {
             if (SharedAvatarCacheByURL[avatarURL].avatarGameObject == null)
             {
                Debug.LogError("Cached avatar has been destroyed: uncaching it");
                SharedAvatarCacheByURL.Remove(avatarURL);
                return info;
             }
             info = SharedAvatarCacheByURL[avatarURL];
             return info;
          }
          return info;
       }
       void RegisterLoadingURL(string avatarURL)
       {
            if (LoadingAvatarURLs.Contains(avatarURL))
            {
             return;
            }
          LoadingAvatarURLs.Add(avatarURL);
       }
       void UnregisterLoadingURL(string avatarURL)
       {
          if (LoadingAvatarURLs.Contains(avatarURL) == false)
          {
             return;
          }
          LoadingAvatarURLs.Remove(avatarURL);
       }
       async Task WaitForCurrentDownload(string avatarURL)
        {
            if (LoadingAvatarURLs.Contains(avatarURL))
            {
             Debug.Log($"[RPMAvatar] Waiting for the current download to be finished... (avatarUrl: {avatarURL})");
             const int waitStep = 100;
             int watchdog = (int)(1000f * maxSameDownloadWaitTime / (float)waitStep);
                while (watchdog != 0 && LoadingAvatarURLs.Contains(avatarURL))
                {
                await AsyncTask.Delay(waitStep);
                watchdog--;
                }
             if (LoadingAvatarURLs.Contains(avatarURL))
                {
                Debug.LogError($"[RPMAvatar] Download did not end properly. Resume download for the same url. Note that if the first download ends up finishing, RPM might reuse the avatar for the second download, making the first avatar disappear. Url: {avatarURL}");
                }
          }
       }
#endregion
    }
}