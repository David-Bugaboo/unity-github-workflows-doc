using System.Collections.Generic;
using UnityEngine;

public class NewUIView<T> : MonoBehaviour
{
#if UNITY_EDITOR
    // Mantido para facilitar a depuração no Inspector
    [SerializeField] protected T[] debugData;
#endif

    // Lista com os dados atuais a serem exibidos
    protected List<T> dataList = new();
    
    // O prefab/modelo do card que será instanciado
    [SerializeField] protected UI_Card<T> cardTemplate;
    
    // A "piscina" de objetos de card, que será reutilizada
    protected List<UI_Card<T>> cardPool = new();

    protected virtual void Awake()
    {
        // Apenas inicializa a lista e garante que o template comece desativado.
        cardPool = new List<UI_Card<T>>();
        if (cardTemplate != null)
        {
            cardTemplate.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Método principal para carregar um novo conjunto de dados na view.
    /// </summary>
    public virtual void LoadData(T[] newData)
    {
        dataList = new List<T>(newData ?? new T[0]);
#if UNITY_EDITOR
        debugData = dataList.ToArray();
#endif
        RefreshView();
    }

    /// <summary>
    /// O novo coração do sistema. Sincroniza a piscina de cartões com a lista de dados.
    /// </summary>
    protected virtual void RefreshView()
    {
        // Percorre a lista de dados que queremos exibir
        for (int i = 0; i < dataList.Count; i++)
        {
            UI_Card<T> card;

            if (i < cardPool.Count)
            {
                // 1. Reutiliza um cartão existente da piscina
                card = cardPool[i];
            }
            else
            {
                // 2. Se não há cartões suficientes na piscina, cria um novo
                card = Instantiate(cardTemplate, cardTemplate.transform.parent);
                cardPool.Add(card);
                OnCardCreated(card);
            }

            // Atualiza os dados do cartão e o ativa
            card.Data = dataList[i];
            card.ValidateCard(true);
        }

        // 3. Desativa quaisquer cartões extras que não estão mais em uso
        for (int i = dataList.Count; i < cardPool.Count; i++)
        {
            cardPool[i].ValidateCard(false);
        }
    }
    
    protected virtual void OnCardCreated(UI_Card<T> card) { }

    /// <summary>
    /// Adiciona um novo item de dado e atualiza a view.
    /// </summary>
    protected virtual void Add(T newData)
    {
        dataList.Add(newData);
        RefreshView();
    }

    /// <summary>
    /// Remove um item de dado e atualiza a view.
    /// </summary>
    protected virtual void Remove(T target)
    {
        if (target == null) return;
        dataList.Remove(target);
        RefreshView();
    }
    
    /// <summary>
    /// Limpa todos os dados e desativa todos os cards.
    /// </summary>
    protected virtual void ClearData()
    {
        dataList.Clear();
        RefreshView();
    }
    
    protected virtual bool Compare(T a, T b) => a.Equals(b);
}