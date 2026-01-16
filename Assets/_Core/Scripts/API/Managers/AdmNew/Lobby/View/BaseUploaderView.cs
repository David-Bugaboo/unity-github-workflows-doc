using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseUploaderView : MonoBehaviour
{
    [Header("Componentes Comuns da UI")]
    [SerializeField] protected ImageSlotView[] slots;
    [SerializeField] protected Button confirmButton;
    [SerializeField] private RawImage previewImage;

    private int currentSlot;
    
    private FileUploadService _fileUploadService;
    protected string[] _slotPaths;
    
    protected abstract string UploadType { get; }
    protected abstract string[] SlotKeys { get; }
    
    protected BaseUploaderView()
    {
        _slotPaths = new string[slots?.Length ?? 0]; 
    }

    protected virtual void Awake()
    {
        _fileUploadService = new FileUploadService(APIManager.Instance);
        if (confirmButton != null) confirmButton.interactable = false;
    }

    /// <summary>
    /// Chamado toda vez que o GameObject é ativado.
    /// </summary>
    protected virtual async void OnEnable()
    {
        await LoadExistingImages();
    }

    /// <summary>
    /// Busca as imagens da API e popula a view.
    /// </summary>
    public async System.Threading.Tasks.Task LoadExistingImages()
    {
        List<ImageInfo> existingImages = await _fileUploadService.GetImagesByType(UploadType);
        
        for (int i = 0; i < slots.Length; i++)
        {
            UpdateSlotImage(i, null, null);
        }

        if (existingImages != null)
        {
            foreach (var imageInfo in existingImages)
            {
                if (int.TryParse(imageInfo.name, out int orderIndex))
                {
                    int slotIndex = orderIndex - 1;
                    if (slotIndex >= 0 && slotIndex < slots.Length)
                    {
                        LoadImageIntoSlot(slotIndex, imageInfo);
                    }
                }
            }
        }
    }
    
    private async void LoadImageIntoSlot(int slotIndex, ImageInfo info)
    {
        if (string.IsNullOrEmpty(info.url)) return;
        Texture2D texture = await APIManager.Instance.GetTextureFromUrl(info.img);
        UpdateSlotImage(slotIndex, texture, info);
        slots[slotIndex].Setted = true;
    }

    public void OnLoadSlotClicked(int slotIndex)
    {
        var extensions = new[] { new ExtensionFilter("Imagem", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecione uma Imagem", "", extensions, false);

        if (paths.Length > 0)
        {
            string path = paths[0];
            _slotPaths[slotIndex] = path;
            
            byte[] fileBytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileBytes);
            
            UpdateSlotImage(slotIndex, texture, slots[slotIndex].SlotInfo);
            ValidateConfirmButton();

            currentSlot = slotIndex;
        }
    }
    
    public void OpenAllSlot(int index)
    {
        currentSlot = index;
        previewImage.texture = slots[index].displayImage.texture;
    }

    public void OnLoadAllSlot()
    {
        var extensions = new[] { new ExtensionFilter("Imagem", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecione uma Imagem", "", extensions, false);

        if (paths.Length > 0)
        {
            string path = paths[0];
            _slotPaths[currentSlot] = path;
            
            byte[] fileBytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(fileBytes);
            
            UpdateSlotImage(currentSlot, texture, slots[currentSlot].SlotInfo);
            ValidateConfirmButton();
        }
    }

    public void OnCancelSlotClicked(int slotIndex)
    {
        _slotPaths[slotIndex] = null;
        UpdateSlotImage(slotIndex, null, null);
        if (previewImage != null) previewImage.texture = null;
        ValidateConfirmButton();
    }
    
    public void OnCancelAllSlot()
    {
        _slotPaths[currentSlot] = null;
        UpdateSlotImage(currentSlot, null, null);
        if (previewImage != null) previewImage.texture = null;
        ValidateConfirmButton();
    }

    public async void OnConfirmUploadsClicked()
    {
        for (int i = 0; i < _slotPaths.Length; i++)
        {
            if (string.IsNullOrEmpty(_slotPaths[i])) continue;
            
            string urlParaEnvio = "";

            // Se esta view for do tipo PortfolioView, pega a URL do InputField
            if (this is PortfolioView portfolioView)
            {
                urlParaEnvio = portfolioView.GetCurrentUrl();
            }
            urlParaEnvio = string.IsNullOrEmpty(urlParaEnvio) ? "https://example.com.br" : urlParaEnvio;
            
            ImageInfo resultInfo = null;
            if (!slots[i].Setted)
            {
                Debug.Log($"Enviando slot {i} como NOVO (POST).");
                resultInfo = await _fileUploadService.CreateImageAsync(SlotKeys[i], _slotPaths[i], UploadType, urlParaEnvio);
            }
            else
            {
                Debug.Log($"Enviando slot {i} como ATUALIZAÇÃO (PATCH) para o ID {slots[i].SlotInfo.id}.");
                resultInfo = await _fileUploadService.UpdateImageAsync(slots[i].SlotInfo.id, SlotKeys[i], _slotPaths[i], UploadType, urlParaEnvio);
            }

            if (resultInfo != null && !string.IsNullOrEmpty(resultInfo.img))
            {
                _slotPaths[i] = null;
                LoadImageIntoSlot(i, resultInfo);
            }
            else
            {
                Debug.LogError($"Falha no upload do slot {i}.");
            }
        }
    }

    // --- Métodos de Controle da UI ---

    public void UpdateSlotImage(int slotIndex, Texture texture, ImageInfo info)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].UpdateDisplay(texture, info);
            if (previewImage != null) previewImage.texture = texture;
        }
    }

    private void ValidateConfirmButton()
    {
        bool anyImageLoaded = _slotPaths.Any(path => !string.IsNullOrEmpty(path));
        if (confirmButton != null) confirmButton.interactable = anyImageLoaded;
    }
}