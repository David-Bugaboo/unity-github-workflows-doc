using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private GameObject loadingScreen;
    
    [Header("Events")]
    [SerializeField] private UnityEvent OnLoginSuccess;
    [SerializeField] private UnityEvent OnLoginFailed;
    [SerializeField] private UnityEvent GoToOnboard;

    // Tornamos o método async void para ser chamado por eventos da UI (como o OnClick de um botão)
    public async void AttemptLogin()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        
        string email = emailField.text;
        string password = passwordField.text;
        
        // Chama o APIManager para fazer o login
        var loginResponse = await APIManager.Instance.Login(email, password);

        if (loadingScreen != null) loadingScreen.SetActive(false);

        if (loginResponse != null)
        {
            // Se o login deu certo, guarda os dados no UserManager
            UserManager.Instance.SetUserData(loginResponse, loginResponse.sessions[0].token);
            var response = await APIManager.Instance.Get(APIEndpointConfig.APIEndpointType.GetUser, loginResponse.id);
            var completeUser = JsonUtility.FromJson<UserData>(response);
            
            UserManager.Instance.SetUserData(completeUser, loginResponse.sessions[0].token);
            if (loginResponse.onboarded) OnLoginSuccess?.Invoke();
            else GoToOnboard?.Invoke();
            Debug.Log("Login bem-sucedido!");
        }
        else
        {
            // Se falhou, o APIManager já deve ter mostrado um popup de erro
            OnLoginFailed?.Invoke();
            ErrorHandler.ShowError("Client");
            Debug.LogError("Falha no login.");
        }
    }
}