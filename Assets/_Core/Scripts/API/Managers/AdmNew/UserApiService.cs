using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserApiService
{
    private readonly APIManager _apiManager;

    public UserApiService(APIManager apiManager)
    {
        _apiManager = apiManager;
    }

    public async Task<List<UserData>> GetAllUsersAsync()
    {
        // NOTA: Adicione 'GetAllUsers' ao seu enum
        var jsonResponse = await _apiManager.Get(APIEndpointConfig.APIEndpointType.GetAllUsersAdmin);
        if (string.IsNullOrEmpty(jsonResponse)) return new List<UserData>();
        return JsonUtility.FromJson<UsersWrapper>(jsonResponse).data;
    }
    
    public async Task<bool> InviteUserToAppAsync(string email)
    {
        // NOTA: Adicione 'InviteUser' ao seu enum
        var payload = new { email = email }; // Payload anônimo
        var request = await _apiManager.Post(APIEndpointConfig.APIEndpointType.InviteUser, payload);
        return request != null;
    }

    public async Task<bool> GrantUserAdminRoleAsync(string playfabId, string role)
    {
        // NOTA: Adicione 'GrantAdminRole' ao seu enum. ex: /users/{}/role
        var payload = new { role = role };
        var request = await _apiManager.Patch(APIEndpointConfig.APIEndpointType.GrantAdminRole, payload, playfabId);
        return request != null;
    }
}