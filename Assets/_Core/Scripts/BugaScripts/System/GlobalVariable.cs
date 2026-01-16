using System;
using UnityEngine;

public abstract class GlobalVariable<T> : ScriptableObject, ISerializationCallbackReceiver
{
    public T InitialValue;

    [NonSerialized]
    protected T RuntimeValue;

    public T GetValue()
    {
        return RuntimeValue;
    }

    public void SetValue(T value)
    {
        Debug.Log($"Setting value of {name} to {value}");
        RuntimeValue = value;
        Debug.Log($"Value of {name} setted to {RuntimeValue}");
    }

    public void OnAfterDeserialize()
    {
        RuntimeValue = InitialValue;
    }

    public void OnBeforeSerialize() { }
}