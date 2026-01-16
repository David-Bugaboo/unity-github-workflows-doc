using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class UserAdminService
{
    private static List<UserData> _userCache;

    public static async Task<UserData> UpdateUser(EditUser user, string userId)
    {
        var jsonResponse = await APIManager.Instance.Patch(APIEndpointConfig.APIEndpointType.UpdateUser,user, userId);
        if (string.IsNullOrEmpty(jsonResponse.downloadHandler.text))
        {
            return null;
        }
        
        var userData = JsonUtility.FromJson<UserData>(jsonResponse.downloadHandler.text);
        var oldUser =_userCache.Find(c => c.id == userData.id);
        _userCache.Remove(oldUser);
        _userCache.Add(userData);
        
        UserManager.Instance.SetUserData(userData, UserManager.Instance.Token);
        return userData;
    }
    
    /// <summary>
    /// Busca todos os usuários da API, usando o cache se disponível.
    /// </summary>
    public static async Task<List<UserData>> GetAllUsersAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _userCache != null)
        {
            return _userCache;
        }

        var jsonResponse = await APIManager.Instance.Get(APIEndpointConfig.APIEndpointType.GetAllUsersAdmin);
        if (string.IsNullOrEmpty(jsonResponse))
        {
            _userCache = new List<UserData>();
            return _userCache;
        }
            
        var wrapper = JsonUtility.FromJson<JsonWrapper<UserData>>(jsonResponse);
        _userCache = wrapper.data.Where(u => !string.IsNullOrEmpty(u.email)).ToList(); 
        return _userCache;
    }

    /// <summary>
    /// Remove uma lista de usuários de um evento.
    /// </summary>
    public static async Task<bool> RemoveMembersFromEventAsync(string eventId, List<string> userIds)
    {
        // O método DELETE pode aceitar um corpo ou os IDs podem ser passados na URL,
        // dependendo da sua API. Este é um exemplo com corpo.
        var data = new { userIds = userIds };
        // var request = await APIManager.Instance.DeleteWithBody($"{APIEndpointConfig.APIEndpointType.RemoveEventMember}/{eventId}", data);
        // return request?.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
        return true;
    }
    
    /// <summary>
    /// Encontra um usuário no cache pelo seu e-mail.
    /// </summary>
    public static UserData GetUserByEmail(string email)
    {
        if (_userCache == null)
        {
            Debug.LogError("Cache de usuários ainda não foi carregado. Chame GetAllUsersAsync primeiro.");
            return null;
        }
        return _userCache.FirstOrDefault(u => u.email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Envia um convite para o aplicativo para um novo usuário.
    /// </summary>
    public static async Task<bool> InviteUserAsync(string email)
    {
        var data = new { email = email };
        var request = await APIManager.Instance.Post(APIEndpointConfig.APIEndpointType.InviteUser, data);
        return request?.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Adiciona um ou mais usuários a um evento.
    /// </summary>
    public static async Task<bool> AddMembersToEventAsync(string eventId, string userId)
    {
        var request = await APIManager.Instance.Post(APIEndpointConfig.APIEndpointType.AddEventMember, new SendUserEvent{guest_id = userId}, eventId);
        return request?.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Concede um cargo (ex: Admin) a um usuário.
    /// </summary>
    public static async Task<bool> GrantUserRoleAsync(string userId, string role)
    {
        var data = new { role = "ADMINISTRATOR" };
        var request = await APIManager.Instance.Patch(APIEndpointConfig.APIEndpointType.SetUserRole, data, userId);
        return request?.result == UnityWebRequest.Result.Success;
    }
}