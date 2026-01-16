using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class FileUploadService
{
    private readonly APIManager _apiManager;

    public FileUploadService(APIManager apiManager)
    {
        _apiManager = apiManager;
    }
    
    /// <summary>
    /// Busca na API a lista de imagens existentes de um tipo específico.
    /// </summary>
    public async Task<List<ImageInfo>> GetImagesByType(string type)
    {
        var jsonResponse = await _apiManager.Get(APIEndpointConfig.APIEndpointType.GetFiles);
        
        if (string.IsNullOrEmpty(jsonResponse))
        {
            return new List<ImageInfo>();
        }
        
        string wrappedJson = "{\"items\":" + jsonResponse + "}";
        ImageInfoListWrapper wrapper = JsonUtility.FromJson<ImageInfoListWrapper>(wrappedJson);
        
        wrapper.items = wrapper.items.FindAll(i => i.type == type);
        
        return wrapper.items;
    }
    
    public async Task<List<ImageInfo>> GetAllImages()
    {
        var jsonResponse = await _apiManager.Get(APIEndpointConfig.APIEndpointType.GetFiles);
        
        if (string.IsNullOrEmpty(jsonResponse))
        {
            return new List<ImageInfo>();
        }
        
        string wrappedJson = "{\"items\":" + jsonResponse + "}";
        ImageInfoListWrapper wrapper = JsonUtility.FromJson<ImageInfoListWrapper>(wrappedJson);
        return wrapper.items;
    }

    public async Task<ImageInfo> CreateImageAsync(string key, string imagePath, string type, string url = "https://example.com.br")
    {
        var payload = BuildPayload(key, imagePath, type, url);
        if (payload == null) return null;
        
        var request = await _apiManager.Post(APIEndpointConfig.APIEndpointType.CreateFile, payload);
        if (request != null) return JsonUtility.FromJson<ImageInfo>(request.downloadHandler.text);
        return null;
    }
    
    public async Task<ImageInfo> UpdateImageAsync(string imageId, string key, string imagePath, string type, string url = "https://example.com.br")
    {
        var payload = BuildPayload(key, imagePath, type, url);
        if (payload == null) return null;
        
        var request = await _apiManager.Patch(APIEndpointConfig.APIEndpointType.UpdateFile, payload, imageId);
        if (request != null) return JsonUtility.FromJson<ImageInfo>(request.downloadHandler.text);
        return null;
    }
    
    private FileUploadPayload BuildPayload(string key, string imagePath, string type, string url)
    {
        if (!File.Exists(imagePath)) return null;
        
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        string mimeType = Path.GetExtension(imagePath).ToLower() == ".png" ? "image/png" : "image/jpeg";

        return new FileUploadPayload
        {
            name = key,
            url = url,
            type = type,
            img = new ImageDataPayload
            {
                mime = mimeType,
                data = Convert.ToBase64String(imageBytes)
            }
        };
    }
}