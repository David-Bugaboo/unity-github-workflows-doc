using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion.Addons.ConnectionManagerAddon;
using Photon.Realtime;
using UnityEngine;
using PhotonAppSettings = Fusion.Photon.Realtime.PhotonAppSettings;

public class ChatSceneController : MonoBehaviour
{
    [Header("Referências da Cena")]
    [SerializeField] private ChatService chatService; 

    private void Start()
    {
        if (chatService == null)
        {
            Debug.LogError("ChatService não foi atribuído no Inspector!", this);
            return;
        }
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogError("UserManager não está pronto ou o usuário não está logado!", this);
            return;
        }
        
        IniciarConexaoDoChat();
    }

    private void IniciarConexaoDoChat()
    {
        var fusionSettings = Resources.Load<PhotonAppSettings>("PhotonAppSettings");
        if (fusionSettings == null || string.IsNullOrEmpty(fusionSettings.AppSettings.AppIdChat))
        {
            Debug.LogError("PhotonAppSettings não encontrado na pasta Resources ou o AppId do Chat não está configurado!");
            return;
        }
        
        var chatSettings = new Photon.Chat.ChatAppSettings
        {
            AppIdChat = fusionSettings.AppSettings.AppIdChat
        };
        
        chatService.OnConnectedToChat += InscreverNosCanais;
        
        string userName = UserManager.Instance.CurrentUser.name;
        chatService.Connect(userName, chatSettings);
    }


    private void InscreverNosCanais()
    {
        if(chatService != null)
        {
            chatService.OnConnectedToChat -= InscreverNosCanais;
        }

        // 4. Monta a lista de canais e manda o serviço se inscrever.
        List<string> canaisParaEntrar = new List<string> { "Lobby" };
        var usuarioLogado = UserManager.Instance.CurrentUser;

        var connection = FindFirstObjectByType<ConnectionManager>();
        if (connection != null)
        {
            if(connection.roomName != "LobbyID") canaisParaEntrar.Add(connection.roomName);
        }
        
        if (canaisParaEntrar.Count > 0 && chatService != null)
        {
            chatService.Subscribe(canaisParaEntrar.ToArray());
        }
    }
}