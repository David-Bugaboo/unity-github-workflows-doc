using UnityEngine;

public class EventListController : MonoBehaviour
{
    [SerializeField] private EventPanelController eventPanelController;
    private EventListPresenter _presenter;

    async void Start()
    {
        var view = GetComponent<EventListView>();
        var apiManager = APIManager.Instance;
        var eventApiService = new EventApiService(apiManager);
        _presenter = new EventListPresenter(view, eventApiService);
        await _presenter.LoadInitialEvents();
    }

    public void UpdateEvents()
    {
        _presenter.LoadInitialEvents();
    }
    
    public void HandleCreateNewRequest()
    {
        eventPanelController.CreateNewEvent();
    }

    public void HandleEditRequest(EventData card)
    {
        eventPanelController.EditEvent(card);
    }
    
    public void HandleDeleteRequest(EventData card)
    {
        _presenter.TriggerDelete(card);
    }
}
