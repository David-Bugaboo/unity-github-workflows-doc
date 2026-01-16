using UnityEngine;

public class CloseGame : MonoBehaviour
{
    // Método público chamado para fechar a aplicação
    public void CloseApp()
    {
        // Fecha a aplicação
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
