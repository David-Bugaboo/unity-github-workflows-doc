// using UnityEngine;
//
// public class AdminPanelController : MonoBehaviour
// {
//     [Header("Referências das Views")]
//     [SerializeField] private EventListView eventListView;
//     [SerializeField] private EditEventPanel editEventPanel;
//     // Adicione outras views aqui (ex: UserListView)
//
//     private void Start()
//     {
//         // Conecta os eventos entre as views
//         eventListView.OnEventSelectedForEdit += HandleEventSelectedForEdit;
//         editEventPanel.OnActionComplete += HandleEditActionComplete;
//
//         // Esconde o painel de edição e carrega a lista inicial
//         editEventPanel.gameObject.SetActive(false);
//         RefreshEventList();
//     }
//
//     private void OnDestroy()
//     {
//         // Limpa as inscrições dos eventos
//         eventListView.OnEventSelectedForEdit -= HandleEventSelectedForEdit;
//         editEventPanel.OnActionComplete -= HandleEditActionComplete;
//     }
//
//     // Método chamado pelo botão "Novo Evento"
//     public void CreateNewEvent()
//     {
//         eventListView.gameObject.SetActive(false);
//         editEventPanel.gameObject.SetActive(true);
//         editEventPanel.Show(null); // Passa null para indicar modo de "criação"
//     }
//
//     // Chamado quando um card de evento é clicado na lista
//     private void HandleEventSelectedForEdit(EventData eventData)
//     {
//         eventListView.gameObject.SetActive(false);
//         editEventPanel.gameObject.SetActive(true);
//         editEventPanel.Show(eventData); // Passa os dados do evento para o painel de edição
//     }
//
//     // Chamado quando o painel de edição termina sua ação (salvar, deletar)
//     private void HandleEditActionComplete()
//     {
//         editEventPanel.gameObject.SetActive(false);
//         eventListView.gameObject.SetActive(true);
//         RefreshEventList(); // Atualiza a lista com os novos dados
//     }
//
//     private async void RefreshEventList()
//     {
//         // Mostra um loading...
//         var events = await EventAdminService.GetAllEventsAsync();
//         eventListView.LoadData(events.ToArray());
//         // Esconde o loading...
//     }
// }