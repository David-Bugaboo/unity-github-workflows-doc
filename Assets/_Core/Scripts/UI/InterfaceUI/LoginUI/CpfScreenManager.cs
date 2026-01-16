using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CpfScreenManager : MonoBehaviour
{
    [SerializeField] private CpfInputField cpfInputField;
    [SerializeField] private EmailInputField emailInputField;
    [SerializeField] private TMP_InputField passInputField;
    [SerializeField] private Button submitButton;

    [SerializeField] private UnityEvent OnLoginSuccess;
    [SerializeField] private UnityEvent OnLoginFailed;
    [SerializeField] private UnityEvent GoToOnboard;
    
    [SerializeField] private GameObject loadingScreen;

    public void CheckIsOk()
    {
        if((!cpfInputField.IsCpfValid() || !emailInputField.IsEmailValid()) && !submitButton.interactable) return;
        submitButton.interactable = cpfInputField.IsCpfValid() && emailInputField.IsEmailValid();
    }

    public async void AttemptLogin()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);

        string email = cpfInputField.GetComponent<TMP_InputField>().text;
        string password = passInputField.text;
        
        // Chama o APIManager para fazer o login
        var loginResponse = await APIManager.Instance.Login(email, password);

        if (loadingScreen != null) loadingScreen.SetActive(false);

        if (loginResponse != null)
        {
            UserManager.Instance.SetUserData(loginResponse, loginResponse.sessions[0].token);

            if (loginResponse.onboarded) OnLoginSuccess?.Invoke();
            else GoToOnboard?.Invoke();
            
            Debug.Log("Login bem-sucedido!");
        }
        else
        {
            OnLoginFailed?.Invoke();
            Debug.LogError("Falha no login.");
        }
    }
}
