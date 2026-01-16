using UnityEngine;

public abstract class UI_Card<T> : UI_Card_Base {
    [SerializeField] T data;
    public T Data { get => data; set { data = value; OnDataSet(); } }
    public object UserData;
    protected abstract void OnDataSet();
    public virtual void ValidateCard( bool active ) => gameObject.SetActive( data != null && active );
    public static implicit operator T (UI_Card<T> card) => card.data;
}