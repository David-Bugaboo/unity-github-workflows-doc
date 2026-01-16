using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EventApiService
{
    private readonly APIManager _apiManager;

    public EventApiService(APIManager apiManager)
    {
        _apiManager = apiManager;
    }

    public async Task<List<EventData>> GetAllEventsAsync()
    {
        var jsonResponse = await _apiManager.Get(APIEndpointConfig.APIEndpointType.GetAllEvents);
        if (string.IsNullOrEmpty(jsonResponse)) return new List<EventData>();

        return JsonUtility.FromJson<EventsWrapper>(jsonResponse).data;
    }
    
    public async Task<EventData> GetEventDetailsAsync(string eventId)
    {
        var jsonResponse = await _apiManager.Get(APIEndpointConfig.APIEndpointType.GetEvent, eventId);
        if (string.IsNullOrEmpty(jsonResponse)) return null;
        return JsonUtility.FromJson<EventData>(jsonResponse);
    }

    public async Task<EventData> CreateEventAsync(EventPayload payload)
    {
        var request = await _apiManager.Post(APIEndpointConfig.APIEndpointType.CreateEvent, payload);
        if (request == null) return null;
        return JsonUtility.FromJson<EventData>(request.downloadHandler.text);
    }


    public async Task<bool> EditEventAsync(string eventId, EventPayload payload)
    {
        var request = await _apiManager.Patch(APIEndpointConfig.APIEndpointType.EditEvent, payload, eventId);
        return request != null;
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        var request = await _apiManager.Delete(APIEndpointConfig.APIEndpointType.DeleteEvent, eventId);
        return request != null;
    }

    public async Task<bool> AddMembersAsync(string eventId, List<string> userIds)
    {
        if (userIds == null || !userIds.Any()) return true;

        bool allSucceeded = true;
        Debug.Log($"Iniciando processo de convite para {userIds.Count} usuários...");

        // Loop para convidar um por vez.
        foreach (var userId in userIds)
        {
            bool success = await InviteUserToEventAsync(eventId, userId);
            if (!success)
            {
                allSucceeded = false;
                Debug.LogError($"Falha ao convidar o usuário com ID: {userId}");
                // Podemos continuar convidando os outros ou parar aqui. Continuar é mais robusto.
            }
        }
    
        return allSucceeded;
    }
    
    public async Task<bool> InviteUserToEventAsync(string eventId, string userId)
    {
        var payload = new GuestIdPayload { guest_id = userId };
        var request = await _apiManager.Post(APIEndpointConfig.APIEndpointType.InviteUser, payload, eventId);
        return request != null;
    }
    
    public async Task<bool> RemoveMembersAsync(string eventId, List<string> userIdsToRemove)
    {
        if (userIdsToRemove == null || !userIdsToRemove.Any()) return true; 
        
        var payload = new MembersPayload { user_ids = userIdsToRemove };
        var request = await _apiManager.Post(APIEndpointConfig.APIEndpointType.RemoveEventMembers, payload, eventId);
        return request != null;
    }
}

[Serializable]
public class MembersPayload
{
    public List<string> user_ids;
}

[Serializable]
public class GuestIdPayload
{
    public string guest_id;
}