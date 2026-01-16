using Photon.Chat;
using System;

[System.Serializable]
public class ChatGroupData 
{ 
    public string Name; 
    public string LastMessage;
    public string LastMessageTimestamp;
    public int UnreadCount;
}