using System;
using System.Threading.Tasks;

public class AdminApiService
{
    private readonly APIManager _apiManager;

    public AdminApiService(APIManager apiManager)
    {
        _apiManager = apiManager;
    }
    
    public async Task<bool> SetUserRoleAsync(string userId, string role)
    {
        var payload = new RolePayload { role = role };
        var request = await _apiManager.Patch(APIEndpointConfig.APIEndpointType.SetUserRole, payload, userId);

        return request != null;
    }
}

[Serializable]
public class RolePayload
{
    public string role;
}