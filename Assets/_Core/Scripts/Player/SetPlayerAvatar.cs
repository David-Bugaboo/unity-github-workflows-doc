using Fusion.Addons.Avatar;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetPlayerAvatar : MonoBehaviour
{
    private void Start()
    {
        string meuAvatarPadraoURL = "simpleavatar://?hairMesh=2&skinMat=0&clothMat=0&hairMat=0&clothMesh=0"; 
        
        Debug.Log("Definindo avatar padrão: " + meuAvatarPadraoURL);
        PlayerPrefs.SetString(UserInfo.SETTINGS_AVATARURL, meuAvatarPadraoURL);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }
}
