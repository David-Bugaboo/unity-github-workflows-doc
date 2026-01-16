using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DayView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private TMP_Text weekDayText;
    [SerializeField] private TMP_Text monthDayText;
    [SerializeField] private GameObject eventIcon;
    [SerializeField] private Image background;

    [Header("State Colors")]
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField] private Color defaultTextColor = Color.black;
    [SerializeField] private Color selectedBackgroundColor = Color.black;
    [SerializeField] private Color defaultBackgroundColor = Color.white;
    
    public UnityEvent<DateTime> OnDateSelected;

    public DateTime Date { get; private set; }
    
    public void Setup(DateTime date, bool hasEvent)
    {
        Date = date;
        gameObject.name = $"Day_{date:dd}";
        
        var culture = new System.Globalization.CultureInfo("pt-BR");
        weekDayText.text = culture.DateTimeFormat.GetAbbreviatedDayName(date.DayOfWeek);
        monthDayText.text = date.Day.ToString();
        eventIcon.SetActive(hasEvent);
        SetSelected(false);
    }
    
    public void SetSelected(bool isSelected)
    {
        weekDayText.color = isSelected ? selectedTextColor : defaultTextColor;
        monthDayText.color = isSelected ? selectedTextColor : defaultTextColor;
        background.color = isSelected ? selectedBackgroundColor : defaultBackgroundColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnDateSelected?.Invoke(Date);
    }
}