using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_EventView : UI_View<EventData>
{
    public async void GetAndDisplayEvents(string localName)
    {
        var allEvents = await EventService.GetAllEventsAsync();
        var isAdmin = UserManager.Instance.CurrentUser.IsAdmin;

        var filteredEvents = allEvents
            .Where(evt => isAdmin || evt.IsPublic)
            .Where(evt => evt.location == localName);

        LoadData(filteredEvents.ToList());
    }
}