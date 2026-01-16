using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class APIHandler : MonoBehaviour
{
    // --- Singleton e referências continuam iguais ---
    private static APIHandler _instance;
    public static APIHandler Instance
    {
        get {
            if (_instance == null) _instance = FindFirstObjectByType<APIHandler>();
            return _instance;
        }
    }

#if UNITY_EDITOR
    public SceneAsset TargetScene;
#endif
    [SerializeField] private string sceneName;
    [SerializeField] private float loadSceneDelay;

    [Header("UI References")]
    [SerializeField] private TMP_InputField displayName, desc;
    [SerializeField] private InterestUIManager interestUIManager; 
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private EnableDisableAnimation faderStart;

    [Header("Events")]
    [SerializeField] private UnityEvent<UserData> OnUserDataUpdated;
    [SerializeField] private UnityEvent<string> onAvatarLoaded;
    public UnityEvent onFinishedOnboardingEvent;
    public static event Action OnLogout;
    
    // A variável _editableUser foi REMOVIDA.

    private void Start()
    {
        UpdateUI();
    
        if (interestUIManager != null)
        {
            // O InterestUIManager é inicializado com os dados vindos diretamente do UserManager
            interestUIManager.InitializeWithManager();
        }
    }
    
    private void UpdateUI()
    {
        // Lê os dados direto do UserManager para atualizar a tela
        var currentUser = UserManager.Instance.CurrentUser;
        if (currentUser == null) return;

        if (displayName != null) displayName.text = currentUser.name;
        if (desc != null) desc.text = currentUser.bio;
    }

    #region Onboarding / Profile Update
    
    public async void FinishOnboarding()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        
        // A fonte dos dados é sempre o UserManager.Instance.CurrentUser
        var userToUpdate = UserManager.Instance.CurrentUser;
    
        var patchUser = new PatchRegistrationData
        {
            name = userToUpdate.name,
            bio = userToUpdate.bio,
            avatar = userToUpdate.avatar,
            main_interest = userToUpdate.InterestsAsArray.Count > 0 ? userToUpdate.InterestsAsArray[0] : "",
            interests = userToUpdate.interests,
            onboarded = true
        };

        if (!ValidateUser(patchUser))
        {
            ErrorHandler.ShowError("MissingDados");
            if (loadingScreen != null) loadingScreen.SetActive(false);
            faderStart.OnContentOff();
            return;
        }

 if (!string.IsNullOrEmpty(userToUpdate.id))
    {
        var request = await APIManager.Instance.Patch(APIEndpointConfig.APIEndpointType.SetOnBoard, patchUser, userToUpdate.id);

        if (loadingScreen != null) loadingScreen.SetActive(false);

        if (request != null)
        {
            var createdUser = JsonUtility.FromJson<UserData>(request.downloadHandler.text);
            UserManager.Instance.UpdateLocalUserData(createdUser);
            OnUserDataUpdated?.Invoke(createdUser);
            onFinishedOnboardingEvent?.Invoke();
        }
        else
        {
            ErrorHandler.ShowError("Server");
            faderStart.OnContentOff();
        }
    }
    else
    {
        if (loadingScreen != null) loadingScreen.SetActive(false);
        var createdUser = new UserData
        {
            name = patchUser.name,
            bio = patchUser.bio,
            avatar = patchUser.avatar,
            main_interest = patchUser.main_interest,
            interests = patchUser.interests,
            onboarded = true,
            role = "GHOST"
        };
        
        UserManager.Instance.UpdateLocalUserData(createdUser);
        OnUserDataUpdated?.Invoke(createdUser);
        onFinishedOnboardingEvent?.Invoke();
    }
}

    private bool ValidateUser(PatchRegistrationData user)
    {
        return !string.IsNullOrWhiteSpace(user.name) && !string.IsNullOrEmpty(user.avatar);
    }

    public void SetDisplayName() => UserManager.Instance.SetDisplayName(displayName.text);
    public void SetDescription() => UserManager.Instance.SetDescription(desc.text);
    public void SetAvatarURL(string url) => UserManager.Instance.SetAvatarURL(url);
    public void RegisterInterest(string interest) => UserManager.Instance.RegisterInterest(interest);
    public void RemoveInterest(string interest) => UserManager.Instance.RemoveInterest(interest);
    public void SetAllInterests(List<string> interests) => UserManager.Instance.SetAllInterests(interests);
    
    public void ConfirmAvatar() => onAvatarLoaded?.Invoke(UserManager.Instance.CurrentUser.avatar);

    #endregion

    // --- O resto do script permanece o mesmo ---
    public void PerformLogout()
    {
        UserManager.Instance.ClearUserData();
        OnLogout?.Invoke();
    }

    public void LoadNextScene()
    {
        StartCoroutine(DelayToLoadScene());
    }

    private IEnumerator DelayToLoadScene()
    {
        yield return new WaitForSeconds(loadSceneDelay);
        SceneManager.LoadScene(sceneName);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (TargetScene != null && TargetScene.name != sceneName)
            sceneName = TargetScene.name;
    }
#endif
}