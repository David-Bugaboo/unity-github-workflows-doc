using UnityEngine;

[CreateAssetMenu(fileName = "APIEndpointConfig", menuName = "API/Endpoint Configuration")]
public class APIEndpointConfig : ScriptableObject
{
    [System.Serializable]
    public struct APIEndpoint
    {
        public APIEndpointType endpointType;
        public bool useBaseEndpoint;
        public bool useToken;
        public string path;
    }

    public enum APIEndpointType
    {
        Login, 
        Register,
        InviteUser,
        GetAllUsersAdmin,
        AddEventMember,
        SetUserRole,
        GetAllEvents,
        GetEvent,
        SetOnBoard,
        CreateEvent,
        EditEvent,
        DeleteEvent,
        AddEventMembers,
        RemoveEventMembers,
        GetEventMembers,
        GetUser,
        UpdateUser,
        DeleteUser,
        GrantAdminRole,
        CreateFile,
        UpdateFile,
        GetFiles
    }

    [Header("Base Configuration")]
    public string baseURL = "https://api.example.com/v1/";
    public int timeout = 10;

    [Header("Endpoints Configuration")]
    public APIEndpoint[] endpoints;
}
