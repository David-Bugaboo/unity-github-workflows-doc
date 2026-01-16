using Fusion;
using Fusion.Menu;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : FusionMenuUIScreen
{
    [Tooltip("The network runner prefab.")]
    public NetworkRunner aoiRunnerPrefab;

    [Tooltip("The name of the scene to load when creating the room.")]
    public string aoiSceneName;

    public GameObject mainPanel;

    public RunnerStatePanel runnerPanel;

    [Tooltip("ScriptableObject that contains settings for the AoI and other aspects of this sample")]
    public AoIMPSettings settings;

    [SerializeField]
    TMP_Dropdown modeDropdown;

    [SerializeField]
    TMP_InputField characterCountInput;

    [SerializeField]
    TMP_InputField aoiRadiusInput;

    [SerializeField]
    TMP_InputField aoiCellInput;

    [SerializeField]
    TMP_InputField aoiGridXInput;

    [SerializeField]
    TMP_InputField aoiGridYInput;

    [SerializeField]
    TMP_InputField aoiGridZInput;

    [SerializeField]
    Toggle aoiServerZoneDisplayOnToggle;

    [SerializeField]
    Toggle aoiServerZoneDisplayOffToggle;

    [SerializeField]
    Toggle aoiPlayerInterestDisplayOnToggle;

    [SerializeField]
    Toggle aoiPlayerInterestDisplayOffToggle;

    [SerializeField]
    Animator uiSectionAnimator;

    public override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;

        modeDropdown.value = settings.currentGameMode == GameMode.Shared ? 0 : 1;

        SetPlayerCount(settings.PlayersToSpawn.ToString());
        
        SetAreaOfInterestRadius(settings.AreaOfInterestRadius.ToString());
        
        SetAreaOfInterestCellSize(settings.AreaOfInterestCellSize.ToString());
        
        SetAreaOfInterestGridX(settings.AreaOfInterestGrid.x.ToString());
        SetAreaOfInterestGridY(settings.AreaOfInterestGrid.y.ToString());
        SetAreaOfInterestGridZ(settings.AreaOfInterestGrid.z.ToString());

        SetPlayerRadiusDisplayOn(settings.ShowPlayerRadius);
        SetPlayerRadiusDisplayOff(!settings.ShowPlayerRadius);

        SetServerZoneDisplayOn(settings.ShowActiveServerZones);
        SetServerZoneDisplayOn(!settings.ShowActiveServerZones);

        SetPlayerInterestDisplayOn(settings.ShowPlayerInterest);
        SetPlayerInterestDisplayOff(!settings.ShowPlayerInterest);

        SetAoIGridDisplayOn(settings.ShowAreaOfInterestGrid);
        SetAoIGridDisplayOff(!settings.ShowAreaOfInterestGrid);

    }

    public void SetPlayerCount(string s)
    {
        settings.PlayersToSpawn = SetInputValue(s, characterCountInput, settings.PlayersToSpawn, 1, 100);
    }

    public void SetAreaOfInterestRadius(string s)
    {
        settings.AreaOfInterestRadius = SetInputValue(s, aoiRadiusInput, settings.AreaOfInterestRadius, 0, settings.currentGameMode == GameMode.Shared ? 300 : 2048);
    }

    public void SetAreaOfInterestCellSize(string s)
    {
        settings.AreaOfInterestCellSize = SetInputValue(s, aoiCellInput, settings.AreaOfInterestCellSize, 0, 2048);
    }

    public void SetAreaOfInterestGridX(string s)
    {
        var aoig = settings.AreaOfInterestGrid;
        aoig.x = SetInputValue(s, aoiGridXInput, aoig.x, 0, 2048);
        settings.AreaOfInterestGrid = aoig;
    }

    public void SetAreaOfInterestGridY(string s)
    {
        var aoig = settings.AreaOfInterestGrid;
        aoig.y = SetInputValue(s, aoiGridYInput, aoig.y, 0, 2048);
        settings.AreaOfInterestGrid = aoig;
    }

    public void SetAreaOfInterestGridZ(string s)
    {
        var aoig = settings.AreaOfInterestGrid;
        aoig.z = SetInputValue(s, aoiGridZInput, aoig.z, 0, 2048);
        settings.AreaOfInterestGrid = aoig;
    }

    public int SetInputValue(string inString, TMP_InputField field, int inValue, int? min = null, int? max = null)
    {
        if (string.IsNullOrEmpty(inString) || !int.TryParse(inString, out int result))
        {
            field.text = inValue.ToString();
            return inValue;
        }

        if (min.HasValue && max.HasValue)
        {
            int clampedResult = Mathf.Clamp(result, min.Value, max.Value);
            if (clampedResult != result)
            {
                result = clampedResult;
                field.text = clampedResult.ToString();
            }
        }

        return result;
    }

    public void SetGameMode(int gameMode)
    {
        SetSharedMode(gameMode == 0);
        SetAutoHostOrClientMode(gameMode == 1);
    }

    public void SetUIScreenToMainMenu(bool value)
    {
        uiSectionAnimator.SetBool("MainMenu", value);
    }

    public void SetSharedMode(bool isTrue)
    {
        if (isTrue)
        {
            settings.currentGameMode = GameMode.Shared;

            aoiCellInput.interactable = false;
            aoiGridXInput.interactable = false;
            aoiGridYInput.interactable = false;
            aoiGridZInput.interactable = false;

            aoiCellInput.text = "32";

            aoiGridXInput.text = "1024";
            aoiGridYInput.text = "1024";
            aoiGridZInput.text = "1024";

            aoiServerZoneDisplayOffToggle.isOn = true;
            aoiServerZoneDisplayOffToggle.interactable = false;

            aoiServerZoneDisplayOnToggle.isOn = false;
            aoiServerZoneDisplayOnToggle.interactable = false;

            aoiPlayerInterestDisplayOffToggle.isOn = true;
            aoiPlayerInterestDisplayOffToggle.interactable = false;

            aoiPlayerInterestDisplayOnToggle.isOn = false;
            aoiPlayerInterestDisplayOnToggle.interactable = false;
        }
    }

    public void SetAutoHostOrClientMode(bool isTrue)
    {
        if (isTrue)
        {
            settings.currentGameMode = GameMode.AutoHostOrClient;

            aoiCellInput.interactable = true;
            aoiGridXInput.interactable = true;
            aoiGridYInput.interactable = true;
            aoiGridZInput.interactable = true;

            aoiServerZoneDisplayOffToggle.isOn = false;
            aoiServerZoneDisplayOffToggle.interactable = true;

            aoiServerZoneDisplayOnToggle.isOn = true;
            aoiServerZoneDisplayOnToggle.interactable = true;

            aoiPlayerInterestDisplayOffToggle.isOn = false;
            aoiPlayerInterestDisplayOffToggle.interactable = true;

            aoiPlayerInterestDisplayOnToggle.isOn = true;
            aoiPlayerInterestDisplayOnToggle.interactable = true;
        }
    }

    [ContextMenu("Find Plugins")]
    void FindPlugins()
    {
        List<FusionMenuScreenPlugin> plugins = new List<FusionMenuScreenPlugin>(GetComponentsInChildren<FusionMenuScreenPlugin>());
        foreach (var plugin in plugins)
        {
            if (this.Plugins.Contains(plugin))
                continue;
            this.Plugins.Add(plugin);
        }
    }

    public void SetPlayerRadiusDisplayOn(bool isActive)
    {
        if (isActive)
            settings.ShowPlayerRadius = true;
    }

    public void SetPlayerRadiusDisplayOff(bool isActive)
    {
        if (isActive)
            settings.ShowPlayerRadius = false;
    }

    public void SetAoIGridDisplayOn(bool isActive)
    {
        if (isActive)
            settings.ShowAreaOfInterestGrid = true;
    }

    public void SetAoIGridDisplayOff(bool isActive)
    {
        if (isActive)
            settings.ShowAreaOfInterestGrid = false;
    }

    public void SetPlayerInterestDisplayOn(bool isActive)
    {
        if (isActive)
            settings.ShowPlayerInterest = true;
    }

    public void SetPlayerInterestDisplayOff(bool isActive)
    {
        if (isActive)
            settings.ShowPlayerInterest = false;
    }

    public void SetServerZoneDisplayOn(bool isActive)
    {
        if (isActive)
            settings.ShowActiveServerZones = true;
    }

    public void SetServerZoneDisplayOff(bool isActive)
    {
        if (isActive)
            settings.ShowActiveServerZones = false;
    }

    public void TogglePanel()
    {
        Debug.Log("TOGLLGE");
        SetMainPanelActive(!mainPanel.activeSelf);
    }

    public void SetMainPanelActive(bool state)
    {
        mainPanel.SetActive(state);
    }

    public async void BeginMultipeerSession()
    {
        mainPanel.SetActive(false);

        for (int i = 0; i < settings.PlayersToSpawn; i++)
        {
            var newRunner = Instantiate(aoiRunnerPrefab);

            StartGameArgs startGameArgs = new StartGameArgs()
            {
                GameMode = settings.currentGameMode,
                PlayerCount = 100,
            };

            var result = await newRunner.StartGame(startGameArgs);

            if (!result.Ok)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            if (newRunner.IsSceneAuthority)
            {
                var loadSceneResult = newRunner.LoadScene(aoiSceneName, loadSceneMode: UnityEngine.SceneManagement.LoadSceneMode.Additive);

                await loadSceneResult;

                if (loadSceneResult.IsDone)
                {
                    Debug.Log("Scene Load Complete");
                }
            }

            runnerPanel.AddRunner(newRunner);
        }
    }

    public void OnSettingsButtonPressed()
    {
        Controller.Show<FusionMenuUISettings>();
    }
}
