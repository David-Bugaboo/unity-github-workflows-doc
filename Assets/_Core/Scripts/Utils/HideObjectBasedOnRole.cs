using System;
using UnityEngine;

public class HideObjectBasedOnRole : MonoBehaviour
{
    public GameObject objectToHide;

    private void Start()
    {
        if(!UserManager.Instance.CurrentUser.IsAdmin) objectToHide.SetActive(false);
    }
}
