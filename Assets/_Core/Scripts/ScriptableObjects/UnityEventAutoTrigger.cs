using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class UnityEventAutoTriggerDefinition
{
    public UnityEvent eventToTrigger;
    [Space]
    public bool triggerOnAwake;
    public bool triggerOnStart;
    public bool triggerOnEnable;
    public bool triggerOnDisable;
}

public class UnityEventAutoTrigger : MonoBehaviour
{
    public UnityEventAutoTriggerDefinition[] definitions;

    private void Awake()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnStart)
            {
                item.eventToTrigger.Invoke();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnStart)
            {
                item.eventToTrigger.Invoke();
            }
        }
    }

    private void OnEnable()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnEnable)
            {
                item.eventToTrigger.Invoke();
            }
        }
    }

    private void OnDisable()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnDisable)
            {
                item.eventToTrigger.Invoke();
            }
        }
    }
}
