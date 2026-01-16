using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShareLinkPopUpManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI linkText;
    [SerializeField] private Button copyButton;

    [Header("Configurações")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color hoverColor = new Color(0.2f, 0.2f, 1f);

    private void Start()
    {
        // Configuração inicial
        linkText.color = normalColor;
        
        // Evento do botão
        copyButton.onClick.AddListener(CopyToClipboard);
        
        // Evento de clique no texto (duas opções)
        AddTextClickEvent();
    }

    // Método 1: Usando EventTrigger
    private void AddTextClickEvent()
    {
        EventTrigger trigger = linkText.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OpenLink(); });
        
        trigger.triggers.Add(entry);
    }

    // Método 2: Usando Button component
    // (Adicione um componente Button ao LinkText e configure no Inspector)
    public void OpenLink()
    {
        Application.OpenURL(linkText.text);
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = linkText.text;
        Debug.Log("Texto copiado: " + linkText.text);
        
        // Feedback visual (opcional)
        StartCoroutine(AnimateButton());
    }

    private System.Collections.IEnumerator AnimateButton()
    {
        copyButton.image.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        copyButton.image.color = Color.white;
    }

    // Efeito hover opcional
    public void OnTextHover(bool isHovering)
    {
        linkText.color = isHovering ? hoverColor : normalColor;
    }
}
