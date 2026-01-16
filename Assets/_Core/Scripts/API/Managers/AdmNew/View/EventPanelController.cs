using UnityEngine;

public class EventPanelController : MonoBehaviour
{
    private EventPanelPresenter _presenter;
    private bool _isInitialized;
    
    private void Initialize()
    {
        if (_isInitialized) return;

        var view = GetComponent<EventPanelView>();
        var apiManager = APIManager.Instance;
        
        var eventApiService = new EventApiService(apiManager);
        var userApiService = new UserApiService(apiManager);
        
        _presenter = new EventPanelPresenter(view, eventApiService, userApiService);
        _isInitialized = true;
    }

    private void Awake()
    {
        Initialize();
    }
    
    public void CreateNewEvent()
    {
        Initialize();
        _presenter.StartCreateNewEvent();
    }
    
    public void EditEvent(EventData eventData)
    {
        Initialize();
        _presenter.StartEditEvent(eventData);
    }
    
    public void OnConfirmClicked()
    {
        _presenter.TriggerConfirm();
    }
    
    /// <summary>
    /// Conecte esta função ao botão "Salvar Evento" (na primeira tela).
    /// </summary>
    public void OnSaveEventClicked()
    {
        _presenter.TriggerSaveEvent();
    }

    /// <summary>
    /// Conecte esta função ao botão "Confirmar Convites" (na segunda tela).
    /// </summary>
    public void OnInviteUsersClicked()
    {
        _presenter.TriggerInviteUsers();
    }
}