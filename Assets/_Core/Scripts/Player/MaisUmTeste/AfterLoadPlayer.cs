using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterLoadPlayer : MonoBehaviour
{
    private Buga_PlayerController controller;
    
    public List<GameObject> allDisableObjects;
    
    public void Start()
    {
        StartCoroutine(LoadPlayer());
    }

    private IEnumerator LoadPlayer()
    {
        yield return new WaitForSeconds(1);
        foreach (GameObject obj in allDisableObjects)
        {
            obj.SetActive(false);
        }
    }
}
