using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EventPanelPresenter
{
    private readonly EventPanelView _view;
    private readonly EventApiService _eventApiService;
    private readonly UserApiService _userApiService;

    private EventData _currentEditingEvent;
    private readonly List<string> _stages = new List<string> { "03_PalcoA", "05_PalcoC", "04_PalcoB" };
    private List<UserData> _allUsersCache = new List<UserData>();
    
    public EventPanelPresenter(EventPanelView view, EventApiService eventApiService, UserApiService userApiService)
    {
        _view = view;
        _eventApiService = eventApiService;
        _userApiService = userApiService;
    }
    
    private async Task LoadAllUsersForAutocomplete()
    {
        if (_allUsersCache.Any()) return;

        _view.SetLoadingState(true);
        _allUsersCache = await _userApiService.GetAllUsersAsync() ?? new List<UserData>();
        _view.SetLoadingState(false);
        Debug.Log($"{_allUsersCache.Count} usuários carregados no cache do presenter.");
    }

    public async void StartCreateNewEvent()
    {
        _currentEditingEvent = null;
        _view.ResetToCreateMode();
        await LoadAllUsersForAutocomplete();
    }

    public async void StartEditEvent(EventData eventToEdit)
    {
        _currentEditingEvent = eventToEdit;
        _view.SetLoadingState(true);

        await LoadAllUsersForAutocomplete();

        Texture2D bannerTexture = null;
        if (!string.IsNullOrEmpty(eventToEdit.banner))
        {
            bannerTexture = await APIManager.Instance.GetTextureFromUrl(eventToEdit.banner);
        }

        List<UserData> members = eventToEdit.guests.Select(guestInfo => guestInfo.guest).ToList();
        _view.PopulateForEdit(eventToEdit, members, bannerTexture);
        _view.SetLoadingState(false);
    }

    public async void TriggerConfirm()
    {
        var uiData = _view.GetCurrentUIData();

        if (string.IsNullOrWhiteSpace(uiData.Name))
        {
            Debug.LogError("O nome do evento não pode ser vazio.");
            return;
        }

        _view.SetLoadingState(true);

        try
        {
            var payload = new EventPayload
            {
                name = uiData.Name,
                description = uiData.Description,
                location = _stages[uiData.LocationIndex],
                start_date = ParseDateFromUI(uiData.Day, uiData.Month, uiData.Year, uiData.Hour, uiData.Minute),
                end_date = ParseDateFromUI(uiData.Day, uiData.Month, uiData.Year, "23", "59"),
                visibility = uiData.IsPublic ? "PUBLIC" : "PRIVATE",
            };

            var (bannerBytes, bannerMimeType) = _view.GetBannerData();
            if (bannerBytes != null && bannerBytes.Length > 0)
            {
                payload.banner = new BannerPayload
                {
                    mime = bannerMimeType,
                    data = Convert.ToBase64String(bannerBytes)
                };
            }

            if (_currentEditingEvent == null)
            {
                EventData createdEvent = await _eventApiService.CreateEventAsync(payload);
                if (createdEvent != null && !string.IsNullOrEmpty(createdEvent.id))
                {
                    if (uiData.NewInvitedEmails.Any())
                    {
                        // Agora esta linha funcionará, pois '_allUsersCache' existe
                        List<string> userIds = uiData.NewInvitedEmails
                            .Select(email => _allUsersCache.FirstOrDefault(user => user.email == email)?.id)
                            .Where(id => !string.IsNullOrEmpty(id))
                            .ToList();

                        if (userIds.Any())
                        {
                            await _eventApiService.AddMembersAsync(createdEvent.id, userIds);
                        }
                    }

                    StartCreateNewEvent();
                }
            }
            else // MODO EDIÇÃO
            {
                string eventId = _currentEditingEvent.id;
                bool success = await _eventApiService.EditEventAsync(eventId, payload);
                if (success)
                {
                    var tasks = new List<Task>();
                    if (uiData.NewInvitedEmails.Any())
                    {
                        List<string> userIdsToAdd = uiData.NewInvitedEmails
                            .Select(email => _allUsersCache.FirstOrDefault(user => user.email == email)?.id)
                            .Where(id => !string.IsNullOrEmpty(id))
                            .ToList();
                        if (userIdsToAdd.Any()) tasks.Add(_eventApiService.AddMembersAsync(eventId, userIdsToAdd));
                    }

                    if (uiData.UsersToRemove.Any())
                    {
                        List<string> userIdsToRemove = uiData.UsersToRemove
                            .Select(email => _allUsersCache.FirstOrDefault(user => user.email == email)?.id)
                            .Where(id => !string.IsNullOrEmpty(id))
                            .ToList();
                        if (userIdsToRemove.Any())
                            tasks.Add(_eventApiService.RemoveMembersAsync(eventId, userIdsToRemove));
                    }

                    await Task.WhenAll(tasks);

                    StartEditEvent(_currentEditingEvent);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Um erro ocorreu em TriggerConfirm: {ex.Message} \n {ex.StackTrace}");
        }
        finally
        {
            _view.SetLoadingState(false);
        }
    }
    
    public async void TriggerSaveEvent()
{
    // PASSO 1: Obter dados da View e validar
    var uiData = _view.GetCurrentUIData();
    if (string.IsNullOrWhiteSpace(uiData.Name))
    {
        Debug.LogError("O nome do evento não pode ser vazio.");
        return;
    }
    
    _view.SetLoadingState(true);

    try
    {
        var payload = new EventPayload
        {
            name = uiData.Name,
            description = uiData.Description,
            location = _stages[uiData.LocationIndex],
            start_date = ParseDateFromUI(uiData.Day, uiData.Month, uiData.Year, uiData.Hour, uiData.Minute),
            end_date = ParseDateFromUI(uiData.Day, uiData.Month, uiData.Year, "23", "59"),
            visibility = uiData.IsPublic ? "PUBLIC" : "PRIVATE",
        };

        var (bannerBytes, bannerMimeType) = _view.GetBannerData();
        if (bannerBytes != null && bannerBytes.Length > 0)
        {
            payload.banner = new BannerPayload
            {
                mime = bannerMimeType,
                data = Convert.ToBase64String(bannerBytes)
            };
        }
        
        EventData savedEvent = null;
        if (_currentEditingEvent == null)
        {
            savedEvent = await _eventApiService.CreateEventAsync(payload);
        }
        else
        {
            bool success = await _eventApiService.EditEventAsync(_currentEditingEvent.id, payload);
            if(success)
            {
                savedEvent = _currentEditingEvent;
            }
        }
        
        if (savedEvent != null)
        {
            Debug.Log($"Evento salvo com sucesso! ID: {savedEvent.id}");
            _currentEditingEvent = savedEvent;

            _view.ShowInvitePanel();
        }
        else
        {
            Debug.LogError("Falha ao salvar o evento na API.");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Um erro ocorreu em TriggerSaveEvent: {ex.Message}");
    }
    finally
    {
        _view.SetLoadingState(false);
    }
}
        
    public async void TriggerInviteUsers()
    {
        if (_currentEditingEvent == null)
        {
            Debug.LogError("ERRO: Tentando convidar usuários sem um evento salvo.");
            return;
        }

        _view.SetLoadingState(true);
        var uiData = _view.GetCurrentUIData();
        string eventId = _currentEditingEvent.id;

        var tasks = new List<Task>();
        if (uiData.NewInvitedEmails.Any())
        {
            List<string> userIds = uiData.NewInvitedEmails
                .Select(email => _allUsersCache.FirstOrDefault(u => u.email == email)?.id)
                .Where(id => !string.IsNullOrEmpty(id)).ToList();
            if(userIds.Any()) tasks.Add(_eventApiService.AddMembersAsync(eventId, userIds));
        }
        if (uiData.UsersToRemove.Any())
        {
            List<string> userIds = uiData.UsersToRemove
                .Select(email => _allUsersCache.FirstOrDefault(u => u.email == email)?.id)
                .Where(id => !string.IsNullOrEmpty(id)).ToList();
            if(userIds.Any()) tasks.Add(_eventApiService.RemoveMembersAsync(eventId, userIds));
        }

        await Task.WhenAll(tasks);
        _view.SetLoadingState(false);

        Debug.Log("Operações de membros concluídas!");
        // Opcional: navegar de volta para a lista principal ou fechar tudo.
    }

    private string ParseDateFromUI(string day, string month, string year, string hour, string minute)
    {
        try
        {
            string dateStr = $"{day}/{month}/{year} {hour}:{minute}";
            var date = DateTime.ParseExact(dateStr, "d/M/yyyy HH:mm", CultureInfo.InvariantCulture);
            // A API espera o formato ISO 8601 com 'Z' (UTC)
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao parsear a data: {ex.Message}");
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
    }

    private async void HandleDelete()
    {
        if (_currentEditingEvent == null) return;

        _view.SetLoadingState(true);
        bool success = await _eventApiService.DeleteEventAsync(_currentEditingEvent.id);
        if (success)
        {
            Debug.Log("Evento deletado com sucesso!");
            StartCreateNewEvent(); // Volta para a tela de criação
        }

        _view.SetLoadingState(false);
    }

    private string ParseDateFromUI(EventPanelView.EventUIData uiData)
    {
        try
        {
            string dateStr = $"{uiData.Day}/{uiData.Month}/{uiData.Year} {uiData.Hour}:{uiData.Minute}";
            var date = DateTime.ParseExact(dateStr, "d/M/yyyy HH:mm", CultureInfo.InvariantCulture);
            return date.ToString(); // Formato que a API espera
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao parsear a data: {ex.Message}");
            return DateTime.Now.ToString();
        }
    }
}