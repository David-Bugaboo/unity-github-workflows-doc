using Fusion.Addons.Avatar;
using TMPro;
using UnityEngine;

public class Buga_ChangeAvatar : MonoBehaviour
{
    private Buga_PlayerController playerController;
    private Buga_RPMLoader bugaRPMLoader;
    public DefaultLocalUserInfoScriptable defaultLocalUserInfoScriptable;
    
    public void OnCompletoAvatarLoader()
    {
        playerController = FindFirstObjectByType<Buga_PlayerController>();
        bugaRPMLoader = playerController.MyNetworkPlayer.GetComponent<Buga_RPMLoader>();
    }

    public void ChangeAvatar(string avatar)
    {
        defaultLocalUserInfoScriptable.defaultLocalAvatarURL = avatar;
        playerController.MyNetworkPlayer.GetComponent<UserInfo>().AvatarURL = avatar;
        bugaRPMLoader.ReloadAvatar();
    }
}
