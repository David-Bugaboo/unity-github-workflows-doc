using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameScene : MonoBehaviour
{
    public TextMeshProUGUI debug;
    private void Start()
    {
        debug.text = "começando";
        SceneManager.LoadScene(2, LoadSceneMode.Single);
        debug.text = "iniciando outra cena";
    }
}
