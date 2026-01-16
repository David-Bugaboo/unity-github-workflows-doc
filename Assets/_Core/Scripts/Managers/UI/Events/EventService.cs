using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class EventService
{
    private static List<EventData> _eventCache;
    private static bool _isCacheDirty = true;
    
    private const float CACHE_DURATION = 10f; 
    private static float _lastFetchTime = 0f;
    
    public static async Task<List<EventData>> GetAllEventsAsync(bool forceRefresh = false)
    {
        bool isCacheExpired = (Time.realtimeSinceStartup - _lastFetchTime) > CACHE_DURATION;
        
        if (!forceRefresh && !_isCacheDirty && !isCacheExpired && _eventCache != null)
        {
            return _eventCache;
        }
        
        var jsonResponse = await APIManager.Instance.Get(APIEndpointConfig.APIEndpointType.GetAllEvents);

        if (string.IsNullOrEmpty(jsonResponse))
        {
            return new List<EventData>();
        }
        
        var wrapper = JsonUtility.FromJson<JsonWrapper<EventData>>(jsonResponse);
        _eventCache = wrapper.data;
        
        _isCacheDirty = false;
        _lastFetchTime = Time.realtimeSinceStartup;

        return _eventCache;
    }
    
    public static void InvalidateCache()
    {
        _isCacheDirty = true;
        _lastFetchTime = 0f;
    }
}

[System.Serializable]
public class JsonWrapper<T>
{
    public List<T> data;
}