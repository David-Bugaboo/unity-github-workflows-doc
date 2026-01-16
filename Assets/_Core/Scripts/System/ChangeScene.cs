using Fusion;
using Fusion.Samples.IndustriesComponents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string sceneName;
    public float delay = 2f;
    public UnityEvent OnChangeScene;

    public void ChangeToScene()
    {
        if(sceneName == SceneManager.GetActiveScene().name) return;
        OnChangeScene?.Invoke();
        Invoke("LoadSceneWithDelay", delay);
    }

    private async void LoadSceneWithDelay()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();
        if (runner != null)
        {
            SessionManager.Instance.SetIntentionalShutdownFlag();
            FindFirstObjectByType<ApplicationManager>().isQuitting = true;
            await runner.Shutdown();
            DestroyImmediate(runner.gameObject);
            if(sceneName == "Login") UserManager.Instance.ClearUserData();
        }
        SceneManager.LoadScene(sceneName);
    }
}
