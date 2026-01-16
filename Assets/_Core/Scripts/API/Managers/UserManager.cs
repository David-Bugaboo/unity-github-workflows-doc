using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    private static UserManager _instance;
    public static UserManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<UserManager>();
            return _instance;
        }
    }
    
    public UserData CurrentUser;
    public string Token;
    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    private void Awake()
    {
        if (CurrentUser == null)
        {
            CurrentUser = new UserData();
        }
    }
    
    public void SetUserData(UserData data, string token)
    {
        CurrentUser = data;
        Token = token;
        Debug.Log($"Usuário '{data.name}' logado com sucesso!");
    }

    public void UpdateLocalUserData(UserData data)
    {
        CurrentUser = data;
    }
    
    public void SetAllInterests(List<string> interests)
    {
        if (CurrentUser == null) return;
        CurrentUser.interests = string.Join(",", interests);
    }

    public void ClearUserData()
    {
        CurrentUser = new UserData();
        Token = null;
        Debug.Log("Usuário deslogado.");
    }

    public void SetDisplayName(string name)
    {
        if (CurrentUser == null) return;
        CurrentUser.name = name;
    }

    public void SetDescription(string bio)
    {
        if (CurrentUser == null) return;
        CurrentUser.bio = bio;
    }

    public void SetAvatarURL(string url)
    {
        if (CurrentUser == null) return;
        CurrentUser.avatar = url;
    }

    public void RegisterInterest(string interest)
    {
        if (CurrentUser == null) return;
        var userInterests = CurrentUser.InterestsAsArray.ToList();
        if (userInterests.Contains(interest)) return;
        userInterests.Add(interest);
        CurrentUser.interests = string.Join(",", userInterests);
    }

    public void RemoveInterest(string interest)
    {
        if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.interests)) return;
        var userInterests = CurrentUser.InterestsAsArray.ToList();
        if (userInterests.Remove(interest))
        {
            CurrentUser.interests = string.Join(",", userInterests);
        }
    }
}