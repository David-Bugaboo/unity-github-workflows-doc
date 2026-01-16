using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserListCard : UI_Card<string>
{
    [SerializeField] private TextMeshProUGUI userEmailText;
    [SerializeField] private Button removeButton;

    // Evento que avisa que o botão de remover deste card foi clicado
    public event Action<string> OnRemoveClicked;

    private void Awake()
    {
        // Quando o botão de remover for clicado, dispara o evento com o email deste card
        removeButton.onClick.AddListener(() => OnRemoveClicked?.Invoke(Data));
    }

    protected override void OnDataSet()
    {
        // Atualiza o texto com o email do usuário
        userEmailText.text = Data;
    }
    
    /// <summary>
    /// Método para dar feedback visual de que o usuário foi marcado para remoção.
    /// </summary>
    public void SetMarkedForRemoval(bool isMarked)
    {
        // Exemplo: muda a cor do texto para cinza e desativa o botão
        userEmailText.color = isMarked ? Color.gray : Color.white;
        removeButton.interactable = !isMarked;
    }
}