using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class EventAdminService
{
    /// <summary>
    /// Busca todos os eventos da API.
    /// </summary>
    public static async Task<List<EventData>> GetAllEventsAsync()
    {
        var jsonResponse = await APIManager.Instance.Get(APIEndpointConfig.APIEndpointType.GetAllEvents);
        
        if (string.IsNullOrEmpty(jsonResponse))
            return new List<EventData>();
        
        var wrapper = JsonUtility.FromJson<JsonWrapper<EventData>>(jsonResponse);
        return wrapper.data;
    }

    /// <summary>
    /// Busca um evento da API.
    /// </summary>
    public static async Task<EventData> GetEventAsync(string eventId)
    {
        var jsonResponse = await APIManager.Instance.Get(APIEndpointConfig.APIEndpointType.GetEvent, eventId);
        if (string.IsNullOrEmpty(jsonResponse)) return null;
        return JsonUtility.FromJson<EventData>(jsonResponse);
    }

    /// <summary>
    /// Cria um novo evento.
    /// </summary>
    public static async Task<EventData> CreateEventAsync(EventData eventData)
    {
        var request = await APIManager.Instance.Post(APIEndpointConfig.APIEndpointType.CreateEvent, eventData);
        
        if (request?.result == UnityWebRequest.Result.Success)
        {
            return JsonUtility.FromJson<EventData>(request.downloadHandler.text);
        }
        return null;
    }

    /// <summary>
    /// Edita um evento existente.
    /// </summary>
    public static async Task<bool> EditEventAsync(string eventId, EventData eventData)
    {
        var request = await APIManager.Instance.Patch(APIEndpointConfig.APIEndpointType.EditEvent, eventData, eventId);
        return request?.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Deleta um evento.
    /// </summary>
    public static async Task<bool> DeleteEventAsync(string eventId)
    {
        var request = await APIManager.Instance.Delete(APIEndpointConfig.APIEndpointType.DeleteEvent, eventId);
        return request?.result == UnityWebRequest.Result.Success;
    }
    
    public static Texture2D Base64ToTexture(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            Debug.LogError("A string Base64 fornecida é nula ou vazia.");
            return null;
        }

        try
        {
            byte[] imageData = Convert.FromBase64String(base64String);
            
            Texture2D texture = new Texture2D(2, 2);
            
            if (texture.LoadImage(imageData))
            {
                return texture;
            }
            else
            {
                Debug.LogError("Falha ao carregar os dados da imagem na textura. O formato pode não ser um PNG ou JPG válido.");
                return null;
            }
        }
        catch (FormatException e)
        {
            Debug.LogError($"Erro ao converter a string Base64. A string pode estar malformada ou corrompida. Erro: {e.Message}");
            return null;
        }
    }
}