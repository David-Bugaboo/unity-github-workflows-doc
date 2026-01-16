using System;

[Serializable]
public class Session
{
    public string id;
    public string user_id;
    public string created_at;
    public string expires_at;
    public string token;
    public string status;
}