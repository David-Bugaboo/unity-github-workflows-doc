using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventCard : UI_Card<EventData>
{
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI eventDateText;
    
    public Button editButton;
    public Button deleteButton;

    protected override void OnDataSet()
    {
        if (Data == null) return;
        eventNameText.text = Data.name;
        eventDateText.text = Data.StartDateAsDateTime.ToString("dd/MM/yyyy HH:mm");
    }
}