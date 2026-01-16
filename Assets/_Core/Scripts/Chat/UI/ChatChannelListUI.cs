using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ChatChannelListUI : NewUIView<ChatGroupData>
{
    public ChatMessagePanelUI messagePanel;
    public UnityEvent OnOpenChat;
    public UnityEvent OnCloseChat;

    private bool abrindo;

    private void Start()
    {
        ChatService.Instance.OnChannelSubscribed += HandleChannelSubscribed;
        ChatService.Instance.OnChannelStateUpdated += HandleChannelStateUpdated;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ChatService.Instance != null)
        {
            ChatService.Instance.OnChannelSubscribed -= HandleChannelSubscribed;
            ChatService.Instance.OnChannelStateUpdated -= HandleChannelStateUpdated;
        }
    }

    private void HandleChannelSubscribed(ChatGroupData channelData)
    {
        Add(channelData);
    }
    
    public void SelectChannel(string channelName)
    {
        if (messagePanel != null)
        {
            messagePanel.gameObject.SetActive(true);
            OnOpenChat?.Invoke();
            messagePanel.ShowChannel(channelName);
            abrindo = true;
        }
    }
    
    private void HandleChannelStateUpdated(ChatGroupData updatedData)
    {
        // CORREÇÃO: Use 'cards' em vez de 'cardPool' para corresponder à sua classe base.
        var cardToUpdate = cardPool.FirstOrDefault(card => card.Data != null && card.Data.Name == updatedData.Name);
        if (cardToUpdate != null)
        {
            // Apenas atualiza os dados. O OnDataSet do card fará o resto.
            cardToUpdate.Data = updatedData;
        }
    }

    public void ExitChat()
    {
        if (abrindo)
        {
            OnCloseChat?.Invoke();   
        }

        abrindo = false;
    }
}