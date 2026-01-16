using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AutoCompleteCardController : MonoBehaviour
{
    [SerializeField] private TMP_InputField targetInputField;
    [SerializeField] private UI_View<string> suggestionsView; // A sua UI_View que usará os AutoCompleteCards
    
    public event System.Action<string> OnItemSelected;

    private List<string> _fullDataSource; // Ex: Lista de todos os e-mails de usuários

    private void Start()
    {
        targetInputField.onValueChanged.AddListener(OnInputValueChanged);
        suggestionsView.gameObject.SetActive(false); // Começa com as sugestões escondidas
    }
    
    // Este método deve ser chamado para alimentar o auto-complete com os dados
    public void SetDataSource(List<string> dataSource)
    {
        _fullDataSource = dataSource;
    }

    private void OnInputValueChanged(string currentText)
    {
        if (string.IsNullOrWhiteSpace(currentText) || _fullDataSource == null)
        {
            suggestionsView.gameObject.SetActive(false);
            return;
        }

        // Filtra a fonte de dados com base no texto atual
        var suggestions = _fullDataSource
            .Where(item => item.ToLower().Contains(currentText.ToLower()))
            .Take(5) // Limita a 5 sugestões, por exemplo
            .ToList();

        if (suggestions.Count > 0)
        {
            suggestionsView.LoadData(suggestions);
            suggestionsView.gameObject.SetActive(true);
        }
        else
        {
            suggestionsView.gameObject.SetActive(false);
        }
    }

    // O controlador se inscreve nos eventos dos cards criados
    private void OnCardCreated(UI_Card_New<string> card)
    {
        var autoCompleteCard = card as AutoCompleteCard;
        if (autoCompleteCard != null)
        {
            autoCompleteCard.OnSuggestionSelected += HandleSuggestionSelected;
        }
    }

    private void HandleSuggestionSelected(string selectedValue)
    {
        targetInputField.text = ""; 
        suggestionsView.gameObject.SetActive(false);
        OnItemSelected?.Invoke(selectedValue);
    }
}