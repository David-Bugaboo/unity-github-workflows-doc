using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(RectTransform))]
public class FitColliderToRectTransform : MonoBehaviour
{
    private BoxCollider boxCollider;
    private RectTransform rectTransform;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        UpdateColliderSize();
    }
    
    public void UpdateColliderSize()
    {
        Vector2 rectSize = rectTransform.rect.size;
        boxCollider.size = new Vector3(rectSize.x, rectSize.y, 0.01f);
    }
}