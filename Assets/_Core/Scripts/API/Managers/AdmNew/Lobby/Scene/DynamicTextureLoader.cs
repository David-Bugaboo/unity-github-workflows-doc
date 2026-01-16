using UnityEngine;
using UnityEngine.UI;

public class DynamicTextureLoader : MonoBehaviour
{
    public enum TargetType { RawImage, MeshRenderer }

    [Header("Identificação (Configurado pelo Manager)")] 
    public string ImageIdentifier;
    public string ImageType;

    [Header("Configuração de Exibição")]
    [SerializeField] private TargetType target = TargetType.RawImage;
    [SerializeField] private Texture defaultTexture;
    [SerializeField] private bool deactivateOnFailure = false;

    [Header("Configuração Avançada de Material (para 3D)")]
    [Tooltip("Índice do material no array de materiais do MeshRenderer. Deixe -1 para usar o material principal.")]
    public int materialIndex;
    [Tooltip("Nome da propriedade da textura no shader (ex: _MainTex, _BaseMap, _EmissionMap).")]
    public string texturePropertyName;

    [Header("Atualização Automática")]
    [Tooltip("Intervalo em segundos para recarregar a imagem. Deixe 0 para não recarregar.")]
    [SerializeField] private float refreshInterval = 30;
    
    private RawImage _rawImage;
    private MeshRenderer _meshRenderer;
    private string _currentUrl;
    private string url;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        if (target == TargetType.RawImage)
        {
            if (_meshRenderer != null) _meshRenderer.enabled = false;
        }
        else
        {
            if (_rawImage != null) _rawImage.enabled = false;
        }
        
        ApplyTexture(defaultTexture);
    }
    
    public async void Initialize(string imageUrl, string urlClick)
    {
        if(_currentUrl == imageUrl || string.IsNullOrEmpty(imageUrl)) return;
        
        url = urlClick;
        _currentUrl = imageUrl;
        ApplyTexture(defaultTexture);
        
        Texture2D downloadedTexture = await APIManager.Instance.GetTextureFromUrl(imageUrl);
        if (downloadedTexture == null && deactivateOnFailure)
        {
            gameObject.SetActive(false);
        }
        
        ApplyTexture(downloadedTexture);
    }
    
    private void ApplyTexture(Texture texture)
    {
        Texture textureToApply = texture ?? defaultTexture;

        switch (target)
        {
            case TargetType.RawImage:
                if (_rawImage != null) _rawImage.texture = textureToApply;
                break;

            case TargetType.MeshRenderer:
                if (_meshRenderer != null)
                {
                    Material targetMaterial = (materialIndex < 0) ? _meshRenderer.material : _meshRenderer.materials[materialIndex];
                    if (targetMaterial != null)
                    {
                        targetMaterial.SetTexture(texturePropertyName, textureToApply);
                    }
                }
                break;
        }
    }

    public void OpenLink()
    {
        if(string.IsNullOrEmpty(url)) return;
        Application.OpenURL(url);
    }

    public void ConfigureForMeshRenderer(int matIndex, string texPropertyName)
    {
        target = TargetType.MeshRenderer;
        materialIndex = matIndex;
        texturePropertyName = texPropertyName;

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_rawImage != null) _rawImage.enabled = false;
        if (_meshRenderer != null) _meshRenderer.enabled = true;
    }
}