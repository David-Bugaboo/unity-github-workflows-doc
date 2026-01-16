using System;
using UnityEngine;
using UnityEngine.UI;

public class InterestUIItem : MonoBehaviour {
    public string Interest;
    [SerializeField] Toggle toggle;
    internal event Action<InterestUIItem, bool> OnToggleEvent;
    private void Awake() => toggle.onValueChanged.AddListener( ToggleInterest );
    public void ToggleInterest( bool tgl ) => OnToggleEvent?.Invoke( this, tgl );
    public void SetActive( bool active ) => toggle.interactable = active || toggle.isOn;
    public void ForceToggle( bool val ) => toggle.isOn = val;
}