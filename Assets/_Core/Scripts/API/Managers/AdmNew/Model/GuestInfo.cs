using System;
using UnityEngine;

[Serializable]
public class GuestInfo
{
    public string id;
    public string invited_at;
    public string accepted_at;
    public string guest_id;
    public string event_id;
    
    public UserData guest;
}