using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatChannelCard : UI_Card<ChatGroupData> 
{
    [Header("UI References")]
    [SerializeField] private TMP_Text channelNameText;
    [SerializeField] private TMP_Text lastMessageText;
    [SerializeField] private TMP_Text timestampText;
    [SerializeField] private GameObject unreadBadge;
    [SerializeField] private TMP_Text unreadCountText;
    [SerializeField] private Button selectButton;
    
    private ChatChannelListUI _ownerList;

    private void Awake()
    {
        _ownerList = GetComponentInParent<ChatChannelListUI>();
        selectButton.onClick.AddListener(OnSelect);
    }

    protected override void OnDataSet()
    {
        if (Data == null) return;
        
        channelNameText.text = Data.Name;
        lastMessageText.text = Data.LastMessage;
        timestampText.text = Data.LastMessageTimestamp;
        
        bool hasUnread = Data.UnreadCount > 0;
        unreadBadge.SetActive(hasUnread);
        if (hasUnread)
        {
            unreadCountText.text = Data.UnreadCount.ToString();
        }
    }

    private void OnSelect()
    {
        if (_ownerList != null && Data != null)
        {
            _ownerList.SelectChannel(Data.Name);
        }
    }
}