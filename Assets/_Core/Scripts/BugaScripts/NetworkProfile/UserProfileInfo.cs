using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class UserProfileInfo : NetworkBehaviour
{
    [Networked]
    [OnChangedRender(nameof(OnDescriptionChange)), Capacity(128)]
    public string Description { get; set; }

    [Networked]
    [OnChangedRender(nameof(OnMainInterestChange)), Capacity(16)]
    public string MainInterest { get; set; }

    [Networked]
    [OnChangedRender(nameof(OnInterestsChange)), Capacity(16*4+3)]
    public string Interests { get; set; }


    [Header("Events")]
    public UnityEvent onDescriptionChange;
    public UnityEvent onMainInterestChange;
    public UnityEvent onInterestsChange;
    
    public void OnDescriptionChange()
    {
        Debug.Log("[UserInfo] Description changed: " + Description);
        onDescriptionChange?.Invoke();
    }

    public void OnMainInterestChange()
    {
        Debug.Log("[UserInfo] Main Interest changed: " + MainInterest);
        onMainInterestChange?.Invoke();
    }
    
    public void OnInterestsChange()
    {
        Debug.Log("[UserInfo] Interests changed: " + Interests);
        onInterestsChange?.Invoke();
    }
}