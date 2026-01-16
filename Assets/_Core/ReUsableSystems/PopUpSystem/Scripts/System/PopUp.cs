using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    public PopUpData popUpData;
    public RectTransform panel;
    public TextMeshProUGUI HeaderText;
    public TextMeshProUGUI DescText;
    public Button ConfirmButton;
    public Button CancelButton;
    public GameObject prefabEffect;
    [SerializeField] private GameObject instanciedGO;

    public void Initialize(PopUpData data, Action confirmAction, Action cancelAction)
    {
        // SoundManager.Instance.PlaySound("PopUpSound");
        
        popUpData = data;
        
        if(HeaderText) HeaderText.text = popUpData.Header;

        if (string.IsNullOrEmpty(popUpData.Description))
        {
            DescText.gameObject.SetActive(false);
            if (panel != null)
            {
                var newHeight = panel.sizeDelta.y + DescText.rectTransform.rect.y;
                panel.sizeDelta = new Vector2(panel.sizeDelta.x, newHeight);   
            }
        }
        else
        {
            if(DescText != null) DescText.text = popUpData.Description;
        }

        if (data.PopUpType == EPopUpType.Ok)
        {
            ConfirmButton.transform.localPosition = new Vector2(0, ConfirmButton.transform.localPosition.y);
        }
        
        if(!string.IsNullOrEmpty(data.confirmName)) ConfirmButton.GetComponentInChildren<TextMeshProUGUI>().text = data.confirmName;
        if(!string.IsNullOrEmpty(data.cancelName)) CancelButton.GetComponentInChildren<TextMeshProUGUI>().text = data.cancelName;

        if (ConfirmButton != null)
        {
            ConfirmButton.onClick.AddListener(() =>
            {
                confirmAction?.Invoke();
                if(instanciedGO != null) Destroy(instanciedGO);
                Destroy(gameObject);
                Debug.Log("popup destruido");
            });   
        }

        if (data.PopUpType != EPopUpType.Ok)
        {
            if (CancelButton)
            {
                if(cancelAction != null) CancelButton.onClick.AddListener(() =>
                {
                    cancelAction.Invoke();
                    Destroy(gameObject);
                });
                else CancelButton.onClick.AddListener(() => Destroy(gameObject));   
            }
        }
        
        if (prefabEffect != null)
        {
            instanciedGO = Instantiate(prefabEffect);
            instanciedGO.transform.position = new Vector3(-1.4f, 4.2f);
        }
    }

    public void InitializeHeader(string header, Action callback = null)
    {
        // SoundManager.Instance.PlaySound("PopUpSound");
        
        HeaderText.text = header;
        if (prefabEffect != null)
        {
            instanciedGO = Instantiate(prefabEffect);
            instanciedGO.transform.position = new Vector3(-1.4f, 4.2f);
        }
        if (ConfirmButton != null)
        {
            if (callback != null) ConfirmButton.onClick.AddListener(callback.Invoke);
            if (instanciedGO != null)
            {
                ConfirmButton.onClick.AddListener(() => Destroy(instanciedGO));   
            }
            ConfirmButton.onClick.AddListener(() => Destroy(gameObject));
        }
    }
    
    void OnDestroy()
    {
        if(ConfirmButton) ConfirmButton.onClick.RemoveAllListeners();
        if(CancelButton) CancelButton.onClick.RemoveAllListeners();
    }
    
    public void SelfDestroy() => Destroy(gameObject);
}
