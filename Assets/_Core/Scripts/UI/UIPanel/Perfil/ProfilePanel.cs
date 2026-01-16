using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    const string defaultDescription = "Sem descri\u00e7\u00e3o...";

    [SerializeField] UserData DebugUser;
    [SerializeField] TMP_Text userName, userDesc, userMail, userRole, userRegister, userGroup;
    [SerializeField] RawImage playerAvatar;
    [SerializeField] Image[] interestsImg;
    [SerializeField] List<InterestsData> InterestIconData;
    
    public void UpdateView()
    {
        var User = UserManager.Instance.CurrentUser;
        DebugUser = User;
        userName.text = User.name;
        userDesc.text = string.IsNullOrEmpty( User.bio.Trim() ) ? defaultDescription : User.bio;
        var interests = User.InterestsAsArray;
        for(int i = 0; i < interests.Count; i++)
        {
            var icon = FindInterestIcon(interests[i]);
            interestsImg[i].sprite = icon;
        }
        
        User.RequestAvatar( playerAvatar );
    }
    
    public Sprite FindInterestIcon( string target ) {
        if ( string.IsNullOrEmpty( target ) ) return null;
        foreach ( var data in InterestIconData ) 
            if ( data.name == target ) return data.Icon;
        return null;
    }
}

[Serializable]
public struct InterestsData {
    public string name;
    public Sprite Icon;
}
