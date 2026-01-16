using Fusion;
using Fusion.Addons.Avatar;
using Fusion.XR.Shared.Rig;
using TMPro;
using UnityEngine;

/**
 * 
 * ScreenShareSetUsername is in charge of loading & saving the username in the screensharing UI
 * 
 **/
public class ScreenShareSetUsername : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    RigInfo rigInfo;
    UserInfo userInfo;
    NetworkRig networkRig;

    private TMP_InputField usernameInputFieldTMP;
    private string username;
    

    private void OnEnable()
    {
        username = PlayerPrefs.GetString(UserInfo.SETTINGS_USERNAME);

        if (!usernameInputFieldTMP)
            usernameInputFieldTMP = GetComponent<TMP_InputField>();

        if (username != null)
            usernameInputFieldTMP.text = username;       
    }

    private void FindUserInfo()
    {
        if (runner == null)
            Debug.LogError("Runner is not set");
        else
        {
            rigInfo =  RigInfo.FindRigInfo(runner);
            if (rigInfo)
                networkRig = rigInfo.localNetworkedRig;
            if (networkRig)
                userInfo = networkRig.GetComponentInChildren<UserInfo>();
            if (!userInfo)
                Debug.LogWarning("userInfo not found");
        }
    }

    public void SetScreenShareUsername()
    {
        if (!userInfo) 
                FindUserInfo();
        else
        {
            userInfo.UserName = usernameInputFieldTMP.text;
            SaveUsername();
        }
    }

    
    public void SaveUsername()
    {
        PlayerPrefs.SetString(UserInfo.SETTINGS_USERNAME, usernameInputFieldTMP.text);
    }
}
