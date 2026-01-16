using System.Collections.Generic;
using UnityEngine;

public class AdminPanelCoordinator : MonoBehaviour
{
    [Header("Painéis do Fluxo")]
    [SerializeField] private GameObject eventListPanel;
    [SerializeField] private GameObject eventDetailsPanel;
    [SerializeField] private GameObject addPhotoPanel;
    [SerializeField] private GameObject addMembersPanel;

    // Referências aos controllers de cada painel (arraste os GameObjects no Inspector)
    [SerializeField] private EventListController eventListController;
    [SerializeField] private EventPanelController eventDetailsController;
    // Adicione referências para os controllers dos outros painéis se eles tiverem lógica complexa

    // Armazena os dados do evento durante o processo de criação
    private EventPayload _eventBeingCreated;
    private List<string> _membersToInvite;

    private EventApiService _apiService;

    void Start()
    {
        // Inicializa o serviço de API
        _apiService = new EventApiService(APIManager.Instance);
        
        // Garante que apenas o painel inicial esteja visível
        GoToEventList();
    }

    public void GoToEventList()
    {
        eventListPanel.SetActive(true);
        eventDetailsPanel.SetActive(false);
        addPhotoPanel.SetActive(false);
        addMembersPanel.SetActive(false);
        // Opcional: dizer ao eventListController para recarregar a lista
    }

    // -- FLUXO DE CRIAÇÃO --

    // Chamado pelo EventListPresenter quando o usuário clica em "Criar Novo"
    public void StartCreateEventFlow()
    {
        _eventBeingCreated = new EventPayload(); // Limpa/cria um novo payload
        
        eventListPanel.SetActive(false);
        eventDetailsPanel.SetActive(true);
        addPhotoPanel.SetActive(false);
        addMembersPanel.SetActive(false);
    }

    // Chamado pelo EventPanelPresenter (painel de detalhes) quando o usuário clica em "Avançar"
    public void ProceedToPhotoStep(EventPayload detailsPayload)
    {
        // Salva os dados da primeira etapa
        _eventBeingCreated.name = detailsPayload.name;
        _eventBeingCreated.description = detailsPayload.description;
        _eventBeingCreated.start_date = detailsPayload.start_date;
        _eventBeingCreated.end_date = detailsPayload.end_date;
        _eventBeingCreated.location = detailsPayload.location;
        _eventBeingCreated.visibility = detailsPayload.visibility;

        eventListPanel.SetActive(false);
        eventDetailsPanel.SetActive(false);
        addPhotoPanel.SetActive(true);
        addMembersPanel.SetActive(false);
    }
    
    // Chamado pelo painel de foto quando o usuário clica em "Avançar"
    public void ProceedToMembersStep(BannerPayload bannerPayload)
    {
        // Salva os dados da segunda etapa
        _eventBeingCreated.banner = bannerPayload;

        eventListPanel.SetActive(false);
        eventDetailsPanel.SetActive(false);
        addPhotoPanel.SetActive(false);
        addMembersPanel.SetActive(true);
    }

    // Chamado pelo painel de membros quando o usuário clica em "Finalizar"
    public async void FinishCreateEventFlow(List<string> memberEmails)
    {
        _membersToInvite = memberEmails;

        // Agora temos todos os dados! É hora de chamar a API.
        Debug.Log("Finalizando criação. Enviando para a API...");
        EventData createdEvent = await _apiService.CreateEventAsync(_eventBeingCreated);

        if (createdEvent != null)
        {
            Debug.Log($"Evento {createdEvent.id} criado com sucesso!");
            if (_membersToInvite != null && _membersToInvite.Count > 0)
            {
                await _apiService.AddMembersAsync(createdEvent.id, _membersToInvite);
                Debug.Log("Membros adicionados!");
            }
            
            // Sucesso! Volta para a lista de eventos.
            GoToEventList();
        }
        else
        {
            Debug.LogError("Falha ao criar evento na API.");
            // Opcional: Mostrar um pop-up de erro e deixar o usuário tentar de novo.
        }
    }
    
    public void CancelFlow()
    {
        // Se o usuário clicar em "Cancelar" em qualquer etapa
        _eventBeingCreated = null;
        _membersToInvite = null;
        GoToEventList();
    }
}