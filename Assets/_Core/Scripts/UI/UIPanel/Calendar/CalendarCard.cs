using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarCard : MonoBehaviour
{
    [SerializeField] TMP_Text weekDay, monthDay;
    [SerializeField] GameObject eventIcon;
    [SerializeField] Image backGround;
    [SerializeField] Color type_1, type_2;
    string _currentDate, _chosenDate;
    
    public string CurrentDate => _currentDate;
    
    public void SetDate( DateTime date, string targetDate ) 
    {
        SetDate( date );
        _currentDate = targetDate;
    }
    
    public void SetDate( DateTime date ) 
    {
        weekDay.text = CalendarPanel.Info.DateTimeFormat.GetAbbreviatedDayName( date.DayOfWeek );
        monthDay.text = date.Day.ToString();
        _currentDate = date.ToString( CalendarPanel.DATE_MASK );
        eventIcon.SetActive( CalendarPanel.HasEvent( _currentDate ) );
        InternalUpdate( _chosenDate == _currentDate );
    }
    
    public void SendDateToView( CalendarPanel view ) => view.UpdateCards( _currentDate, this );
    
    public void UpdateCardState( bool chosen ) 
    {
        _chosenDate = chosen ? _currentDate : null;
        InternalUpdate( chosen );
    }
    
    void InternalUpdate( bool chosen ) 
    {
        weekDay.color = chosen ? type_1 : type_2;
        monthDay.color = chosen ? type_1 : type_2;
        backGround.color = chosen ? type_2 : type_1;
    }
}
