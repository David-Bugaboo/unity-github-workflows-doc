using Fusion.Addons.ConnectionManagerAddon;
using TMPro;
using UnityEngine;

public class ServerDebug : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI serverName;

    public void ShowServer()
    {
        var more = PlayerPrefs.GetString("SPACES_NAVIGATION_GROUPID");
        serverName.text = FindFirstObjectByType<ConnectionManager>().roomName;
        serverName.text += $"\n{more}";
    }
}
