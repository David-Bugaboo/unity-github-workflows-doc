using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Addons.ConnectionManagerAddon;
using Fusion.Addons.Spaces;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BugaScreenShareManager : MonoBehaviour
{
    [Header("Configuração do Executável")]
    [SerializeField]
    private string screenShareExePath = "screen_share\\Mundo IEL.exe";

    [Header("UI")]
    [SerializeField] private TMP_Text btnLabel;
    [SerializeField] private string startShareText = "Iniciar Compartilhamento";
    [SerializeField] private string stopShareText = "Parar Compartilhamento";

    private Process screenShareProcess;
    private void Start()
    {
        UpdateButtonUI();
    }
    
    public void ToggleScreenShare()
    {
        if (screenShareProcess == null)
        {
            StartShareApplication();
        }
        else
        {
            StopShareApplication();
        }
    }

    public void StartShareApplication()
{
    if (screenShareProcess != null)
    {
        Debug.LogWarning("Aplicação de compartilhamento já está em execução.");
        return;
    }

    var spaceRoom = FindFirstObjectByType<SpaceRoom>();
    if (spaceRoom == null)
    {
        Debug.LogError("SpaceRoom não foi encontrado. Não é possível obter os dados da sala.");
        return;
    }

    // --- LÓGICA DE ARGUMENTOS ATUALIZADA ---
    // Usamos um construtor de string para montar os argumentos de forma segura
    var arguments = new System.Text.StringBuilder();

    // Adiciona o spaceId (obrigatório)
    arguments.Append($"--spaceId \"{spaceRoom.spaceId}\"");

    // Adiciona o groupId APENAS se ele não for nulo ou vazio
    if (!string.IsNullOrEmpty(spaceRoom.groupId))
    {
        arguments.Append($" --groupId \"{spaceRoom.groupId}\"");
    }

    // Adiciona o instanceId APENAS se ele não for nulo ou vazio
    if (!string.IsNullOrEmpty(spaceRoom.instanceId))
    {
        arguments.Append($" --instanceId \"{spaceRoom.instanceId}\"");
    }

    string finalArguments = arguments.ToString();
    // --- FIM DA LÓGICA ATUALIZADA ---

    try
    {
        string dirPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string exePath = Path.Combine(dirPath, screenShareExePath);
        ProcessStartInfo startInfo = new ProcessStartInfo(exePath);

        startInfo.Arguments = finalArguments; // Usa a string segura que montamos
        Debug.Log($"[ScreenShare] Iniciando com os argumentos: {startInfo.Arguments}");
        
        // ... resto do seu código para iniciar o processo
        screenShareProcess = new Process { StartInfo = startInfo };
        screenShareProcess.EnableRaisingEvents = true;
        screenShareProcess.Exited += OnProcessExited;
        screenShareProcess.Start();

        Debug.Log($"[ScreenShare] Aplicação iniciada com sucesso. Process ID: {screenShareProcess.Id}");
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Falha ao iniciar a aplicação de compartilhamento: {ex.Message}");
        screenShareProcess = null;
    }
    finally
    {
        UpdateButtonUI();
    }
}

    public void StopShareApplication()
    {
        if (screenShareProcess == null) return;
        try
        {
            if (!screenShareProcess.HasExited)
            {
                screenShareProcess.CloseMainWindow();

                // Aguarda um curto período para o processo fechar graciosamente
                if (!screenShareProcess.WaitForExit(1000))
                {
                    // Se não fechou, força o encerramento
                    screenShareProcess.Kill();
                    Debug.Log("[ScreenShare] Processo forçado a encerrar.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ScreenShare] Erro ao fechar processo: {ex.Message}");
        }
        finally
        {
            screenShareProcess = null;
            UpdateButtonUI();
        }
    }

    private void OnProcessExited(object sender, System.EventArgs e)
    {
        screenShareProcess = null;
    }

    void Update()
    {
        UpdateButtonUI();
    }

    private void UpdateButtonUI()
    {
        if (btnLabel == null) return;
        btnLabel.text = (screenShareProcess != null) ? stopShareText : startShareText;
    }

    private void OnDisable()
    {
        StopShareApplication();
    }
}
