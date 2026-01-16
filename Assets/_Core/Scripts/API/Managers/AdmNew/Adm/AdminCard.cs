using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminCard : UI_Card<UserData>
{
    [SerializeField] private TextMeshProUGUI adminNameText;
    [SerializeField] private Button demoteButton;

    public event Action<UserData> OnActionClicked;

    private void Awake()
    {
        if (demoteButton != null)
        {
            demoteButton.onClick.AddListener(() => OnActionClicked?.Invoke(Data));
        }
    }

    protected override void OnDataSet()
    {
        if (Data != null) adminNameText.text = Data.name;
    }
}