using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CalendarPanel : UI_EventView
{
    [SerializeField] CalendarCard calendarBtnTemplate;
    [SerializeField] DateCalendar calendarControl;
    [SerializeField] UnityEvent onFinishedupdating;

    public List<EventData> Data => dataList;
    public static List<EventData> LoadedEvents;
    internal const string DATE_MASK = "dd/MM/yyyy", DATE_SEPARATOR = " ";

    static CultureInfo _info;
    internal static CultureInfo Info => _info ??= new("pt-BR");

    Dictionary<string, CalendarCard> _calendarMap;
    Dictionary<string, List<EventData>> _eventMap;
    // ICardGeneration _generator;
    CalendarCard _currentCard;

    protected override void Awake()
    {
        base.Awake();
        LoadData(new List<EventData>());
        calendarBtnTemplate.gameObject.SetActive(false);
        if (UserManager.Instance.CurrentUser == null) return;
        Initialization();
        LoadData(UserManager.Instance.CurrentUser.events.Where(evt => !string.IsNullOrEmpty(evt.start_date)).ToList());
        if (calendarControl) return;
    //     _generator = new CompleteCalendarCardGeneration(DATE_MASK, this, calendarBtnTemplate);
    //     _generator.PreferredCall().Invoke(_calendarMap, _eventMap);
    }

    void Initialization()
    {
        _eventMap = new();
        _calendarMap = new();
    }

    public async void UpdateCalendarCards()
    {
        // if (_generator != null) _generator.UpdateCalendarCards(_calendarMap, _eventMap);
        // else
        // {
            var eventsResponse = await EventService.GetAllEventsAsync();
            Dictionary<string, EventData> map = new();
            // var dataList = (UserManager.Instance.CurrentUser.IsAdmin ? this : (UI_View<EventData>)this.Where(evt => evt.IsPublic)).ToList();
            foreach (var item in dataList) map.Add(item.id, item);
            dataList.AddRange(UserManager.Instance.CurrentUser.events.Where(evt => !map.ContainsKey(evt.id)));
            LoadData(dataList.Where(dt => !string.IsNullOrEmpty(dt.start_date)).ToList());
            onFinishedupdating?.Invoke();
        // }
    }

    public override void LoadData(List<EventData> data, bool setActive = true)
    {
        _eventMap ??= new();
        base.LoadData(data, setActive);
        LoadedEvents = data;
        foreach (var item in Data)
        {
            var date = item.start_date.Split(DATE_SEPARATOR)[0];
            if (!_eventMap.ContainsKey(date))
                _eventMap.Add(date, new());
            if (!_eventMap[date].Exists(i => i.id == item.id))
                _eventMap[date].Add(item);
        }

        UpdateCards(DateTime.Today.ToString(DATE_MASK));
    }

    public void UpdateCards(string date, CalendarCard card = null)
    {
        if (UserManager.Instance.CurrentUser == null) return;
        UpdateCardState(card);
        int evtCount = _eventMap.ContainsKey(date) ? _eventMap[date].Count : 0;
        Debug.Log($"event map [{date}] count: {evtCount}");
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(i < evtCount);
            if (i < evtCount)
                cards[i].Data = _eventMap[date][i];
        }
    }

    void UpdateCardState(CalendarCard card)
    {
        if (_currentCard) _currentCard.UpdateCardState(false);
        _currentCard = card;
        if (card) card.UpdateCardState(true);
    }

    public static bool HasEvent(string date) => LoadedEvents.FirstOrDefault(evt => evt.start_date?.Split(DATE_SEPARATOR)[0] == date) != null;
}