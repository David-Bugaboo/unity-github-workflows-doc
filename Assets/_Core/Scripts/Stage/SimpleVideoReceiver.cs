using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

public class SimpleVideoReceiver : MonoBehaviour
{
    private Renderer screenRenderer;
    private Material screenMaterialInstance;
    private Texture originalTexture;
    
    private IVideoPlayer videoPlayer;
    private int frameCount = 0; // Contador para evitar spam no log do Update

    void Awake()
    {
        Debug.Log("[SimpleVideoReceiver] Awake: Inicializando o receptor de vídeo.");
        screenRenderer = GetComponent<Renderer>();
        if (screenRenderer == null)
        {
            Debug.LogError("[SimpleVideoReceiver] ERRO: Nenhum componente Renderer encontrado neste objeto!", this.gameObject);
            return;
        }
        
        screenMaterialInstance = screenRenderer.material;
        originalTexture = screenMaterialInstance.mainTexture;
        screenRenderer.enabled = false;
        Debug.Log("[SimpleVideoReceiver] Awake: Receptor inicializado e tela desligada.");
    }

    void Update()
    {
        // Para não poluir o console, vamos logar o estado a cada 60 frames (~1 segundo)
        frameCount++;
        bool shouldLog = (frameCount % 60 == 0);

        if (videoPlayer == null)
        {
            if (shouldLog) Debug.Log("[SimpleVideoReceiver] Update: Nenhum player de vídeo ativo. Procurando por um...");
            FindActiveVideoSpeaker();
        }
        else
        {
            try
            {
                var speakerComponent = videoPlayer as Component;
                if (speakerComponent == null || !speakerComponent.GetComponent<Speaker>().IsPlaying)
                {
                    if (shouldLog) Debug.Log("[SimpleVideoReceiver] Update: Player de vídeo encontrado, mas não está mais tocando. Limpando a tela.");
                    ClearScreen();
                    return;
                }

                Texture videoTexture = videoPlayer.PlatformView as Texture;
                if (videoTexture != null)
                {
                    if (screenMaterialInstance.mainTexture != videoTexture)
                    {
                        // Este log é importante, então o deixamos fora do contador de frames
                        Debug.Log("[SimpleVideoReceiver] Update: Aplicando nova textura de vídeo ao material do telão.");
                        screenMaterialInstance.mainTexture = videoTexture;
                    }
                }
                else
                {
                    if (shouldLog) Debug.LogWarning("[SimpleVideoReceiver] Update: Player de vídeo está ativo, mas a textura é nula. Aguardando primeiro frame...");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SimpleVideoReceiver] Update: Erro ao acessar o videoPlayer. Provavelmente foi destruído. Limpando... Erro: {ex.Message}");
                ClearScreen();
            }
        }
    }

    /// <summary>
    /// Procura por todos os Speakers e seleciona o primeiro que for um stream de VÍDEO ativo.
    /// </summary>
    private void FindActiveVideoSpeaker()
    {
        Speaker[] allSpeakers = FindObjectsOfType<Speaker>();
        if (allSpeakers.Length == 0) return;

        // Este log só aparecerá uma vez a cada 60 frames por causa do 'shouldLog' no Update
        Debug.Log($"[SimpleVideoReceiver] Find: Encontrados {allSpeakers.Length} speakers na cena. Verificando...");

        foreach (var speaker in allSpeakers)
        {
            if (speaker.IsLinked && speaker.IsPlaying)
            {
                Codec codec = speaker.RemoteVoice.VoiceInfo.Codec;
                if (codec == Codec.VideoVP8 || codec == Codec.VideoVP9 || codec == Codec.VideoH264)
                {
                    Debug.Log($"  - Encontrado Speaker de VÍDEO do Player {speaker.RemoteVoice.PlayerId}. Codec: {codec}. Tentando selecionar...");
                    IVideoPlayer player = speaker.GetComponent<IVideoPlayer>();
                    if (player != null)
                    {
                        Debug.Log($"[SimpleVideoReceiver] SUCESSO! Speaker de VÍDEO selecionado.");
                        videoPlayer = player;
                        screenRenderer.enabled = true;
                        return; // Sai do método pois já encontramos o que queríamos.
                    }
                    else
                    {
                        Debug.LogWarning($"[SimpleVideoReceiver] Speaker de vídeo do Player {speaker.RemoteVoice.PlayerId} não possui o componente IVideoPlayer.");
                    }
                }
                else
                {
                    Debug.Log($"  - Ignorando Speaker de ÁUDIO do Player {speaker.RemoteVoice.PlayerId}. Codec: {codec}.");
                }
            }
            else
            {
                Debug.Log($"  - Ignorando Speaker do Player {speaker.RemoteVoice.PlayerId} pois não está ligado ou não está tocando.");
            }
        }
    }

    /// <summary>
    /// Limpa a textura do telão e o desliga.
    /// </summary>
    private void ClearScreen()
    {
        Debug.Log("[SimpleVideoReceiver] ClearScreen: O stream de vídeo parou. Limpando a tela e resetando para procurar novamente.");
        if (screenMaterialInstance != null)
        {
            screenMaterialInstance.mainTexture = originalTexture;
        }
        if (screenRenderer != null)
        {
            screenRenderer.enabled = false;
        }
        videoPlayer = null; 
    }
}