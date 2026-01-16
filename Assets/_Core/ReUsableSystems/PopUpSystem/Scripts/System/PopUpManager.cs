using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    private static PopUpManager instance;
    public static PopUpManager Instance 
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PopUpManager>();
            }

            return instance;
        }
    }

    [SerializeField] private PopUpDatabase _popUpDatabase;
    private GameObject loadingPopUp;
    private GameObject achievementPopUp;
    private GameObject upgradePopUp;

    private int loadingCount;
    
    public void SendPopUp(int index, EPopUpScene popUpScene, EPopUpType popUpType, Action confirmAction, Action cancelAction = null)
    {
        var data = _popUpDatabase.popUps.Find(c => c.EPopUpScene == popUpScene).PopUpClasses
            .Find(c => c.PopUpType == popUpType).PopUpDatas.Find(c => c.id == index);

        var prefab = _popUpDatabase.PopUpPrefabs.Find(c => c.PopUpType == popUpType).prefab;

        var popup = Instantiate(prefab, transform).GetComponent<PopUp>();
        popup.Initialize(data, confirmAction, cancelAction);

        if (loadingPopUp)
        {
            DestroyLoadingPopUp();
        }
    }
    
    public void SendCustomPopUp(PopUpData data, Action confirmAction, Action cancelAction = null)
    {
        var prefab = _popUpDatabase.PopUpPrefabs.Find(c => c.PopUpType == data.PopUpType).prefab;

        var popup = Instantiate(prefab, transform).GetComponent<PopUp>();
        popup.Initialize(data, confirmAction, cancelAction);

        if (loadingPopUp)
        {
            DestroyLoadingPopUp();
        }
    }

    public void SendHeaderPopUp(string header, EPopUpType popUpType, Action callback = null)
    {
        var prefab = _popUpDatabase.PopUpPrefabs.Find(c => c.PopUpType == popUpType).prefab;

        var popup = Instantiate(prefab, transform).GetComponent<PopUp>();
        popup.InitializeHeader(header, callback);
        upgradePopUp = popup.gameObject;

        if (loadingPopUp)
        {
            DestroyLoadingPopUp();
        }
    }

    public void SendLoadingPopUp()
    {
        loadingCount++;
        if(loadingPopUp) return;
        var prefab = _popUpDatabase.PopUpPrefabs.Find(c => c.PopUpType == EPopUpType.Loading).prefab;
        loadingPopUp = Instantiate(prefab, transform);
    }

    public void DestroyLoadingPopUp()
    {
        loadingCount--;
        if(!loadingPopUp ) return;
        StartCoroutine(LoadingCoroutine());
    }

    private IEnumerator LoadingCoroutine()
    {
        yield return new WaitForSeconds(1f);
        if(loadingCount > 0 || loadingPopUp == null) yield break;
        Destroy(loadingPopUp);
        loadingPopUp = null;
    }
}
