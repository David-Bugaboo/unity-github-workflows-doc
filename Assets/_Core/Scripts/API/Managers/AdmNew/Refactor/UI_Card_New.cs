using System;
using UnityEngine;

public abstract class UI_Card_New<T> : UI_Card_Base_New {
        
    [SerializeField] protected T data; // Alterado para 'protected' para que classes filhas possam acessar
        
    public T Data { 
        get => data; 
        set { 
            data = value; 
            OnDataSet(); 
        } 
    }
        
    // Evento genérico para notificar quando o card for clicado.
    // O Presenter irá se inscrever neste evento.
    public event Action<UI_Card_New<T>> OnCardClicked;

    /// <summary>
    /// Método a ser chamado pelo componente Button do Unity no Inspector (via OnClick).
    /// </summary>
    public void RaiseClickEvent()
    {
        OnCardClicked?.Invoke(this);
    }

    /// <summary>
    /// Chamado sempre que a propriedade 'Data' é atualizada.
    /// A classe filha deve implementar isso para atualizar seus elementos visuais.
    /// </summary>
    protected abstract void OnDataSet();

    public virtual void ValidateCard(bool active) => gameObject.SetActive(data != null && active);
        
    public static implicit operator T(UI_Card_New<T> card) => card.data;
}