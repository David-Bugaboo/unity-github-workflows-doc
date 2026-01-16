using UnityEngine;

public static class SessionPersistence
{
    // Chaves para salvar os dados
    private const string SESSION_NAME_KEY = "Recon_SessionName";
    private const string SCENE_NAME_KEY = "Recon_SceneName";
    private const string POS_X_KEY = "Recon_PosX";
    private const string POS_Y_KEY = "Recon_PosY";
    private const string POS_Z_KEY = "Recon_PosZ";
    private const string ROT_Y_KEY = "Recon_RotY"; // Salvaremos apenas a rotação Y por simplicidade

    // Flag estático para comunicar entre o ConnectionManager e o PlayerReconnectionHandler
    public static ReconnectionData DataToRestore;

    public static bool HasReconnectionData()
    {
        return PlayerPrefs.HasKey(SESSION_NAME_KEY);
    }

    public static void SaveData(string session, string scene, Vector3 pos, Quaternion rot)
    {
        PlayerPrefs.SetString(SESSION_NAME_KEY, session);
        PlayerPrefs.SetString(SCENE_NAME_KEY, scene);
        PlayerPrefs.SetFloat(POS_X_KEY, pos.x);
        PlayerPrefs.SetFloat(POS_Y_KEY, pos.y);
        PlayerPrefs.SetFloat(POS_Z_KEY, pos.z);
        PlayerPrefs.SetFloat(ROT_Y_KEY, rot.eulerAngles.y);
        PlayerPrefs.Save();
        Debug.Log($"Dados de reconexão salvos para a sessão: {session}");
    }

    public static ReconnectionData LoadData()
    {
        if (!HasReconnectionData()) return null;

        return new ReconnectionData
        {
            SessionName = PlayerPrefs.GetString(SESSION_NAME_KEY),
            SceneName = PlayerPrefs.GetString(SCENE_NAME_KEY),
            PlayerPosition = new Vector3(
                PlayerPrefs.GetFloat(POS_X_KEY),
                PlayerPrefs.GetFloat(POS_Y_KEY),
                PlayerPrefs.GetFloat(POS_Z_KEY)
            ),
            PlayerRotation = Quaternion.Euler(0, PlayerPrefs.GetFloat(ROT_Y_KEY), 0)
        };
    }

    public static void LoadScene()
    {
        PlayerPrefs.SetString(SESSION_NAME_KEY, "");
        PlayerPrefs.SetString(SCENE_NAME_KEY, "");
    }

    public static void ClearData()
    {
        PlayerPrefs.DeleteKey(SESSION_NAME_KEY);
        PlayerPrefs.DeleteKey(SCENE_NAME_KEY);
        PlayerPrefs.DeleteKey(POS_X_KEY);
        PlayerPrefs.DeleteKey(POS_Y_KEY);
        PlayerPrefs.DeleteKey(POS_Z_KEY);
        PlayerPrefs.DeleteKey(ROT_Y_KEY);
        Debug.Log("Dados de reconexão limpos.");
    }
}