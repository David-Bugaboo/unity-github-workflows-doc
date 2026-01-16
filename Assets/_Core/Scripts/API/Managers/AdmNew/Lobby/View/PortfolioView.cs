using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PortfolioView : BaseUploaderView
{
    [Header("Campos Específicos do Portfólio")]
    [SerializeField] private TMP_InputField urlInputField;
    
    protected override string UploadType => "PORTFOLIO";
    
    protected override string[] SlotKeys => new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    
    public PortfolioView() : base()
    {
        _slotPaths = new string[9];
    }
    
    public string GetCurrentUrl()
    {
        return urlInputField != null ? urlInputField.text : "";
    }
}