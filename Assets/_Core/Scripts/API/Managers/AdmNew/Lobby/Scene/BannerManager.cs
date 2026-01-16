using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BannerManager : MonoBehaviour
{
    [SerializeField] private List<DynamicTextureLoader> bannerLoaders;
    private FileUploadService _fileUploadService;

    private void Awake()
    {
        _fileUploadService = new FileUploadService(APIManager.Instance);
    }

    private void Start()
    {
        bannerLoaders = FindObjectsByType<DynamicTextureLoader>(FindObjectsSortMode.None).ToList();
        if (bannerLoaders == null || bannerLoaders.Count == 0)
        {
            Debug.LogError($"Nenhum banner load encontrado.");
            return;
        }

        StartCoroutine(RefreshBanners());
    }

    public IEnumerator RefreshBanners()
    {
        while (true)
        {
            InitializeBanners();   
            yield return new WaitForSeconds(30);
        }
    }

    public async void InitializeBanners()
    {
        List<ImageInfo> allImages = await _fileUploadService.GetAllImages();
        if (allImages == null || !allImages.Any())
        {
            Debug.LogError($"Nenhuma imagem foi encontrada na API.");
            return;
        }
        
        Debug.Log($"{allImages.Count} imagens encontradas. Distribuindo para os loaders...");
        
        foreach (var loader in bannerLoaders)
        {
            if (loader.ImageType != "BANNER")
            {
                loader.ImageType = "PORTFOLIO";
                loader.ConfigureForMeshRenderer(2, "_BaseMap");
            }
            
            if (loader == null || string.IsNullOrEmpty(loader.ImageIdentifier))
            {
                continue;
            }
            
            var targetImageInfo = allImages.FirstOrDefault(img => img.name == loader.ImageIdentifier && img.type == loader.ImageType);
            if (targetImageInfo != null && !string.IsNullOrEmpty(targetImageInfo.img))
            {
                loader.Initialize(targetImageInfo.img, targetImageInfo.url);
            }
            else
            {
                Debug.LogWarning($"Não foi encontrada na API uma imagem com o identificador '{loader.ImageIdentifier}'.");
            }
        }
    }
}