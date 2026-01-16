using UnityEngine;

[CreateAssetMenu( menuName = "IEL/Popup", fileName = "New Popup Data" )]
public class PopupContainerData : ScriptableObject {

    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField] public string Message { get; private set; }
    [field: SerializeField] public int Code { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

}
