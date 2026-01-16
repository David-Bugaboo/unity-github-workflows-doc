using Fusion;
using Fusion.Statistics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunnerStateItem : MonoBehaviour
{
    [SerializeField, Tooltip("Text label that displays the runner and PlayerRef that this item is associated with.")]
    private TextMeshProUGUI label = null;

    [SerializeField, Tooltip("Reference to ths image that displays when the associated NetworkRunner is visible.")]
    private Image visibleImage = null;

    [SerializeField, Tooltip("Reference to ths image that displays when the associated NetworkRunner is not visible.")]
    private Image notVisibleImage = null;

    [SerializeField, Tooltip("Reference to ths image that displays when the associated NetworkRunner is providing input.")]
    private Image inputImage = null;

    [SerializeField, Tooltip("Reference to ths image that displays when the associated NetworkRunner is not providing input.")]
    private Image noInputImage = null;

    /// <summary>
    /// The previous visibility; will update this item's visuals if the current NetworkRunner's visibility state has changed.
    /// </summary>
    private bool? previousVisibility = null;

    /// <summary>
    /// The previous visibility; will update this item's visuals if the current NetworkRunner's input state has changed.
    /// </summary>
    private bool? previousInput = null;

    /// <summary>
    /// Reference to this item's transform.
    /// </summary>
    private Transform _transform = null;

    /// <summary>
    /// If true, this means a NetworkRunner has been assigned.
    /// </summary>
    private bool hasNetworkRunner = false;

    /// <summary>
    /// The NetworkRunner monitoried by this UI item.
    /// </summary>
    private NetworkRunner runner = null;

    private void Awake()
    {
        _transform = transform;
    }

    public void SupplyRunner(NetworkRunner suppliedRunner, bool visible, bool provideInput)
    {
        runner = suppliedRunner;
        hasNetworkRunner = runner != null;

        if (hasNetworkRunner)
        {
            Debug.Log(runner + ":  NDFS");

            runner.SetVisible(visible);
            runner.ProvideInput = provideInput;

            label.text = suppliedRunner.IsClient ? "Client " + suppliedRunner.LocalPlayer : "Host " + suppliedRunner.LocalPlayer;
        }
        else
        {
            Debug.LogWarning("A null NetworkRunner was supplied.");
        }
    }

    public bool GetRunner(out NetworkRunner runner)
    {
        runner = this.runner;
        return hasNetworkRunner;
    }

    /// <summary>
    /// Updates the state of this item.  Done through an update loop since runner visibility and input can be changed within the editor as well as this UI.
    /// </summary>
    public void Update()
    {
        bool runnerIsVisible = hasNetworkRunner && runner.GetVisible();
        if (previousVisibility != runnerIsVisible)
        {
            visibleImage.enabled = runnerIsVisible;
            notVisibleImage.enabled = !runnerIsVisible;

            previousVisibility = runnerIsVisible;
        }

        bool runnerIsProvidingInput = hasNetworkRunner && runner.ProvideInput;
        if (previousInput != runnerIsProvidingInput)
        {
            inputImage.enabled = runnerIsProvidingInput;
            noInputImage.enabled = !runnerIsProvidingInput;

            previousInput = runnerIsProvidingInput;
        }
    }

    public void ToggleInput()
    {
        if (!GetRunner(out NetworkRunner runner))
            return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            var items = _transform.parent.GetComponentsInChildren<RunnerStateItem>(true);
            foreach (var item in items)
            {
                if (item.GetInstanceID() == GetInstanceID())
                    continue;
                item.TurnOffInput();
            }
            runner.ProvideInput = true;
        }
        else
        {
            runner.ProvideInput = !runner.ProvideInput;
        }
    }
    public void ToggleVisbility()
    {
        if (!GetRunner(out NetworkRunner runner))
            return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            var items = _transform.parent.GetComponentsInChildren<RunnerStateItem>(true);
            foreach (var item in items)
            {
                if (item.GetInstanceID() == GetInstanceID())
                    continue;
                item.TurnOffVisibility();
            }
            runner.SetVisible(true);
        }
        else
        {
            runner.SetVisible(!runner.GetVisible());
        }
    }

    public void TurnOffVisibility()
    {
        if (!GetRunner(out NetworkRunner runner))
            return;

        runner.SetVisible(false);
    }

    public void TurnOffInput()
    {
        if (!GetRunner(out NetworkRunner runner))
            return;

        runner.ProvideInput = false;
    }

    public void ShowStats()
    {
        if (!GetRunner(out NetworkRunner runner))
            return;

        FusionStatistics stats = runner.GetComponent<FusionStatistics>();
        if (stats == null)
            return;

        stats.enabled = true;
        stats.SetupStatisticsPanel();
        //FusionStats.Create(runner: runner, screenLayout: FusionStats.DefaultLayouts.Left);
    }
}