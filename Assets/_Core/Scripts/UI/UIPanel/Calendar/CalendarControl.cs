using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CalendarControl : MonoBehaviour
{
    [SerializeField] int month;
    List<CalendarCard> _cards;
    DateTime _date;

    public DateTime Date => _date;

    private void Awake() => _cards = GetComponentsInChildren<CalendarCard>().ToList();
    public CalendarCard SetDate( DateTime date, bool chooseToday = false ) {
        CalendarCard card = null;
        month = date.Month;
        _date = date.AddDays( -(date.Day - 1) );
        var days = DateTime.DaysInMonth( date.Year, date.Month );
        for ( int i = 0; i < _cards.Count; i++ ) {
            _cards[i].gameObject.SetActive( days > i );
            if ( days <= i ) continue;
            _cards[i].SetDate( _date );
            if ( chooseToday && _date.ToString( CalendarPanel.DATE_MASK ) == DateTime.Today.ToString( CalendarPanel.DATE_MASK ) )
                card = _cards[i];
            _date = _date.AddDays( 1 );
        }
        return card;
    }
}
