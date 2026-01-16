using System.Collections.Generic;
using UnityEngine;

public abstract class UI_View<T> : UI_View_Base
{
    //internal enum ViewPoolingType { Uncapped, Capped, CappedFlexible }

#if UNITY_EDITOR
    [SerializeField] protected List<T> debugData;
#endif
    protected List<T> dataList => _dataListList;
    List<T> _dataListList;
    
    [SerializeField] protected UI_Card<T> cardTemplate;
    [SerializeField] protected int poolDefaultSize = 50;

    [SerializeField, Tooltip("if its on, it'll create new templates as the data size grows")]
    bool capAtPoolSize = false;

    protected List<UI_Card<T>> cards;
    bool _active;

    protected virtual void Awake()
    {
        cards = new();
        cardTemplate.gameObject.SetActive(false);
        if (!capAtPoolSize)
            UpdateCardListSize(poolDefaultSize);
    }

    public void LoadData(List<T> data) => LoadData(data, true);

    [ContextMenu("Refresh")]
    public virtual void LoadData(List<T> data, bool setActive = true)
    {
        if (data == null) return;
        _dataListList = new(data);
        if (_dataListList.Count > poolDefaultSize && capAtPoolSize)
            _dataListList.RemoveRange(0, _dataListList.Count - poolDefaultSize);
#if UNITY_EDITOR
        debugData = data;
#endif
        UpdateCards(setActive);
    }

    void UpdateCards(bool setActive)
    {
        _active = setActive;
        if (!capAtPoolSize)
            UpdateCardListSize(dataList.Count);
        for (int i = 0; i < cards.Count; i++)
        {
            if (dataList.Count > i)
                cards[i].Data = dataList[i];
            cards[i].ValidateCard(setActive && dataList.Count > i);
        }
    }

    void UpdateCardListSize(int size)
    {
        Debug.Log($"{gameObject.name} cards is null? {cards == null}");
        cards ??= new();
        for (int i = cards.Count; i < size; i++)
            cards.Add(Instantiate(cardTemplate, cardTemplate.transform.parent));
    }

    protected virtual void Add(T newData, bool updateCardList = false)
    {
        AddDataToList(newData);
        if (capAtPoolSize && _dataListList.Count >= poolDefaultSize) _dataListList.RemoveAt(0);
#if UNITY_EDITOR
        debugData = _dataListList;
#endif
        if (updateCardList) UpdateCardListSize(_dataListList.Count);
        UpdateCards(_active);
    }

    protected virtual void Remove(T target)
    {
        if (target == null) return;
        _dataListList.Remove(target);
#if UNITY_EDITOR
        debugData = _dataListList;
#endif
        foreach (var card in cards)
        {
            if (Compare(card.Data, target))
            {
                cards.Remove(card);
                Destroy(card.gameObject);
                break;
            }
        }
    }

    protected virtual bool Compare(T a, T b) => a.Equals(b);

    protected void AddDataToList(T targetData)
    {
        if (targetData == null) return;
        _dataListList?.Add(targetData);
    }

    protected void ClearData(bool destroyCards = true)
    {
        _dataListList.Clear();
        if (!destroyCards) return;
        foreach (var item in cards)
            Destroy(item.gameObject);
        cards.Clear();
    }
}