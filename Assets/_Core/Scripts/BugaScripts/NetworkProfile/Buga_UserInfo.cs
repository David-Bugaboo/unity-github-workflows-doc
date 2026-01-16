using UnityEngine;
using Fusion;
using Fusion.Addons.Avatar;
using UnityEngine.Events;

public class Buga_UserInfo : UserInfo
{
    [Header("----- Local UserData -----")]
    public StringVariable usernameVariable;
    public UserDataVariable userDataVariable;

    [Networked(), Capacity(128)]
    public string Description { get; set; }

    [Networked(), Capacity(16)]
    public string MainInterest { get; set; }

    [Networked(), Capacity(16 * 4 + 3)]
    public string Interests { get; set; }

    [Header("Events")]
    public UnityEvent onDescriptionChange;
    public UnityEvent onMainInterestChange;
    public UnityEvent onInterestsChange;

    public void RefreshLocalUsername()
    {
        if (!HasStateAuthority) return;
        usernameVariable.SetValue(UserName.Value);
    }

    private void FixedUpdate()
    {
        if (!HasStateAuthority) return;
        UserName = UserManager.Instance.CurrentUser.name;
        MainInterest = UserManager.Instance.CurrentUser.main_interest;
        Description = UserManager.Instance.CurrentUser.bio;
        AvatarURL = UserManager.Instance.CurrentUser.avatar;
    }
}
