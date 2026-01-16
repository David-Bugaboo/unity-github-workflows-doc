using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScrollRectMessages : MonoBehaviour, IBeginDragHandler, IEndDragHandler {

    [SerializeField] UnityEvent onBeginDrag, onEndDrag;

    public void OnBeginDrag( PointerEventData eventData ) {
        onBeginDrag?.Invoke();
    }

    public void OnEndDrag( PointerEventData eventData ) {
        onEndDrag?.Invoke();
    }
}
