using System;
[Serializable]
public class ChatMessageData {
    public string Message, SentAtTime;
    public SenderData Sender;
    [Serializable]
    public class SenderData {
        public string Name, AvatarUrl, Role;
        public static implicit operator string( SenderData data ) => data.Name;
    }
}