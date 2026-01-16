using UnityEngine;
using Fusion;
using Fusion.Addons.Avatar;
using Fusion.XR.Shared.Rig;
using ReadyPlayerMe.Core;
using UnityEngine.AI;

public class Buga_RPMLoader : NetworkBehaviour, ISpawned
{
    private GameObject avatar;
    private Buga_PlayerController controller;
    public RuntimeAnimatorController animatorController;
    public Transform avatarContainer;
    // private AvatarObjectLoader avatarObjectLoader;

    [Tooltip("Preview avatar to display until avatar loads. Will be destroyed after new avatar is loaded")]
    [SerializeField]
    private GameObject previewAvatar;

    private UserInfo userInfo;
    private string currentAvatarUrl = "";

    private void OnEnable()
    {
        userInfo = GetComponent<UserInfo>();
        if (userInfo != null)
        {
            userInfo.onUserAvatarChange.AddListener(OnAvatarURLChanged);
        }
    }

    private void OnDisable()
    {
        if (userInfo != null)
        {
            userInfo.onUserAvatarChange.RemoveListener(OnAvatarURLChanged);
        }
    }

    private void OnAvatarURLChanged()
    {
        if (userInfo == null) return;

        string avatarUrl = userInfo.AvatarURL.ToString();
        Debug.Log($"[Buga_RPMLoader] Avatar URL changed: {avatarUrl}");

        if (!string.IsNullOrEmpty(avatarUrl))
        {
            LoadAvatar(avatarUrl);
        }
    }

    void SetupAvatar(GameObject targetAvatar)
    {
        if (avatar != null)
        {
            Destroy(avatar);
        }

        avatar = targetAvatar;

        Animator avatarAnimator = avatar.GetComponent<Animator>();
        if (TryGetComponent<NetworkMecanimAnimator>(out var networkAnimator))
        {
            networkAnimator.Animator = avatarAnimator;
        }

        if (controller != null)
        {
            controller.SetupAvatarController(avatar);
        }
        avatarAnimator.runtimeAnimatorController = animatorController;
    }

    private void OnProgessChanged(object sender, ProgressChangeEventArgs e)
    {
        Debug.Log($"Loading avatar <color=blue>{e.Url}</color> {Mathf.Round(e.Progress * 100)}%");
    }

    private void OnLoadFailed(object sender, FailureEventArgs args)
    {
        Debug.Log(args.Message);
    }

    private void OnLoadCompleted(object sender, CompletionEventArgs args)
    {
        if (previewAvatar != null)
        {
            Destroy(previewAvatar);
            previewAvatar = null;
        }
        NetworkObject no = GetComponent<NetworkObject>();
        args.Avatar.name = $"avatar {no.Id}";
        args.Avatar.transform.SetParent(avatarContainer, false);

        SetAvatarLayer(args.Avatar);
        SetupAvatar(args.Avatar);
    }

    private void SetAvatarLayer(GameObject avatarObject)
    {
        int avatarLayer = LayerMask.NameToLayer("Avatar");
        if (avatarLayer == -1)
        {
            Debug.LogWarning("[Buga_RPMLoader] Layer 'Avatar' não encontrada no projeto.");
            return;
        }

        var renderers = avatarObject.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            rend.gameObject.layer = avatarLayer;
        }
    }

    [Rpc]
    public void RPC_ReloadAvatar(PlayerRef player)
    {
        Debug.Log($"[==--==] Reloading avatar [RPC] {player.PlayerId} => {Runner.LocalPlayer.PlayerId}", this);
        if (GetComponent<NetworkObject>().StateAuthority == player)
        {
            // Reset para forçar o reload
            currentAvatarUrl = "";
            LoadAvatar(GetComponent<UserInfo>().AvatarURL.ToString());
        }
    }

    public void UpdateUserInfo()
    {
        if (GetComponent<NetworkObject>().HasInputAuthority)
        {
            GetComponent<UserInfo>().AvatarURL = UserManager.Instance.CurrentUser.avatar;
        }
    }

    public void ReloadAvatar()
    {
        Debug.Log("Reloading avatar");
        RPC_ReloadAvatar(GetComponent<NetworkObject>().StateAuthority);
    }

    public void LoadAvatar(string url)
    {
        //remove any leading or trailing spaces
        string avatarUrl = url.Trim(' ');

        // Evita recarregar o mesmo avatar
        if (avatarUrl == currentAvatarUrl)
        {
            Debug.Log($"[Buga_RPMLoader] Avatar URL já carregada, ignorando: {avatarUrl}");
            return;
        }

        currentAvatarUrl = avatarUrl;

        AvatarLoaderSettings als = new AvatarLoaderSettings();
        AvatarObjectLoader avatarObjectLoader;
        avatarObjectLoader = new AvatarObjectLoader();
        /* avatarObjectLoader.AvatarConfig = new AvatarConfig()
        {
            Pose = ReadyPlayerMe.AvatarLoader.Pose.APose,
        }; */
        avatarObjectLoader.OnCompleted += OnLoadCompleted;
        avatarObjectLoader.OnProgressChanged += OnProgessChanged;
        avatarObjectLoader.OnFailed += OnLoadFailed;
        avatarObjectLoader.LoadAvatar(avatarUrl);
    }

    [ContextMenu("Carregar Avatar")]
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            controller = RigInfo.FindRigInfo(Runner).localHardwareRig.GetComponent<Buga_PlayerController>();
            controller.AssignNetworkPlayer(GetComponent<NetworkObject>());
            if (previewAvatar != null)
            {
                SetupAvatar(previewAvatar);
            }
            GetComponent<NavMeshObstacle>().enabled = false;
        }
        else
        {
            GetComponent<NavMeshObstacle>().enabled = true;
        }
        
        string avatarUrl = GetComponent<UserInfo>().AvatarURL.ToString();
        Debug.Log($"Carregando avatar para o objeto '{gameObject.name}' com a URL: {avatarUrl}");
        
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            LoadAvatar(avatarUrl);
        }
    }
}

