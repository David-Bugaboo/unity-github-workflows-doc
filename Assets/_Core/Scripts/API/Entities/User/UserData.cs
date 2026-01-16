using System;
using System.Collections.Generic;
using System.Linq;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UserData
{
    public string id;
    public string name;
    public string password;
    public string role;
    public string email;
    public string cpf;
    public string bio;
    public bool onboarded;
    public string interests;
    public string main_interest;
    public string avatar;
    public string created_at;
    public string updated_at;
    public string deleted_at;
    public List<EventData> events;
    public List<Session> sessions;
    
    private Dictionary<string, Texture2D> _cachedTextures = new Dictionary<string, Texture2D>();
    private List<string> _currentRequests = new List<string>();
    
    [NonSerialized] private Queue<RawImage> _queuedImages = new Queue<RawImage>();
    
    public bool IsAdmin => "ADMINISTRATOR".Equals(role, StringComparison.OrdinalIgnoreCase);

    public List<string> InterestsAsArray
    {
        get
        {
            if (string.IsNullOrEmpty(interests)) return new List<string>();
            return interests.Split(',').ToList();
        }
    }

    /// <summary>
    /// Ponto de entrada principal para carregar o avatar e exibi-lo em um RawImage.
    /// </summary>
    public void RequestAvatar(RawImage target)
    {
        if (string.IsNullOrEmpty(avatar) || target == null) return;
        
        if (_cachedTextures.TryGetValue(avatar, out Texture2D cachedTex))
        {
            target.texture = cachedTex;
            return;
        }
        
        if (_currentRequests.Contains(avatar))
        {
            _queuedImages.Enqueue(target);
            return;
        }
        
        _queuedImages.Enqueue(target);
        LoadAvatar(avatar);
    }

    /// <summary>
    /// Inicia o processo de carregamento do avatar usando a nova API do RPM.
    /// </summary>
    private void LoadAvatar(string url)
    {
        _currentRequests.Add(url);
        
        var renderSettings = new AvatarRenderSettings
        {
            Camera = RenderCamera.Portrait,
            Pose = RenderPose.Standing,
            IsTransparent = true
        };
        
        var avatarRenderer = new AvatarRenderLoader();
        avatarRenderer.OnCompleted = ReleaseAllRequest;
        
        avatarRenderer.OnFailed = (failureType, message) =>
        {
            Debug.LogError($"Falha ao carregar o avatar de '{url}': {message}");
            _currentRequests.Remove(url);
            while (_queuedImages.Count > 0)
                _queuedImages.Dequeue();
        };
        
        avatarRenderer.LoadRender(url, renderSettings);
        Debug.Log($"Nova requisição de avatar iniciada para a URL: {url}");
    }

    /// <summary>
    /// Chamado quando o carregamento do avatar é concluído com sucesso.
    /// </summary>
    private void ReleaseAllRequest(Texture2D tex)
    {
        _currentRequests.Remove(avatar);

        if (tex == null)
        {
            Debug.LogWarning("Requisição de avatar concluída, mas a textura retornada é nula.");
            return;
        }
        
        _cachedTextures[avatar] = tex;
        while (_queuedImages.Count > 0)
        {
            RawImage imageToUpdate = _queuedImages.Dequeue();
            if (imageToUpdate != null)
            {
                imageToUpdate.texture = tex;
            }
        }
        Debug.Log("Requisição de avatar concluída e imagens da fila atualizadas.");
    }

    /// <summary>
    /// Método estático para limpar o cache de avatares, útil ao trocar de cena ou de usuário.
    /// </summary>
    public void ClearAvatarCache()
    {
        _cachedTextures.Clear();
        Debug.Log("Cache de avatares foi limpo.");
    }
}

public interface IResponse {
    string NameToDisplay();
}
