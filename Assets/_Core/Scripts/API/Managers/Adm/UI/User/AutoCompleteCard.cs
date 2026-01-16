using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoCompleteCard : UI_Card_New<string>
{
    [SerializeField] private TMP_Text suggestionText;
    
    public event Action<string> OnSuggestionSelected;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            OnSuggestionSelected?.Invoke(Data);
        });
    }

    protected override void OnDataSet()
    {
        suggestionText.text = Data;
    }
}