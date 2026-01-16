using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserCard : UI_Card<UserData>
{
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text userEmailText;

    public event System.Action<UserData> OnUserSelected;

    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(() => OnUserSelected?.Invoke(Data));
    }

    protected override void OnDataSet()
    {
        if (Data == null) return;
        userNameText.text = Data.name;
        userEmailText.text = Data.email;
    }
}