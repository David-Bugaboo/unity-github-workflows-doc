using System;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Card_EventView : UI_Card<EventData>
{
    [SerializeField] private ChangeSceneFromButton _changeSceneFromButton;
    [SerializeField] TextMeshProUGUI hour, date, eventName, description;
    [SerializeField] Button visitButton;

    static bool working = false;

    public const string GROUP_ID = "SPACES_NAVIGATION_GROUPID";
    public bool isEventRunning => PlayerPrefs.GetString(GROUP_ID) == Data.id;

    public UnityEvent onBeginEnterEvent;

    private void Start()
    {
        visitButton.onClick.AddListener(ChangeScene);
    }

    private void OnDestroy()
    {
        visitButton.onClick.RemoveListener(ChangeScene);
    }

    public void ChangeScene()
    {
        _changeSceneFromButton.JoinServer(Data.name, Data.location);
    }

    async void EnterEventCoroutine()
    {
        if (working) { return; }
        working = true;
            
        onBeginEnterEvent.Invoke();
        await Task.Delay(1000);

        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            await runner.Shutdown(true);
        }

        working = false;
        SceneManager.LoadScene(Data.location);
        //LoadSceneAssetBundler.StartSceneLoading(Data.GroupData.Auditorio, false); //Metodo para carregar cena por AssetBundle
    }
               
    protected override void OnDataSet() {
        if ( Data == null) return;
        var eventDate = Data.StartDateAsDateTime;
        eventName.text = Data.name;
        description.text = Data.description;
        hour.text = eventDate.ToString( "HH:mm" );
        date.text = $"{eventDate.Day}/{eventDate.Month}";
    }
}