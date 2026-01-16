using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CalendarManager : MonoBehaviour
{
    // A referência agora é para o CalendarController
    [SerializeField] private CalendarView calendarController; 
    [SerializeField] private NewEventListView eventListView; // Supondo que você mantenha este nome

    // Dicionário para buscar eventos de um dia específico
    private Dictionary<DateTime, List<EventData>> _eventsByDay = new Dictionary<DateTime, List<EventData>>();
    // HashSet para uma verificação super rápida se uma data tem evento (para os ícones)
    private HashSet<DateTime> _datesWithEvents = new HashSet<DateTime>();

    private const string DATE_FORMAT = "dd/MM/yyyy";
    private static readonly CultureInfo Culture = new CultureInfo("pt-BR");

    /// <summary>
    /// O Start agora inicia todo o processo de carregamento e exibição.
    /// </summary>

    public void CallOpenCalendar() => OpenCalendar();

    public async void OpenCalendar()
    {
        await LoadAllEvents();
        calendarController.OpenCalendar();
    }

    /// <summary>
    /// Busca e processa todos os eventos.
    /// </summary>
    private async Task LoadAllEvents()
    {
        // Use o seu serviço real aqui
        var allEvents = await EventService.GetAllEventsAsync(); 
        allEvents = allEvents.Where(c => c.guests.Any(c => c.guest_id == UserManager.Instance.CurrentUser.id)).ToList();

        _eventsByDay.Clear();
        _datesWithEvents.Clear();

        foreach (var evt in allEvents)
        {
            if (DateTime.TryParse(evt.start_date, out DateTime eventDate))
            {
                var dateOnly = eventDate.Date;
                if (!_eventsByDay.ContainsKey(dateOnly))
                {
                    _eventsByDay[dateOnly] = new List<EventData>();
                }
                _eventsByDay[dateOnly].Add(evt);
                _datesWithEvents.Add(dateOnly);
            }
        }
    }

    /// <summary>
    /// MÉTODO PÚBLICO: Chamado pelo CalendarController quando uma data é selecionada.
    /// </summary>
    public void OnDateSelected(DateTime date)
    {
        if (_eventsByDay.TryGetValue(date.Date, out var eventsForDay))
        {
            eventListView.DisplayEvents(eventsForDay);
        }
        else
        {
            eventListView.DisplayEvents(new List<EventData>());
        }
    }
    
    /// <summary>
    /// MÉTODO PÚBLICO: Chamado pelo MonthView (através do CalendarController)
    /// para saber se deve exibir o ícone de evento em um dia.
    /// </summary>
    public bool HasEventOnDate(DateTime date)
    {
        return _datesWithEvents.Contains(date.Date);
    }
}