using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Chat;
using UnityEngine;

public class ChatService : MonoBehaviour, IChatClientListener
{
    private static ChatService instance;

    public static ChatService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ChatService>();
            }
            
            return instance;
        }
    }

    private ChatClient _chatClient;
    public string CurrentUserId { get; private set; }
    private string _activeChannel;
    
    private Dictionary<string, ChatGroupData> _channelStates = new Dictionary<string, ChatGroupData>();
    
    private Dictionary<string, List<ChatMessageData>> _channelHistory = new Dictionary<string, List<ChatMessageData>>();
    
    public event Action OnConnectedToChat;
    public event Action<ChatGroupData> OnChannelSubscribed;
    public event Action<ChatGroupData> OnChannelStateUpdated;

    public event Action<string, ChatMessageData> OnMessageReceivedForHistory;
    
    private void OnDestroy()
    {
        _chatClient?.Disconnect();
        Debug.LogError("Destruiu o Chat service");
    }

    private void Update()
    {
        _chatClient?.Service();
    }

    public void Connect(string userId, ChatAppSettings settings)
    {
        CurrentUserId = userId;
        _chatClient = new ChatClient(this);
        _chatClient.ConnectUsingSettings(settings);
    }
    
    public void Subscribe(string[] channels) => _chatClient.Subscribe(channels);

    public void SetActiveChannel(string channelName)
    {
        _activeChannel = channelName;
        if(string.IsNullOrEmpty(channelName)) return;
        if (_channelStates.TryGetValue(channelName, out var state))
        {
            state.UnreadCount = 0;
            OnChannelStateUpdated?.Invoke(state);
        }
    }
    
    public void SendMessage(string channelName, string messageText)
    {
        var messageData = new ChatMessageData {
            Message = messageText,
            SentAtTime = DateTime.Now.ToString("HH:mm"),
            Sender = new ChatMessageData.SenderData { Name = CurrentUserId }
        };

        // --- NOVO LOG DE DIAGNÓSTICO ---
        if (_chatClient.TryGetChannel(channelName, out var channel))
        {
            // Este log nos dirá quantos usuários o Photon acha que estão no canal no momento do envio.
            Debug.Log($"[{CurrentUserId}] enviando para o canal '{channelName}' que tem {channel.Subscribers.Count} inscritos (visão do remetente).");
        }
        else
        {
            Debug.LogWarning($"[{CurrentUserId}] tentou enviar mensagem para o canal '{channelName}', mas o cliente não encontrou o canal localmente.");
        }
        
        // Envio para o servidor
        _chatClient.PublishMessage(channelName, JsonUtility.ToJson(messageData));
    }

    // --- CORREÇÃO 1 (continuação): Busca no dicionário correto ---
    public List<ChatMessageData> GetChannelHistory(string channelName)
    {
        if (_channelHistory.TryGetValue(channelName, out var history))
        {
            return history;
        }
        return new List<ChatMessageData>();
    }

    public void OnConnected()
    {
        Debug.Log($"[{CurrentUserId}] CONECTADO AO CHAT! Estado do cliente: {_chatClient.State}");
        _chatClient.SetOnlineStatus(ChatUserStatus.Online);
        OnConnectedToChat?.Invoke();
    }
    
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            var channelName = channels[i];
            if (results[i])
            {
                Debug.Log($"[{CurrentUserId}] se inscreveu com SUCESSO no canal: {channelName}");
                
                if (!_channelStates.ContainsKey(channelName))
                {
                    var newState = new ChatGroupData { Name = channelName, LastMessage = "Nenhuma mensagem ainda." };
                    _channelStates.Add(channelName, newState);
                    OnChannelSubscribed?.Invoke(newState);
                }
                if (!_channelHistory.ContainsKey(channelName))
                {
                    _channelHistory.Add(channelName, new List<ChatMessageData>());
                }
            }
            else
            {
                Debug.LogError($"[{CurrentUserId}] FALHOU ao se inscrever no canal: {channelName}");
            }
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        Debug.Log($"[{CurrentUserId}] RECEBEU {messages.Length} mensagem(ns) no canal '{channelName}' de '{string.Join(", ", senders)}'");
        
        if (!_channelStates.TryGetValue(channelName, out var state)) return;

        // --- CORREÇÃO 2 (continuação): Processa TODAS as mensagens recebidas ---
        if (messages.Length > 0)
        {
            // Pega a última mensagem para atualizar o resumo
            var lastMessageJson = (string)messages.Last();
            var lastMessageData = JsonUtility.FromJson<ChatMessageData>(lastMessageJson);

            state.LastMessage = lastMessageData.Message;
            state.LastMessageTimestamp = lastMessageData.SentAtTime;

            if (channelName != _activeChannel)
            {
                state.UnreadCount += messages.Length;
            }
            OnChannelStateUpdated?.Invoke(state);
        }
        
        // Adiciona todas as mensagens ao histórico e notifica a UI
        foreach (var msg in messages)
        {
            var messageData = JsonUtility.FromJson<ChatMessageData>((string)msg);
            if (_channelHistory.TryGetValue(channelName, out var history))
            {
                history.Add(messageData);
            }
            // Dispara o evento para a UI de mensagens (se ela estiver ouvindo)
            Debug.Log("mensagem enviada" + messageData.Message + "no" + channelName);
            OnMessageReceivedForHistory?.Invoke(channelName, messageData);
        }
    }

    public void OnDisconnected()
    {
        Debug.LogError($"[{CurrentUserId}] FOI DESCONECTADO DO CHAT.");
    }
    
    // Outros métodos da interface (podem continuar vazios)
    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message) {}
    public void OnChatStateChange(ChatState state) {}
    public void OnUnsubscribed(string[] channels) {}
    public void OnPrivateMessage(string sender, object message, string channelName) {}
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) {}
    public void OnUserSubscribed(string channel, string user) {}
    public void OnUserUnsubscribed(string channel, string user) {}
}