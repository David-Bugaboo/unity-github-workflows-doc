using UnityEngine;
using UnityEngine.Events;

public class VariableListener<T> : MonoBehaviour
{
    public GlobalVariable<T> variable;
    public UnityEvent<T> onChangeEvent;

    [SerializeField] protected T debugValue => variable.GetValue();

    protected virtual void OnValueChanged()
    {
        Debug.Log($"Raised event ({name}) with value {variable.GetValue()}");
        onChangeEvent.Invoke(variable.GetValue());
    }

    private void Start()
    {
        previousValue = variable.GetValue();
    }

    protected T previousValue;

    private void Update()
    {
        if (Compare())
        {
            OnValueChanged();
            previousValue = variable.GetValue();
        }
    }

    public virtual bool Compare()
    {
        return !previousValue.Equals(variable.GetValue());
    }
}