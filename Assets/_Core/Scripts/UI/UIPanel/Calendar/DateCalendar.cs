using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DateCalendar : MonoBehaviour
{
    [SerializeField] CalendarPanel view;
    [SerializeField] TMP_Text monthLabel;
    [SerializeField] RectTransform contentRect;
    [SerializeField] List<CalendarControl> controls;
    [SerializeField] CalendarCard cardSample;
#if UNITY_EDITOR
    [SerializeField, Range(0, 1)] float hnp;
    [SerializeField, Range(1, 31)] int currentDay = 1;
#endif
    [SerializeField] int dayOffset;
    [SerializeField] float spaceOffset;
    [SerializeField] ScrollRect rect;


    Dictionary<Transform, CalendarControl> _controlMap;
    RectTransform _lastMoved;
    Transform _controlParent;
    DateTime _targetDate;

    bool _forwarded, _bakpadeled, _expectingChange;
    float _cardWidth;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        //UpdateValue( currentDay );
    }
#endif
    private void Awake()
    {
        _controlParent = controls[0].transform.parent;
        _controlMap = new();
        foreach (var control in controls)
            _controlMap.Add(control.transform, control);
    }

    public void OpenCalendar()
    {
        //Debug.Log( $"<><><><><><><><><>>> card size delta {( (RectTransform)cardSample.transform ).sizeDelta}" );
        _cardWidth = ((RectTransform)cardSample.transform).sizeDelta.x;
        UpdateTargetMonthText(DateTime.Today);
        CalendarCard targetCard = null;
        for (int i = 0; i < 3; i++)
        {
            var todayCard = _controlMap[_controlParent.GetChild(i)].SetDate(_targetDate.AddMonths(i - 1), i == 1);
            if (todayCard) targetCard = todayCard;
        }

        if (targetCard) targetCard.SendDateToView(view);
        else Debug.LogError("No target card HAHAHAHAHAHAHH");
#if UNITY_EDITOR
        UpdateValue(currentDay = DateTime.Today.Day);
#else
        UpdateValue( DateTime.Today.Day );
#endif
    }

    [ContextMenu("Debug/go for today")]
    void UpdateValue(int day)
    {
        var days = DateTime.DaysInMonth(_targetDate.Year, _targetDate.Month);
        day = Mathf.Clamp(day, 1, days);
        StopAllCoroutines();
        StartCoroutine(DelayedValueUpdate(day));
    }

    public void UpdateCalendarMonths(Vector2 val)
    {
#if UNITY_EDITOR
        hnp = rect.horizontalNormalizedPosition;
#endif
        if (!_expectingChange) return;
        if (val.x <= .3f && !_bakpadeled)
        {
            _expectingChange = false;
            _bakpadeled = true;
            SendBackwards();
        }

        if (val.x >= .7f && !_forwarded)
        {
            _expectingChange = false;
            _forwarded = true;
            SendFoward();
        }
    }

    public void DraginDeezNuts() => _expectingChange = _bakpadeled = _forwarded = false;

    public void ReleasedMouse()
    {
        if (rect.horizontalNormalizedPosition <= .3f) SendBackwards();
        if (rect.horizontalNormalizedPosition >= .7f) SendFoward();
        _expectingChange = true;
    }

    void UpdateTargetMonthText(DateTime val)
    {
        _targetDate = val;
        //Debug.Log( $"{UI_CalendarEvent_View.Info.DateTimeFormat.GetMonthName( _targetDate.Month )}, {_targetDate.Year}" );
        monthLabel.text =
            $"{CalendarPanel.Info.DateTimeFormat.GetMonthName(_targetDate.Month)}, {_targetDate.Year}";
    }

    void SendFoward()
    {
        _lastMoved = (RectTransform)_controlParent.GetChild(0);
        _lastMoved.SetAsLastSibling();
        _controlMap[_lastMoved].SetDate((_targetDate = _targetDate.AddMonths(1)).AddMonths(1));
        contentRect.anchoredPosition = new(contentRect.anchoredPosition.x + _lastMoved.sizeDelta.x,
            contentRect.anchoredPosition.y);
        UpdateTargetMonthText(_controlMap[_controlParent.GetChild(0)].Date);
    }

    void SendBackwards()
    {
        _lastMoved = (RectTransform)_controlParent.GetChild(_controlParent.childCount - 1);
        _lastMoved.SetAsFirstSibling();
        _controlMap[_lastMoved].SetDate((_targetDate = _targetDate.AddMonths(-1)).AddMonths(-1));
        contentRect.anchoredPosition = new(contentRect.anchoredPosition.x - _lastMoved.sizeDelta.x,
            contentRect.anchoredPosition.y);
        UpdateTargetMonthText(_controlMap[_controlParent.GetChild(0)].Date);
    }

    IEnumerator DelayedValueUpdate(int day)
    {
        yield return null;
        var lastMonthDate = _targetDate.AddMonths(-1);
        var lastMonthDays = DateTime.DaysInMonth(lastMonthDate.Year, lastMonthDate.Month);
        var val = (_cardWidth * (lastMonthDays + 1)) + ((day - 1 + dayOffset) * _cardWidth);
        contentRect.anchoredPosition = new(-val + spaceOffset, contentRect.anchoredPosition.y);
    }
}