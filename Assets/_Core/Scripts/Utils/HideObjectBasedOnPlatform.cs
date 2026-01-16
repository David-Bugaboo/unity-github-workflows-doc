using UnityEngine;

[System.Serializable]
public enum HideOnPlatform
{
    Mobile, Desktop
}
public class HideObjectBasedOnPlatform : MonoBehaviour
{

    public GameObject objectToHide;
    public bool HideOnMobile = true;
    public HideOnPlatform hideOn;

    private void Start()
    {
        // Verificar a plataforma atual
#if UNITY_STANDALONE || UNITY_EDITOR
        // Plataforma desktop (Windows, macOS, Linux) ou Editor
        objectToHide.SetActive(hideOn != HideOnPlatform.Desktop);  // Exibir o objeto
#else
            // Outra plataforma (iOS, Android, etc.)
            objectToHide.SetActive(hideOn != HideOnPlatform.Mobile); // Ocultar o objeto
#endif
    }
}