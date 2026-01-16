using System.Collections.Generic;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

public class AvatarLoaderService : MonoBehaviour
{
    private static AvatarLoaderService _instance;

    public static AvatarLoaderService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AvatarLoaderService>();
            }
            
            return _instance;
        }
    }

    // O cache e as filas agora são gerenciados por este serviço central
    private Dictionary<string, Texture2D> _cachedTextures = new();
    private Dictionary<string, Queue<RawImage>> _queuedRequests = new();
    private List<string> _currentLoads = new();
    
    /// <summary>
    /// Solicita o carregamento de um avatar de uma URL e o exibe em uma RawImage.
    /// </summary>
    /// <param name="url">A URL do avatar do Ready Player Me.</param>
    /// <param name="targetImage">O componente RawImage que exibirá o avatar.</param>
    public void RequestAvatar(string url, RawImage targetImage)
    {
        if (string.IsNullOrEmpty(url) || targetImage == null) return;

        // Se já temos a textura em cache, aplicamos imediatamente.
        if (_cachedTextures.TryGetValue(url, out Texture2D cachedTex))
        {
            targetImage.texture = cachedTex;
            return;
        }

        // Se já existe uma solicitação para esta URL, apenas adiciona a imagem na fila.
        if (_queuedRequests.ContainsKey(url))
        {
            _queuedRequests[url].Enqueue(targetImage);
            return;
        }

        // Inicia uma nova fila para esta URL
        _queuedRequests[url] = new Queue<RawImage>();
        _queuedRequests[url].Enqueue(targetImage);

        // Inicia o processo de carregamento
        LoadAvatarFromUrl(url);
    }

    private void LoadAvatarFromUrl(string url)
    {
        if (_currentLoads.Contains(url)) return;
        
        _currentLoads.Add(url);

        var avatarRenderer = new AvatarRenderLoader();
        avatarRenderer.OnCompleted = (texture) => OnLoadCompleted(url, texture);
        avatarRenderer.OnFailed = (type, message) => OnLoadFailed(url, message);
        
        var renderSettings = new AvatarRenderSettings
        {
            Camera = RenderCamera.FullBody,
            IsTransparent = true
        };
        
        avatarRenderer.LoadRender(url, renderSettings);
        Debug.Log($"[AvatarLoaderService] Iniciando download para: {url}");
    }

    private void OnLoadCompleted(string url, Texture2D texture)
    {
        Debug.Log($"[AvatarLoaderService] Download concluído para: {url}");
        _currentLoads.Remove(url);

        if (texture != null)
        {
            // Guarda a textura no cache
            _cachedTextures[url] = texture;

            // Atende a todas as solicitações na fila para esta URL
            if (_queuedRequests.TryGetValue(url, out Queue<RawImage> imageQueue))
            {
                while (imageQueue.Count > 0)
                {
                    RawImage image = imageQueue.Dequeue();
                    if(image != null) image.texture = texture;
                }
                _queuedRequests.Remove(url);
            }
        }
    }

    private void OnLoadFailed(string url, string message)
    {
        Debug.LogError($"[AvatarLoaderService] Falha ao carregar avatar de {url}. Erro: {message}");
        _currentLoads.Remove(url);
        _queuedRequests.Remove(url); // Limpa a fila de solicitações com falha
    }
}