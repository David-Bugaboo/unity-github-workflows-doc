using System;
using System.Collections.Generic;
using UnityEngine;

public class AUI_UserListView : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UserListCard userCardPrefab;
    
    public event Action<string> OnUserMarkedForRemoval;
    
    private List<UserListCard> _instantiatedCards = new List<UserListCard>();

    private void Start()
    {
        userCardPrefab.gameObject.SetActive(false);
    }
    
    public void LoadData(List<string> userEmails)
    {
        foreach (var card in _instantiatedCards)
        {
            if(card.gameObject != userCardPrefab.gameObject) Destroy(card.gameObject);
        }
        _instantiatedCards.Clear();

        if (userEmails == null) return;
        
        foreach (var email in userEmails)
        {
            UserListCard newCard = Instantiate(userCardPrefab, cardContainer);
            newCard.Data = email;
            
            newCard.OnRemoveClicked += (userEmail) => {
                newCard.SetMarkedForRemoval(true);
                OnUserMarkedForRemoval?.Invoke(userEmail);
            };
            
            newCard.gameObject.SetActive(true);
            _instantiatedCards.Add(newCard);
        }
    }
    
    public int Count => _instantiatedCards.Count;
}