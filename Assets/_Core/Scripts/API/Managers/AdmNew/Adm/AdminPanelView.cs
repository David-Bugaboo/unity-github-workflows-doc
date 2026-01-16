using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminPanelView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_InputField userSearchInput;
    [SerializeField] private GameObject autocompleteContainer;
    [SerializeField] private Transform adminListContainer; // O ÚNICO container da lista
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject loadingOverlay;
    
    [Header("Prefabs")]
    [SerializeField] private AutoCompleteCard autocompleteCardPrefab;
    [SerializeField] private AdminCard adminCardPrefab;

    // Serviços de API
    private UserApiService _userApiService;
    private AdminApiService _adminApiService;

    // Listas para gerenciar o estado
    private List<UserData> _allUsersCache = new List<UserData>();
    private List<UserData> _nonAdmins = new List<UserData>();
    
    // Listas de AÇÕES PENDENTES (usando HashSet para eficiência)
    private HashSet<string> _usersToPromote = new HashSet<string>(); // IDs de usuários a promover
    private HashSet<string> _adminsToDemote = new HashSet<string>(); // IDs de admins a rebaixar

    // Dicionário para gerenciar os cards que estão visíveis na UI
    private Dictionary<string, AdminCard> _currentCardsOnScreen = new Dictionary<string, AdminCard>();
    private List<AutoCompleteCard> _autocompleteCards = new List<AutoCompleteCard>();

    private void Awake()
    {
        // Inicializa os serviços
        _userApiService = new UserApiService(APIManager.Instance);
        _adminApiService = new AdminApiService(APIManager.Instance);

        // Conecta os eventos da UI
        userSearchInput.onValueChanged.AddListener(HandleSearchQueryChanged);
        confirmButton.onClick.AddListener(OnConfirmChangesClicked);
        if (autocompleteContainer != null) autocompleteContainer.SetActive(false);
        if (loadingOverlay != null) loadingOverlay.SetActive(false);
    }

    private async void Start()
    {
        adminCardPrefab.gameObject.SetActive(false);
        autocompleteCardPrefab.gameObject.SetActive(false);
        await InitializePanel();
    }

    public void PopulatePanel() => InitializePanel();

    /// <summary>
    /// Carrega todos os usuários, separa as listas e atualiza a UI. Ponto de entrada principal.
    /// </summary>
    public async Task InitializePanel()
    {
        SetLoading(true);
        
        // Limpa as ações pendentes e a UI
        _usersToPromote.Clear();
        _adminsToDemote.Clear();
        ClearAndDestroyAllCards();
        
        // Busca todos os usuários
        _allUsersCache = await _userApiService.GetAllUsersAsync() ?? new List<UserData>();
        var currentAdmins = _allUsersCache.Where(u => u.role == "ADMINISTRATOR").ToList();
        _nonAdmins = _allUsersCache.Where(u => u.role != "ADMINISTRATOR").ToList();
        
        // Desenha a lista de admins atuais na UI
        foreach (var admin in currentAdmins)
        {
            CreateAdminCardUI(admin);
        }
        
        SetLoading(false);
    }
    
    /// <summary>
    /// Filtra a lista de usuários que NÃO são admins para o autocomplete.
    /// </summary>
    private void HandleSearchQueryChanged(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            HideAutocompleteResults();
            return;
        }

        var filteredUsers = _nonAdmins
            .Where(u => u.email.ToLower().Contains(query.ToLower()) && !_currentCardsOnScreen.ContainsKey(u.id))
            .Take(5)
            .ToList();
        
        DisplayAutocompleteResults(filteredUsers);
    }

    /// <summary>
    /// Chamado quando uma sugestão do autocomplete é selecionada.
    /// </summary>
    private void HandleSuggestionSelected(UserData userToAdd)
    {
        if (userToAdd == null) return;

        // LÓGICA DE CANCELAMENTO: Se este usuário estava marcado para ser rebaixado,
        // simplesmente cancelamos essa ação e não fazemos mais nada.
        if (_adminsToDemote.Remove(userToAdd.id))
        {
            Debug.Log($"Ação de rebaixamento para {userToAdd.name} foi cancelada.");
        }
        else
        {
            // Se não for um cancelamento, adiciona para a lista de promoção.
            _usersToPromote.Add(userToAdd.id);
            Debug.Log($"{userToAdd.name} adicionado à lista de promoção.");
        }
        
        // Adiciona o card dele na UI (seja um cancelamento ou uma nova adição)
        CreateAdminCardUI(userToAdd);
    }
    
    /// <summary>
    /// Chamado pelo clique no botão de um card de admin.
    /// </summary>
    private void HandleDemoteAdmin(UserData adminToHandle)
    {
        // LÓGICA DE CANCELAMENTO: Se este usuário foi recém-adicionado para promoção,
        // simplesmente cancelamos a promoção.
        if (_usersToPromote.Remove(adminToHandle.id))
        {
            Debug.Log($"Ação de promoção para {adminToHandle.name} foi cancelada.");
        }
        else
        {
            // Se não, marca para rebaixamento
            _adminsToDemote.Add(adminToHandle.id);
            Debug.Log($"{adminToHandle.name} marcado para ser rebaixado.");
        }
        
        // Em ambos os casos, o card some da tela
        if (_currentCardsOnScreen.TryGetValue(adminToHandle.id, out AdminCard cardToDestroy))
        {
            Destroy(cardToDestroy.gameObject);
            _currentCardsOnScreen.Remove(adminToHandle.id);
        }
    }

    /// <summary>
    /// Chamado pelo botão "Confirmar Alterações". Envia tudo para a API.
    /// </summary>
    public async void OnConfirmChangesClicked()
    {
        SetLoading(true);
        var tasks = new List<Task>();

        Debug.Log($"Promovendo {_usersToPromote.Count} usuários...");
        foreach (var userId in _usersToPromote)
        {
            tasks.Add(_adminApiService.SetUserRoleAsync(userId, "ADMINISTRATOR"));
        }

        Debug.Log($"Rebaixando {_adminsToDemote.Count} usuários...");
        foreach (var userId in _adminsToDemote)
        {
            tasks.Add(_adminApiService.SetUserRoleAsync(userId, "GUEST"));
        }

        await Task.WhenAll(tasks);
        Debug.Log("Alterações de administradores foram processadas!");
        
        // Recarrega tudo do zero para mostrar o estado final e limpo
        await InitializePanel();
    }
    
    // --- Métodos Auxiliares de UI ---

    private void DisplayAutocompleteResults(List<UserData> filteredUsers)
    {
        foreach (var card in _autocompleteCards) Destroy(card.gameObject);
        _autocompleteCards.Clear();
        autocompleteContainer.SetActive(filteredUsers.Any());

        foreach (var user in filteredUsers)
        {
            AutoCompleteCard newCard = Instantiate(autocompleteCardPrefab, autocompleteContainer.transform);
            newCard.gameObject.SetActive(true);
            newCard.Data = user.email;
            newCard.OnCardClicked += (selectedCard) => {
                HandleSuggestionSelected(user);
                userSearchInput.text = "";
                HideAutocompleteResults();
            };
            _autocompleteCards.Add(newCard);
        }
    }

    private void CreateAdminCardUI(UserData user)
    {
        if (_currentCardsOnScreen.ContainsKey(user.id)) return;

        AdminCard newCard = Instantiate(adminCardPrefab, adminListContainer);
        newCard.gameObject.SetActive(true);
        newCard.Data = user;
        newCard.OnActionClicked += HandleDemoteAdmin;
        _currentCardsOnScreen.Add(user.id, newCard);
    }
    
    private void ClearAndDestroyAllCards()
    {
        foreach (var card in _currentCardsOnScreen.Values)
        {
            if(card != null) Destroy(card.gameObject);
        }
        _currentCardsOnScreen.Clear();
    }
    
    private void HideAutocompleteResults()
    {
        foreach (var card in _autocompleteCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        _autocompleteCards.Clear();
        if(autocompleteContainer != null) autocompleteContainer.SetActive(false);
    }

    public void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(isLoading);
    }
}