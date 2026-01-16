using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerStatePanel : MonoBehaviour
{
    [SerializeField, Tooltip("The prefab instantiated when a new runner has been created.")]
    RunnerStateItem prefab;

    [SerializeField, Tooltip("The scroll rect that will contain newly created items for this panel.")]
    Transform scrollRectContent;

    [Tooltip("Reference to the CanvasGroup component that is enabled once runners have been added.")]
    public CanvasGroup mainGroup;

    public void AddRunner(NetworkRunner newRunner)
    {
        mainGroup.interactable = true;
        mainGroup.alpha = 1f;

        RunnerStateItem runnerItem = Instantiate(prefab, scrollRectContent).GetComponent<RunnerStateItem>();

        // We set the visiblity and input based on whether or not this is the first child in the set.
        bool isFirstChild = scrollRectContent.childCount == 1;
        runnerItem.SupplyRunner(newRunner, isFirstChild, isFirstChild);
    }
}
