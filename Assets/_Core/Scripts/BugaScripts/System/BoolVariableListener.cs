using UnityEngine;

public class BoolVariableListener : VariableListener<bool>
{
    public bool invertBool = false;
    protected override void OnValueChanged()
    {
        Debug.Log($"Raised event ({name}) with value {variable.GetValue()}");
        onChangeEvent.Invoke(invertBool? !variable.GetValue() : variable.GetValue());
    }
}
