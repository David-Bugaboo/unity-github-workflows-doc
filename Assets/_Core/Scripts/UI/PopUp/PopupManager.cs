using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    static PopupManager instance;

    const string ERROR_CODE = "Code: {0}", DEFAULT_BUTTON_TEXT = "Fechar";

    [SerializeField] TMP_Text title, msg, code, confirm, cancel;
    [SerializeField] Image icon;
    [SerializeField] RectTransform container;
    [SerializeField] Button blockerBtn;
    [SerializeField] CanvasGroup group;
    [SerializeField] List<PopupMapData> popups;

    public static event Action OnPopupShow, OnPopupHide;

    Action _confirmEvt, _cancelEvt;

    bool _isShowing = false;

    public static bool IsShowing
    {
        get => instance._isShowing;
        private set { ((instance._isShowing = value) ? OnPopupShow : OnPopupHide)?.Invoke(); }
    }

    Action _queuedChangedItems, _queuedError;

    Dictionary<string, PopupContainerData> _popupMap;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _popupMap = new();
        popups.ForEach(p => _popupMap.Add(p.ID, p.data));
        instance = this;
        DontDestroyOnLoad(gameObject);
        group.blocksRaycasts = false;
        blockerBtn.interactable = false;
        container.localScale = Vector3.zero;
        group.alpha = 0;
        ErrorHandler.OnError += ShowError;
        //APIHandler.RegisterItemChangedCallback( ShowChangedItems );
    }

    void ShowChangedItems(UpdatedItems updatedItems)
    {
        if (updatedItems == null) return;
        if (IsShowing)
        {
            _queuedChangedItems = () => ShowChangedItems(updatedItems);
            return;
        }

        IsShowing = true;
        title.text = "Atualizado";
        StringBuilder builder = new();
        if (updatedItems.updatedGroups.added.Length > 0)
            builder.AppendLine($"Events Added: {updatedItems.updatedGroups.added.Length}");
        if (updatedItems.updatedGroups.removed.Length > 0)
            builder.AppendLine($"Events Expired: {updatedItems.updatedGroups.removed.Length}");
        
        // Linhas relacionadas a "Badges" foram removidas daqui.
        
        msg.text = builder.ToString();
        group.blocksRaycasts = true;
        DOTween.To(() => group.alpha, alpha => group.alpha = alpha, 1, .3f);
        container.DOScale(1, .3f).OnComplete(() => blockerBtn.interactable = true);
    }

    public void CallConfirm() => _confirmEvt?.Invoke();
    public void CallCancel() => _cancelEvt?.Invoke();

    public static void ShowMessage(string title, string message)
    {
        if (IsShowing) return;
        instance.SetTextAndShow(title, message, string.Empty);
    }

    public static void ShowMessage(PopupData data)
    {
        if (IsShowing) return;
        instance.SetTextAndShow(data);
    }

    public static void ShowMessage(string title, string message, string yesBtn)
    {
        if (IsShowing) return;
        instance.SetTextAndShow(title, message, string.Empty);
    }

    void ShowError(ErrorHandler.ErrorData data)
    {
        if (data == null || data.Title == null) return;
        if (IsShowing)
        {
            _queuedError = () => ShowError(data);
            return;
        }

        SetTextAndShow(data.Title, data.Message, string.Format(ERROR_CODE, data.Code));
    }

    void ShowError(string id)
    {
        if (_popupMap.ContainsKey(id)) ShowError(_popupMap[id]);
    }

    void ShowError(PopupContainerData data)
    {
        if (data == null || data.Title == null) return;
        if (IsShowing)
        {
            _queuedError = () => ShowError(data);
            return;
        }

        SetTextAndShow(data.Title, data.Message, string.Format(ERROR_CODE, data.Code), data.Icon);
    }

    void SetTextAndShow(string title, string msg, string code, Sprite icon = null)
    {
        IsShowing = true;
        this.icon.sprite = icon;
        if (this.icon)
            this.icon.gameObject.SetActive(icon);
        instance.title.text = title;
        instance.msg.text = msg;
        instance.code.text = code;
        instance.confirm.text = DEFAULT_BUTTON_TEXT;
        instance.cancel.transform.parent.gameObject.SetActive(false);
        group.blocksRaycasts = true;
        DOTween.To(() => group.alpha, alpha => group.alpha = alpha, 1, .3f);
        container.DOScale(1, .3f).OnComplete(() => blockerBtn.interactable = true);
    }

    void SetTextAndShow(PopupData data)
    {
        IsShowing = true;
        title.text = data.Title;
        msg.text = data.Message;
        code.gameObject.SetActive(false);
        confirm.text = data.ConfirmText;
        cancel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(data.CancelText));
        cancel.text = data.CancelText;
        group.blocksRaycasts = true;
        _confirmEvt = data.OnConfirm;
        _cancelEvt = data.OnCancel;
        DOTween.To(() => group.alpha, alpha => group.alpha = alpha, 1, .3f);
        container.DOScale(1, .3f).OnComplete(() => blockerBtn.interactable = true);
    }

    public void HideError()
    {
        _confirmEvt = null;
        _confirmEvt = null;
        blockerBtn.interactable = false;
        DOTween.To(() => group.alpha, alpha => group.alpha = alpha, 0, .3f)
            .OnComplete(() => group.blocksRaycasts = false);
        container.DOScale(0, .3f).OnComplete(() =>
        {
            IsShowing = false;
            _queuedChangedItems?.Invoke();
            _queuedChangedItems = null;
            _queuedError?.Invoke();
            _queuedError = null;
        });
    }

    public class WaitPopup : CustomYieldInstruction
    {
        public override bool keepWaiting => IsShowing;
        public WaitPopup(PopupData data) => ShowMessage(data);
    }
}