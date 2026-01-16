using TMPro;
using UnityEngine;

public class ChatInputUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private string _currentChannel;

    private void Start()
    {
        messageInputField.onSubmit.AddListener((text) => OnClickSend());
    }
    
    public void SetCurrentChannel(string channelName)
    {
        _currentChannel = channelName;
    }
    
    private void OnClickSend()
    {
        Debug.Log("Vai enviar");
        if (string.IsNullOrEmpty(_currentChannel) || string.IsNullOrWhiteSpace(messageInputField.text))
        {
            return;
        }
        
        ChatService.Instance.SendMessage(_currentChannel, messageInputField.text);
        
        messageInputField.text = "";
        messageInputField.ActivateInputField(); 
    }
}