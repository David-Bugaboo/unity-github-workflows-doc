using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [System.Serializable]
    public class Event : UnityEvent<Vector2> { }

    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public float joystickRange = 50f;
    public bool clampToMagnitude;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;
    [SerializeField] private bool lookZone;
    [SerializeField] private CinemachineInputAxisController input;
    [SerializeField] private CinemachineOrbitalFollow cinemachineFollow;

    //Stored Pointer Values
    private Vector2 pointerDownPosition;
    private Vector2 currentPointerPosition;

    [Header("Output")]
    public Event touchZoneOutputEvent;

    void Start()
    {
        SetupHandle();
    }

    private void SetupHandle()
    {
        if(handleRect)
        {
            SetObjectActiveState(handleRect.gameObject, false); 
            SetObjectActiveState(containerRect.gameObject, false); 
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lookZone)
        {
            // input.enabled = true;
        }
        else
        {
            cinemachineFollow.VerticalAxis.Recentering.Enabled = true;
            cinemachineFollow.HorizontalAxis.Recentering.Enabled = true;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition);

        if(handleRect)
        {
            SetObjectActiveState(handleRect.gameObject, true);
            SetObjectActiveState(containerRect.gameObject, true);
            UpdateContainerRectPosition(pointerDownPosition);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition);
        currentPointerPosition = ApplySizeDelta(currentPointerPosition);
        Vector2 clampedPosition = ClampValuesToMagnitude(currentPointerPosition);
        Vector2 outputPosition = ApplyInversionFilter(clampedPosition);
        OutputPointerEventValue(outputPosition * magnitudeMultiplier);
        
        if(handleRect)
        {
            UpdateHandleRectPosition(clampedPosition * joystickRange);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (lookZone)
        {
            // input.enabled = false;
        }
        else
        {
            cinemachineFollow.VerticalAxis.Recentering.Enabled = false;
            cinemachineFollow.HorizontalAxis.Recentering.Enabled = false;
        }
        
        pointerDownPosition = Vector2.zero;
        currentPointerPosition = Vector2.zero;

        OutputPointerEventValue(Vector2.zero);

        if(handleRect)
        {
            SetObjectActiveState(handleRect.gameObject, false);
            SetObjectActiveState(containerRect.gameObject, false);
            UpdateContainerRectPosition(Vector2.zero);
            UpdateHandleRectPosition(Vector2.zero);
        }
    }

    public void SetPointerUp()
    {
        if (lookZone)
        {
            // input.enabled = false;
        }
        else
        {
            cinemachineFollow.VerticalAxis.Recentering.Enabled = false;
            cinemachineFollow.HorizontalAxis.Recentering.Enabled = false;
        }
        
        pointerDownPosition = Vector2.zero;
        currentPointerPosition = Vector2.zero;

        OutputPointerEventValue(Vector2.zero);

        if(handleRect)
        {
            SetObjectActiveState(handleRect.gameObject, false);
            SetObjectActiveState(containerRect.gameObject, false);
            UpdateContainerRectPosition(Vector2.zero);
            UpdateHandleRectPosition(Vector2.zero);
        }
    }
    
    Vector2 ApplySizeDelta(Vector2 position)
    {
        float normalizer = Mathf.Min(containerRect.sizeDelta.x, containerRect.sizeDelta.y);
        float x = (position.x / normalizer) * 2.5f;
        float y = (position.y / normalizer) * 2.5f;
        return new Vector2(x, y);
    }

    void OutputPointerEventValue(Vector2 pointerPosition)
    {
        touchZoneOutputEvent.Invoke(pointerPosition);
    }

    void UpdateHandleRectPosition(Vector2 newPosition)
    {
        handleRect.localPosition = newPosition;
    }
    
    void UpdateContainerRectPosition(Vector2 newPosition)
    {
        containerRect.localPosition = newPosition;
    }

    void SetObjectActiveState(GameObject targetObject, bool newState)
    {
        targetObject.SetActive(newState);
    }

    Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
    {
        return secondPosition - firstPosition;
    }

    Vector2 ClampValuesToMagnitude(Vector2 position)
    {
        return Vector2.ClampMagnitude(position, 1);
    }

    Vector2 ApplyInversionFilter(Vector2 position)
    {
        if(invertXOutputValue)
        {
            position.x = InvertValue(position.x);
        }

        if(invertYOutputValue)
        {
            position.y = InvertValue(position.y);
        }

        return position;
    }

    float InvertValue(float value)
    {
        return -value;
    }
    
}
