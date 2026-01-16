using UnityEngine;
using UnityEngine.UI;

public class ImageSlotView : MonoBehaviour
{
    [Header("Referências de UI do Slot")]
    [Tooltip("A RawImage que vai exibir a imagem carregada ou a padrão.")]
    [SerializeField] public RawImage displayImage;

    [Tooltip("O botão que o usuário clica para carregar uma nova imagem neste slot.")]
    [SerializeField] public Button loadButton;

    [Tooltip("O botão que o usuário clica para remover a imagem carregada.")]
    [SerializeField] public Button cancelButton;

    [Tooltip("A imagem padrão a ser exibida quando nenhum arquivo for carregado.")]
    [SerializeField] public Texture defaultTexture;

    public ImageInfo SlotInfo;
    public bool Setted;
    
    public void Awake()
    {
        if (displayImage != null)
        {
            displayImage.texture = defaultTexture;
        }
        
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(false);
        }
    }
    
    public void UpdateDisplay(Texture newTexture, ImageInfo info)
    {
        SlotInfo = info;

        if (displayImage != null)
        {
            displayImage.texture = newTexture ?? defaultTexture;
        }

        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(newTexture != null);
        }
    }
}