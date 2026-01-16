using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditEventPanel : MonoBehaviour
{
    public event Action OnActionComplete;

    #region Referências da UI
    [Header("Dados do Evento")]
    [SerializeField] private TMP_InputField eventNameField;
    [SerializeField] private TMP_InputField eventDescField;
    [SerializeField] private Toggle publicToggle;
    [SerializeField] private Toggle privateToggle;
    [SerializeField] private RawImage eventBannerImage;
    [SerializeField] private Texture defaultBannerTexture;

    [Header("Data e Hora")]
    [SerializeField] private TMP_Dropdown dayDropdown;
    [SerializeField] private TMP_Dropdown monthDropdown;
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_InputField hourField;
    [SerializeField] private TMP_InputField minuteField;
    [SerializeField] private TMP_Dropdown locationDropdown;

    [Header("Gerenciamento de Membros")]
    [SerializeField] private UserListView existingMembersList;
    [SerializeField] private TMP_InputField inviteEmailField;
    [SerializeField] private Transform newInvitesContainer;
    [SerializeField] private AutoCompleteCard inviteCardTemplate;
    #endregion

    #region Estado Interno
    private EventData _currentEvent;
    private byte[] _newBannerData;
    private List<UserData> _existingMembers = new List<UserData>();
    private List<string> _newlyInvitedEmails = new List<string>();
    private List<string> _membersToRemove = new List<string>();
    private List<AutoCompleteCard> _newInviteCards = new();
    #endregion
    
    #region Fluxo Principal (Show, Save, Cancel, Delete)

    public async void Show(EventData eventToEdit)
    {
        gameObject.SetActive(true);
        ResetPanelState();
        _currentEvent = eventToEdit;

        if (_currentEvent != null)
        {
            eventNameField.text = _currentEvent.name;
            eventDescField.text = _currentEvent.description;
            publicToggle.isOn = _currentEvent.IsPublic;
            privateToggle.isOn = !_currentEvent.IsPublic;
            PopulateDateTime(_currentEvent.StartDateAsDateTime);

            var bannerTexture = await APIManager.Instance.GetTextureFromUrl(_currentEvent.banner);
            eventBannerImage.texture = bannerTexture ?? defaultBannerTexture;

            var guestList = _currentEvent.guests.Where(c => c.accepted_at != null).ToList();
            foreach (var guest in guestList)
            {
                _existingMembers.Add(guest.guest);
            }
            
            existingMembersList.LoadData(_existingMembers.ToArray());
        }
        else
        {
            eventNameField.text = "";
            eventDescField.text = "";
            publicToggle.isOn = true;
            privateToggle.isOn = false;
            eventBannerImage.texture = defaultBannerTexture;
            PopulateDateTime(DateTime.Now);
            existingMembersList.LoadData(new UserData[0]);
        }
    }
    
    public async void OnSave()
    {
        EventData dataToSave = _currentEvent ?? new EventData();
        dataToSave.name = eventNameField.text;
        dataToSave.description = eventDescField.text;
        
        bool isNewEvent = _currentEvent == null;
        var savedEvent = isNewEvent 
            ? await EventAdminService.CreateEventAsync(dataToSave) 
            : (await EventAdminService.EditEventAsync(_currentEvent.id, dataToSave) ? dataToSave : null);

        if (savedEvent == null) {
            Debug.LogError("Falha ao salvar o evento principal.");
            return;
        }

        // 3. Executa ações secundárias
        string eventId = isNewEvent ? savedEvent.id : _currentEvent.id;
        
        // if (_newBannerData != null)
        //     await FileAdminService.UploadEventBannerAsync(eventId, _newBannerData);
            
        if (_newlyInvitedEmails.Count > 0)
        {
            // TODO: Converter e-mails para IDs de usuário antes de enviar
            // await UserAdminService.AddMembersToEventAsync(eventId, userIds);
        }

        if (_membersToRemove.Count > 0)
            await UserAdminService.RemoveMembersFromEventAsync(eventId, _membersToRemove);

        OnActionComplete?.Invoke();
    }

    public void OnCancel() => OnActionComplete?.Invoke();
    
    public async void OnDelete()
    {
        if (_currentEvent == null) return;
        bool success = await EventAdminService.DeleteEventAsync(_currentEvent.id);
        if (success) OnActionComplete?.Invoke();
    }
    
    #endregion
    
    #region Lógica da UI (Helpers)

    private void ResetPanelState()
    {
        _newBannerData = null;
        _newlyInvitedEmails.Clear();
        _membersToRemove.Clear();
        
        foreach (var card in _newInviteCards) Destroy(card.gameObject);
        _newInviteCards.Clear();
    }
    
    private void PopulateDateTime(DateTime dt) { /* ... (Sua lógica de popular dropdowns) ... */ }
    
    public void OnSelectNewBanner()
    {
        // Abre o file browser, lê os bytes da imagem e guarda em _newBannerData
        // Ex: _newBannerData = File.ReadAllBytes(path);
        // Atualiza a 'eventBannerImage.texture' para dar um preview.
    }

    public void OnAddInvitedUser()
    {
        string email = inviteEmailField.text;
        if (string.IsNullOrWhiteSpace(email) || _newlyInvitedEmails.Contains(email)) return;
        
        _newlyInvitedEmails.Add(email);
        var card = Instantiate(inviteCardTemplate, newInvitesContainer);
        _newInviteCards.Add(card);
        card.gameObject.SetActive(true);
    }
    
    public void MarkMemberForRemoval(UserData user)
    {
        if (!_membersToRemove.Contains(user.id))
        {
            _membersToRemove.Add(user.id);
        }
    }

    #endregion
}