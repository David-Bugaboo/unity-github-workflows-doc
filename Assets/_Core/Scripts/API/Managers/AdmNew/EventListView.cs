using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventListView : MonoBehaviour
{
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private EventCard eventCardPrefab;
    [SerializeField] private TextMeshProUGUI EditButtonText;
    
    [SerializeField] private EventListController _controller;

    private List<EventCard> _instantiatedCards = new List<EventCard>();

    private void Awake()
    {
        eventCardPrefab.gameObject.SetActive(false);
    }

    public void SetLoading(bool isLoading)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(isLoading);
        }
    }

    public void DisplayEvents(List<EventData> events)
    {
        EditButtonText.text = "Criar evento";
        foreach (var card in _instantiatedCards)
        {
            if(card != null && card.gameObject != null) Destroy(card.gameObject);
        }
        _instantiatedCards.Clear();

        if (events == null) return;
    
        foreach (var eventData in events)
        {
            EventCard newCard = Instantiate(eventCardPrefab, cardContainer);
            newCard.Data = eventData;
            
            newCard.editButton.onClick.AddListener(() =>
            {
                _controller.HandleEditRequest(eventData);
                EditButtonText.text = "Editar evento";
            });
            newCard.deleteButton.onClick.AddListener(() => _controller.HandleDeleteRequest(eventData));
        
            newCard.gameObject.SetActive(true);
            _instantiatedCards.Add(newCard);
        }
    }
}