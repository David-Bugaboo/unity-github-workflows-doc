using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PopUpData", menuName = "Game/PopUp/PopUpData")]
public class PopUpData : ScriptableObject
{
    public int id;
    public EPopUpType PopUpType;
    public string Header;
    public string Description;
    public string confirmName;
    public string cancelName;
}

[Serializable]
public class PopUpScene
{
    public EPopUpScene EPopUpScene;
    public List<PopUpClass> PopUpClasses;
}

[Serializable]
public class PopUpClass
{
    public EPopUpType PopUpType;
    public List<PopUpData> PopUpDatas;
}

[Serializable]
public class PopUpPrefabs
{
    public EPopUpType PopUpType;
    public GameObject prefab;
}
