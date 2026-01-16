using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MonthView : MonoBehaviour
{
    private List<DayView> _dayViews;
    public DateTime DisplayedDate { get; private set; }

    private void Awake()
    {
        _dayViews = GetComponentsInChildren<DayView>(true).ToList();
    }
    
    public DayView Populate(DateTime monthToShow, bool isCurrentMonthForSetup, Func<DateTime, bool> hasEventCallback, UnityAction<DateTime> onDateSelectedCallback)
    {
        DisplayedDate = monthToShow;
        DateTime firstDayOfMonth = new DateTime(monthToShow.Year, monthToShow.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(monthToShow.Year, monthToShow.Month);
        DayView todayCard = null;

        for (int i = 0; i < _dayViews.Count; i++)
        {
            if (i < daysInMonth)
            {
                DateTime currentDate = firstDayOfMonth.AddDays(i);
                _dayViews[i].gameObject.SetActive(true);
                
                bool hasEvent = hasEventCallback(currentDate);
                _dayViews[i].Setup(currentDate, hasEvent);
                
                _dayViews[i].OnDateSelected.RemoveAllListeners();
                _dayViews[i].OnDateSelected.AddListener(onDateSelectedCallback);
                
                if (isCurrentMonthForSetup && currentDate.Date == DateTime.Today.Date)
                {
                    todayCard = _dayViews[i];
                }
            }
            else
            {
                _dayViews[i].gameObject.SetActive(false);
            }
        }
        
        return todayCard;
    }

    public void UpdateSelection(DateTime selectedDate)
    {
        // Se a data selecionada não pertence a este mês, desmarca todos os dias
        if (DisplayedDate.Year != selectedDate.Year || DisplayedDate.Month != selectedDate.Month)
        {
            foreach(var day in _dayViews)
            {
                if(day.gameObject.activeInHierarchy) day.SetSelected(false);
            }
            return;
        }

        // Caso contrário, marca o dia correto e desmarca os outros
        foreach (var day in _dayViews)
        {
            if(day.gameObject.activeInHierarchy)
            {
                day.SetSelected(day.Date.Day == selectedDate.Day);
            }
        }
    }
}