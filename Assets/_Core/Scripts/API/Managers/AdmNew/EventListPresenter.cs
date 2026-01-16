using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EventListPresenter
{
    private readonly EventListView _view;
    private readonly EventApiService _apiService;
    
    public EventListPresenter(EventListView view, EventApiService apiService)
    {
        _view = view;
        _apiService = apiService;
    }

    /// <summary>
    /// Busca a lista inicial de eventos da API e manda a View exibi-los.
    /// </summary>
    public async Task LoadInitialEvents()
    {
        _view.SetLoading(true);
        List<EventData> events = await _apiService.GetAllEventsAsync();
        if (events != null)
        {
            _view.DisplayEvents(events);
        }
        _view.SetLoading(false);
    }

    /// <summary>
    /// MUDANÇA: O método agora é público e contém a lógica completa de deleção.
    /// </summary>
    public async void TriggerDelete(EventData eventData)
    {
        _view.SetLoading(true);
        
        // A lógica de deleção está descomentada e completa.
        bool success = await _apiService.DeleteEventAsync(eventData.id);
        if (success) 
        {
            Debug.Log($"Evento '{eventData.name}' deletado com sucesso.");
            await LoadInitialEvents(); // Recarrega a lista para refletir a mudança.
        } 
        else 
        {
            Debug.LogError($"Falha ao deletar o evento '{eventData.name}'.");
            _view.SetLoading(false); // Só para de carregar se der erro, pois o sucesso já recarrega.
            // Aqui você poderia mostrar um pop-up de erro para o usuário.
        }
    }
}