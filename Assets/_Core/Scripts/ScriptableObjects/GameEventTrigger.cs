using UnityEngine;

[System.Serializable]
public class GameEventTriggerDefinition
{
    public GameEvent eventToTrigger;
    [Space]
    public bool triggerOnAwake;
    public bool triggerOnStart;
    public bool triggerOnEnable;
    public bool triggerOnDisable;
}


public class GameEventTrigger : MonoBehaviour
{
    public GameEventTriggerDefinition[] definitions;

    private void Awake()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnStart)
            {
                item.eventToTrigger.Raise();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in definitions)
        {
            if(item.triggerOnStart)
            {
                item.eventToTrigger.Raise();
            }
        }
    }

    private void OnEnable()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnEnable)
            {
                item.eventToTrigger.Raise();
            }
        }
    }

    private void OnDisable()
    {
        foreach (var item in definitions)
        {
            if (item.triggerOnDisable)
            {
                item.eventToTrigger.Raise();
            }
        }
    }
}
