using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableObject : MonoBehaviour
{
    public UnityEvent onClick;
    public static bool canUserInteract;
    
    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults;

    void Awake()
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>();
    }

    
    private void OnMouseDown()
    {

    }

    private void OnMouseUpAsButton()
    {
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster[] allRaycasters = FindObjectsOfType<GraphicRaycaster>();

        foreach (var raycaster in allRaycasters)
        {
            raycastResults.Clear();
            raycaster.Raycast(pointerEventData, raycastResults);
            
            if (raycastResults.Count > 0)
            {
                foreach (var obj in raycastResults)
                {
                    GameObject hitUIObject = obj.gameObject;
                
                    if (hitUIObject.CompareTag("Blocker"))
                    {
                        Debug.Log("Clique bloqueado pela UI: " + hitUIObject.name);
                        return;
                    }   
                }
            }
        }
        
        Debug.Log("Clique no objeto 3D '" + gameObject.name + "' foi bem-sucedido.");
        if (canUserInteract)
        {
            onClick.Invoke();
        }
    }
}
