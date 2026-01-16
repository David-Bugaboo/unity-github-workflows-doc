using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelView : MonoBehaviour
{
    [Header("Navegação de Tela")]
    [Tooltip("Arraste aqui o GameObject do painel de criar/editar evento.")]
    [SerializeField] private GameObject createEventPanel;
    [Tooltip("Arraste aqui o GameObject do painel de convidar usuários.")]
    [SerializeField] private GameObject inviteUsersPanel;

    
    // --- Referências da UI ---
    [Header("Fields")] [SerializeField] private TMP_InputField eventName;
    [SerializeField] private TMP_InputField eventDesc, hour, minute;
    [SerializeField] private TMP_Dropdown day, month, year, location;
    [SerializeField] private Toggle publicToggle;

    [Header("Display")] [SerializeField] private RawImage loadedImage;
    [SerializeField] private Texture defaultTexture;
    [SerializeField] private GameObject loadingOverlay;
    
    [Header("User Autocomplete")]
    [SerializeField] private TMP_InputField inviteSearchInput;
    [SerializeField] private GameObject autocompleteContainer;
    [SerializeField] private AutoCompleteCard autocompleteCardPrefab;

    [SerializeField] private Transform invitedUsersContainer;
    [SerializeField] private AUI_UserListView userList;
    [SerializeField] private int maxMembers = 20;
    
    private UserApiService _userApiService;
    private List<UserData> _allUsersCache = new List<UserData>();
    private List<AutoCompleteCard> _autocompleteCards = new List<AutoCompleteCard>();
    
    private byte[] _bannerBytes;
    private string _bannerMimeType;
    
    public struct EventUIData
    {
        public string Name, Description, Hour, Minute;
        public string Day, Month, Year;
        public int LocationIndex;
        public bool IsPublic;
        public List<string> NewInvitedEmails;
        public List<string> UsersToRemove;
    }
    
    private Dictionary<string, AutoCompleteCard> _invitedEmailCards = new Dictionary<string, AutoCompleteCard>();
    private List<string> _usersToRemoveFromEvent = new List<string>();

    private void Awake()
    {
        inviteSearchInput.onValueChanged.AddListener(HandleSearchQueryChanged);
        if (autocompleteContainer != null) autocompleteContainer.SetActive(false);
        if(loadingOverlay != null) loadingOverlay.SetActive(false);
    }
    
    private async void Start()
    {
        _userApiService = new UserApiService(APIManager.Instance);
        await LoadAllUsersForAutocomplete();
    }
    
    /// <summary>
    /// Mostra o painel de criação/edição de evento e esconde o de convite.
    /// </summary>
    public void ShowCreatePanel()
    {
        createEventPanel.SetActive(true);
        inviteUsersPanel.SetActive(false);
    }
    
    /// <summary>
    /// Mostra o painel de convite de usuários e esconde o de criação.
    /// </summary>
    public void ShowInvitePanel()
    {
        createEventPanel.SetActive(false);
        inviteUsersPanel.SetActive(true);
    }
    
    private async System.Threading.Tasks.Task LoadAllUsersForAutocomplete()
    {
        if (_allUsersCache.Any()) return;
        
        SetLoadingState(true);
        _allUsersCache = await _userApiService.GetAllUsersAsync() ?? new List<UserData>();
        SetLoadingState(false);
        Debug.Log($"{_allUsersCache.Count} usuários carregados para o autocomplete.");
    }
    
    private void HandleSearchQueryChanged(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            HideAutocompleteResults();
            return;
        }
        
        List<UserData> filteredUsers = _allUsersCache
            .Where(user => user.email.ToLower().Contains(query.ToLower()))
            .Take(5)
            .ToList();
        
        DisplayAutocompleteResults(filteredUsers);
    }
    
    public void SetLoadingState(bool isLoading)
    {
        if(loadingOverlay != null) loadingOverlay.SetActive(isLoading);
    }

    public void PopulateForEdit(EventData eventData, List<UserData> members, Texture bannerTexture)
    {
        eventName.text = eventData.name;
        eventDesc.text = eventData.description;
        
        publicToggle.isOn = string.Equals(eventData.visibility, "PUBLIC", StringComparison.OrdinalIgnoreCase);
        
        DateTime targetDay = eventData.StartDateAsDateTime;
        if (targetDay != DateTime.MinValue)
        {
            PopulateDate(targetDay);
        }
        
        loadedImage.texture = bannerTexture ?? defaultTexture;
        
        if (userList != null && members != null)
        {
            List<string> memberEmails = members.Select(user => user.email).ToList();
            userList.LoadData(memberEmails);
        }
        
        ClearInvites();
    }
    
    public void SelectImageFromFile()
    {
        var extensions = new[] {
            new ExtensionFilter("Arquivos de Imagem", "png", "jpg", "jpeg")
        };
        
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecione um Banner", "", extensions, false);
        
        if (paths.Length > 0)
        {
            string filePath = paths[0];
            if (File.Exists(filePath))
            {
                _bannerBytes = File.ReadAllBytes(filePath);
            
                if (filePath.EndsWith(".png")) _bannerMimeType = "image/png";
                else if (filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg")) _bannerMimeType = "image/jpeg";
            
                var texture = new Texture2D(2, 2);
                texture.LoadImage(_bannerBytes);
                loadedImage.texture = texture;
            }
        }
    }
    public (byte[] bytes, string mimeType) GetBannerData()
    {
        return (_bannerBytes, _bannerMimeType);
    }

    public void ResetToCreateMode()
    {
        eventName.text = string.Empty;
        eventDesc.text = string.Empty;
        publicToggle.isOn = true;

        PopulateDate(DateTime.Now);

        loadedImage.texture = defaultTexture;

        userList.LoadData(new List<string>());
        ClearInvites();
        
        _bannerBytes = null;
        _bannerMimeType = null;
    }

    public void UpdateImage(Texture newTexture)
    {
        loadedImage.texture = newTexture;
    }

    public void ResetImage()
    {
        loadedImage.texture = defaultTexture;
    }
    
    public void DisplayAutocompleteResults(List<UserData> filteredUsers)
    {
        foreach(var card in _autocompleteCards) Destroy(card.gameObject);
        _autocompleteCards.Clear();

        autocompleteContainer.SetActive(filteredUsers.Any());

        foreach(var user in filteredUsers)
        {
            var newCard = Instantiate(autocompleteCardPrefab, autocompleteContainer.transform);
            newCard.Data = user.email;
            
            newCard.OnCardClicked += (selectedCard) => {
                AddInviteCard(selectedCard.Data);
                inviteSearchInput.text = "";
            };
            
            newCard.gameObject.SetActive(true);
            _autocompleteCards.Add(newCard);
        }
    }
    public void HideAutocompleteResults()
    {
        foreach(var card in _autocompleteCards) Destroy(card.gameObject);
        _autocompleteCards.Clear();
        if(autocompleteContainer != null) autocompleteContainer.SetActive(false);
    }

    public void AddInviteCard(string email)
    {
        if (_invitedEmailCards.ContainsKey(email) || _invitedEmailCards.Count + userList.Count >= maxMembers) return;
        var card = Instantiate(autocompleteCardPrefab, invitedUsersContainer); 
    
        card.Data = email;
        card.gameObject.SetActive(true);
        _invitedEmailCards.Add(email, card);
    }

    public void RemoveInviteCard(string email)
    {
        if (!_invitedEmailCards.ContainsKey(email)) return;
        Destroy(_invitedEmailCards[email].gameObject);
        _invitedEmailCards.Remove(email);
    }

    public void MarkUserForRemoval(string userEmail)
    {
        _usersToRemoveFromEvent.Add(userEmail);
    }

    public EventUIData GetCurrentUIData()
    {
        return new EventUIData
        {
            Name = eventName.text,
            Description = eventDesc.text,
            Hour = hour.text,
            Minute = minute.text,
            Day = day.options[day.value].text,
            Month = (month.value + 1).ToString(),
            Year = year.options[year.value].text,
            LocationIndex = location.value,
            IsPublic = publicToggle.isOn,
            NewInvitedEmails = new List<string>(_invitedEmailCards.Keys),
            UsersToRemove = _usersToRemoveFromEvent,
        };
    }

    private void ClearInvites()
    {
        foreach (var card in _invitedEmailCards.Values)
        {
            Destroy(card.gameObject);
        }

        _invitedEmailCards.Clear();
        _usersToRemoveFromEvent.Clear();
    }

    private void PopulateDate(DateTime date)
    {
        year.value = year.options.FindIndex(opt => opt.text == date.Year.ToString());
        month.value = date.Month - 1;
        SetMonthDays(date.Year, date.Month);
        day.value = date.Day - 1;
        hour.text = date.Hour.ToString("00");
        minute.text = date.Minute.ToString("00");
    }

    private void SetMonthDays(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        if (day.options.Count == daysInMonth) return;

        day.options.Clear();
        for (int i = 1; i <= daysInMonth; i++)
        {
            day.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }

        day.RefreshShownValue();
    }
}