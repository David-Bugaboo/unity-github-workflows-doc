using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProfileEditorPanel : MonoBehaviour
{
    [SerializeField] private ProfilePanel _profilePanel;
    [SerializeField] Image mainInterestIcon;
    [SerializeField] RawImage avatarImage;
    [SerializeField] TMP_Text displayName;
    [SerializeField] TMP_InputField editName, editDesc;
    [SerializeField] Toggle[] interests;
    [SerializeField] UnityEvent<UserData> onUserUpdate;
    [SerializeField] Buga_ChangeAvatar changeAvatar;
    [SerializeField] List<Image> interestsImg;
    [SerializeField] InterestUIManager interestUIManager;
    
    List<string> _interests;

    int _mainInterest;
    public event Action<List<string>> OnInterestLoaded;
    bool _rpmChanged;
    
    int GetMainInterestIndex() => UserManager.Instance.CurrentUser.InterestsAsArray.IndexOf( UserManager.Instance.CurrentUser.main_interest );

    private void Awake()
    {
        _interests = UserManager.Instance.CurrentUser.InterestsAsArray;
    }

    public void UpdateView() {
        UserManager.Instance.CurrentUser.RequestAvatar( avatarImage );
        editName.text = displayName.text = UserManager.Instance.CurrentUser.name;
        editDesc.text = UserManager.Instance.CurrentUser.bio;
        _interests = UserManager.Instance.CurrentUser.InterestsAsArray;
        UpdateInterestIcons();
    }
    
    public void UpdateInterestIcons() 
    {
        for (var i = 0; i < _interests.Count; i++)
        {
            interestsImg[i].sprite = _profilePanel.FindInterestIcon( _interests[i] );
        }

        mainInterestIcon.sprite = _profilePanel.FindInterestIcon( UserManager.Instance.CurrentUser.main_interest );
        int interstIndex = Mathf.Max( GetMainInterestIndex(), 0 );
        interests[interstIndex].isOn = true;
    }
    
    public void SetMainInterest( int index ) => _mainInterest = index;
    public void LoadInterests() {
        Debug.Log("Loaded interests");
        _interests = UserManager.Instance.CurrentUser.InterestsAsArray;
        OnInterestLoaded?.Invoke( _interests );
    }
    
    public void RegisterInterest( string interest ) {
        var userInterests = _interests;
        if ( userInterests.Contains( interest ) ) return;
        for ( int i = 0; i < userInterests.Count; i++ ) {
            if ( string.IsNullOrEmpty( userInterests[i] ) ) {
                userInterests[i] = interest;
                _interests = userInterests;
                break;
            }
        }
    }
    
    public void RemoveInterest( string interest ) {
        var userInterests = _interests;
        if ( !userInterests.Contains( interest ) ) return;
        for ( int i = 0; i < userInterests.Count; i++ ) {
            if ( userInterests[i] == interest ) {
                userInterests[i] = null;
                _interests = userInterests;
                break;
            }
        }
    }
    
    public void SetAvatarURL( string url ) {
        UserManager.Instance.CurrentUser.avatar = url;
        UserManager.Instance.CurrentUser.RequestAvatar( avatarImage );
        changeAvatar.ChangeAvatar(url);
        _rpmChanged = true;
    }
    
    public void Confirm() {
        if ( _rpmChanged ) return;
        _rpmChanged = false;
        UserManager.Instance.CurrentUser.main_interest = _interests[_mainInterest];
        mainInterestIcon.sprite = _profilePanel.FindInterestIcon( _interests[_mainInterest] );
        UserManager.Instance.CurrentUser.name = editName.text;
        UserManager.Instance.CurrentUser.bio = editDesc.text;

        var editUser = new EditUser
        {
            name = UserManager.Instance.CurrentUser.name,
            bio = UserManager.Instance.CurrentUser.bio,
            interests = UserManager.Instance.CurrentUser.interests,
            avatar = UserManager.Instance.CurrentUser.avatar,
            main_interest = UserManager.Instance.CurrentUser.main_interest,
            onboarded = UserManager.Instance.CurrentUser.onboarded
        };
        
        
        if(UserManager.Instance.IsLoggedIn) UserAdminService.UpdateUser(editUser, UserManager.Instance.CurrentUser.id);
    }

    public void UpdateInterest()
    {
        interestUIManager.ConfirmInterests();
        _interests = UserManager.Instance.CurrentUser.InterestsAsArray;
        UpdateInterestIcons();
    }
    
    public void OpenInterests() => OnInterestLoaded?.Invoke( _interests );
}
