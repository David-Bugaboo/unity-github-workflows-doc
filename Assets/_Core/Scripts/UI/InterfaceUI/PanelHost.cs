using UnityEngine;

public class PanelHost : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    
    private OriginalState _savedState;
    private GameObject _currentHostedPanel;
    
    private class OriginalState
    {
        public Transform Parent;
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public Vector3 LocalScale;
        public int SiblingIndex;
    }
    
    public void HostAndExpandPanel(GameObject panelToHost)
    {
        if (panelToHost == null)
        {
            Debug.LogError("O painel para hospedar não pode ser nulo.", this);
            return;
        }
        
        if (_currentHostedPanel != null)
        {
            CloseHostedPanel();
        }

        var rectTransform = panelToHost.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("O objeto fornecido não possui um RectTransform. Não é um item de UI válido.", this);
            return;
        }

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        
        _savedState = new OriginalState
        {
            Parent = rectTransform.parent,
            AnchorMin = rectTransform.anchorMin,
            AnchorMax = rectTransform.anchorMax,
            AnchoredPosition = rectTransform.anchoredPosition,
            SizeDelta = rectTransform.sizeDelta,
            LocalScale = rectTransform.localScale,
            SiblingIndex = rectTransform.GetSiblingIndex()
        };
        
        _currentHostedPanel = panelToHost;
        rectTransform.SetParent(this.transform, worldPositionStays: false);
        
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        rectTransform.localScale = Vector3.one;
    }
    
    public void CloseHostedPanel()
    {
        if (_currentHostedPanel == null || _savedState == null)
        {
            return;
        }
        
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        var rectTransform = _currentHostedPanel.GetComponent<RectTransform>();
        rectTransform.SetParent(_savedState.Parent, worldPositionStays: false);
        
        rectTransform.SetSiblingIndex(_savedState.SiblingIndex);
        
        rectTransform.anchorMin = _savedState.AnchorMin;
        rectTransform.anchorMax = _savedState.AnchorMax;
        rectTransform.anchoredPosition = _savedState.AnchoredPosition;
        rectTransform.sizeDelta = _savedState.SizeDelta;
        rectTransform.localScale = _savedState.LocalScale;
        
        _currentHostedPanel = null;
        _savedState = null;
    }
}