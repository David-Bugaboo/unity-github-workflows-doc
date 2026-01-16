  using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InterestUIManager : MonoBehaviour
{
    [Header("Configurações de Seleção")]
    [SerializeField] private int minSelections = 4;
    [SerializeField] private int maxSelections = 4;

    [Header("Referências da UI")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private List<InterestUIItem> interests;
    
    private readonly List<InterestUIItem> _selectedInterests = new();

    [ContextMenu("Popular Interesses Automaticamente")]
    private void PopulateInterests() => interests = GetComponentsInChildren<InterestUIItem>().ToList();

    private void Awake()
    {
        foreach (var item in interests)
        {
            item.OnToggleEvent += ToggleInterest;
        }
    }
    
    public void InitializeWithManager()
    {
        _selectedInterests.Clear();
        foreach (var item in interests)
        {
            item.ForceToggle(false);
        }

        var userInterests = UserManager.Instance.CurrentUser?.InterestsAsArray;
        if (userInterests != null)
        {
            foreach (string interestKey in userInterests)
            {
                var itemToSelect = interests.FirstOrDefault(item => item.Interest == interestKey);
                if (itemToSelect != null)
                {
                    itemToSelect.ForceToggle(true);
                    if (!_selectedInterests.Contains(itemToSelect))
                    {
                        _selectedInterests.Add(itemToSelect);
                    }
                }
            }
        }
        
        UpdateUIState();
    }
    
    public void ConfirmInterests()
    {
        var finalInterests = _selectedInterests.Select(item => item.Interest).ToList();
        
        if (APIHandler.Instance != null)
        {
            APIHandler.Instance.SetAllInterests(finalInterests);
        }
        else
        {
            UserManager.Instance.SetAllInterests(finalInterests);
        }
    }

    private void ToggleInterest(InterestUIItem item, bool isSelected)
    {
        if (isSelected)
        {
            if (!_selectedInterests.Contains(item))
            {
                _selectedInterests.Add(item);
            }
        }
        else
        {
            _selectedInterests.Remove(item);
        }
        
        UpdateUIState();
    }
    
    private void UpdateUIState()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = _selectedInterests.Count >= minSelections;
        }
        
        bool canSelectMore = _selectedInterests.Count < maxSelections;
        foreach (var item in interests)
        {
            if (!_selectedInterests.Contains(item))
            {
                item.SetActive(canSelectMore);
            }
        }
    }
}