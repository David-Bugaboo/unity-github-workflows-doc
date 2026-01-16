using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageCard : UI_Card<ChatMessageData>
{
    [Header("UI References")]
    [SerializeField] private RawImage avatar;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text timeText;
    
    // Opcional: Para links clicáveis em mensagens de administradores
    // [SerializeField] private LinkOpener linkOpener; 

    [Header("Styling")]
    [Tooltip("Ex: O painel de fundo da bolha de chat.")]
    [SerializeField] private Image background;
    [SerializeField] private Color myMessageColor = Color.cyan;
    [SerializeField] private Color otherMessageColor = Color.white;
    [SerializeField] private HorizontalLayoutGroup layoutGroup; // Para alinhar à direita/esquerda

    private void Awake()
    {
        ClearTexts();
    }

    private void ClearTexts()
    {
        if (messageText != null) messageText.text = string.Empty;
        if (senderText != null) senderText.text = string.Empty;
        if (timeText != null) timeText.text = string.Empty;
    }

    /// <summary>
    /// Este método é chamado automaticamente sempre que a propriedade 'Data' do card é alterada.
    /// </summary>
    protected override void OnDataSet()
    {
        if (Data == null || Data.Sender == null) return;

        // 1. Preenche os textos com os dados da mensagem
        senderText.text = Data.Sender.Name;
        messageText.text = Data.Message;
        timeText.text = Data.SentAtTime;

        // 2. Carrega o avatar usando o nosso serviço centralizado
        // Removemos a lógica complexa de '_userMap' e centralizamos no AvatarLoaderService.
        if (avatar != null && !string.IsNullOrEmpty(Data.Sender.AvatarUrl))
        {
            AvatarLoaderService.Instance.RequestAvatar(Data.Sender.AvatarUrl, avatar);
        }

        // 3. Lógica para links (se aplicável)
        // if (linkOpener != null && Data.Sender.Role == "Admin")
        // {
        //     linkOpener.GetLinks();
        // }

        // 4. Lógica para estilizar as mensagens do próprio usuário
        // Em vez de simplesmente parar a execução, agora estilizamos o card de forma diferente.
        bool isMyMessage = (ChatService.Instance.CurrentUserId == Data.Sender.Name);
        
        if (background != null)
        {
            background.color = isMyMessage ? myMessageColor : otherMessageColor;
        }

        if (layoutGroup != null)
        {
            // Alinha as próprias mensagens à direita e as dos outros à esquerda
            layoutGroup.childAlignment = isMyMessage ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        }
    }

    /// <summary>
    /// Validação para ativar/desativar o card de forma segura.
    /// </summary>
    public override void ValidateCard(bool active)
    {
        bool hasData = Data != null && Data.Sender != null && !string.IsNullOrEmpty(Data.Sender.Name);
        gameObject.SetActive(active && hasData);
    }
}