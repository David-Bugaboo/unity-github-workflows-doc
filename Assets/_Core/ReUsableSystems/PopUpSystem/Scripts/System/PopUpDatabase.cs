using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PopUpDatabase", menuName = "Game/PopUp/PopUpDatabase")]
public class PopUpDatabase : ScriptableObject
{
    public List<PopUpScene> popUps;
    public List<PopUpPrefabs> PopUpPrefabs;
}
