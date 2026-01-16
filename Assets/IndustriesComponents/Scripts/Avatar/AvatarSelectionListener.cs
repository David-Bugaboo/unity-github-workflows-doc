using Fusion.Addons.Avatar.ReadyPlayerMe;
using Fusion.Addons.Avatar.SimpleAvatar;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;

public class AvatarSelectionListener : MonoBehaviour, IAvatarRepresentationListener
{
    AvatarRepresentation avatarRepresentation;
    public AvatarCustomizer avatarCustomizer;

    private void Awake()
    {
        if (avatarCustomizer == null)
            avatarCustomizer = FindObjectOfType<AvatarCustomizer>();
    }
    public void OnAvailableAvatarsListed(AvatarRepresentation avatarRepresentation)
    {
    }

    public void OnRepresentationAvailable(IAvatar avatar, bool isLocalUserAvatar)
    {
        if(avatar is SimpleAvatar simpleAvatar)
        {
            avatarCustomizer.latestSimpleAvatarURL = simpleAvatar.AvatarURL;
        }
        else if (avatar is RPMAvatarLoader rpmAvatar)
        {
            avatarCustomizer.latestRPMAvatarURL = rpmAvatar.AvatarURL;
        }
    }

    public void OnRepresentationUnavailable(IAvatar avatar)
    {
    }

    public void AvailableAvatarListed(AvatarRepresentation avatarRepresentation)
    {
        throw new System.NotImplementedException();
    }
}
