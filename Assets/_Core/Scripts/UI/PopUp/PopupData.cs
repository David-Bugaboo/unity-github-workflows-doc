using System;

public readonly struct PopupData
{
    public readonly string Title, Message, ConfirmText, CancelText;
    public readonly Action OnConfirm, OnCancel;

    public PopupData(string title, string message, string confirmTxt, string cancelTxt, Action confirm,
        Action cancel)
    {
        Title = title;
        Message = message;
        ConfirmText = confirmTxt;
        CancelText = cancelTxt;
        OnConfirm = confirm;
        OnCancel = cancel;
    }
}