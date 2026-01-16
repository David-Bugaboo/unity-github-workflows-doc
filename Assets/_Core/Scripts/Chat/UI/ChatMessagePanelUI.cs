using System;
using UnityEngine;

public class ChatMessagePanelUI : NewUIView<ChatMessageData>
{
    [SerializeField] private GameObject panelRoot; // O objeto pai do painel
    // [SerializeField] private TMPro.TMP_Text channelNameText;
    
    // Adicione a referência ao seu script de input aqui
    [SerializeField] private ChatInputUI inputControls; 

    private string _currentChannel;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("rodou o awake");
    }

    private void Start()
    {
        ChatService.Instance.OnMessageReceivedForHistory += HandleMessageReceived;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ChatService.Instance != null)
        {
            // CORREÇÃO: O nome do evento foi atualizado aqui também.
            ChatService.Instance.OnMessageReceivedForHistory -= HandleMessageReceived;
        }
    }

    // Mostra o painel e carrega o histórico de mensagens
    public void ShowChannel(string channelName)
    {
        _currentChannel = channelName;
        // channelNameText.text = channelName;
        
        // Carrega o histórico de mensagens do canal
        LoadData(ChatService.Instance.GetChannelHistory(channelName).ToArray());
        
        // Informa ao serviço qual canal está ativo para zerar as não lidas
        ChatService.Instance.SetActiveChannel(channelName);

        // Informa ao controle de input qual é o canal atual
        if (inputControls != null)
        {
            inputControls.SetCurrentChannel(channelName);
        }
        
        panelRoot.SetActive(true);
    }
    
    public void CloseChannelView()
    {
        ChatService.Instance.SetActiveChannel(null);
        _currentChannel = null;
    }
    
    private void HandleMessageReceived(string channelName, ChatMessageData message)
    {
        // Se a mensagem for do canal que estamos vendo, adiciona na UI
        if (channelName == _currentChannel)
        {
            Add(message);
        }
        else
        {
            // A notificação de "nova mensagem" já é gerenciada pelo
            // ChatChannelListUI ao ouvir o evento OnChannelStateUpdated.
        }
    }
}