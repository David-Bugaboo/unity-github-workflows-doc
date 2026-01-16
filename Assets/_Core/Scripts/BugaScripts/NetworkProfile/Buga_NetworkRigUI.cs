using TMPro;
using UnityEngine;

public class Buga_NetworkRigUI : MonoBehaviour
{
    public Buga_UserInfo userInfo;
    public UserProfileInfo userProfile;

    public GameObject networkRigUI;

    [Header("----- UI -----")]
    public TMP_Text usernameText;
    public SpriteRenderer mainInterestRenderer;

    [Header("----- Database -----")]
    public InterestLibrary interests;

    private void Update()
    {
        if (userInfo.IsProxy)
        {
            networkRigUI.SetActive(true);
            usernameText.TrySetText(userInfo.UserName.ToString());
            mainInterestRenderer.sprite = interests.FindInterest(userInfo.MainInterest).sprite;
        }
        else
        {
            networkRigUI.SetActive(false);
        }
    }
}