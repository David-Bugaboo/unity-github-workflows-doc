using System.Collections.Generic;
using UnityEngine;

public class NewEventListView : MonoBehaviour
{
    [SerializeField] private GameObject eventCardTemplate; // Prefab do cartão de evento
    [SerializeField] private Transform contentParent;

    private List<GameObject> _instantiatedCards = new List<GameObject>();

    private void Awake()
    {
        eventCardTemplate.SetActive(false);
    }

    /// <summary>
    /// Limpa a lista e exibe os novos eventos.
    /// </summary>
    public void DisplayEvents(List<EventData> events) // Supondo que você tenha uma classe EventData
    {
        // Limpa cartões antigos
        foreach (var card in _instantiatedCards)
        {
            Destroy(card);
        }
        _instantiatedCards.Clear();
        
        if (events == null || events.Count == 0)
        {
            return;
        }

        // Cria novos cartões
        foreach (var evt in events)
        {
            GameObject newCard = Instantiate(eventCardTemplate, contentParent);
            newCard.GetComponent<UI_Card_EventView>().Data = evt;
            newCard.SetActive(true);
            _instantiatedCards.Add(newCard);
        }
    }
}