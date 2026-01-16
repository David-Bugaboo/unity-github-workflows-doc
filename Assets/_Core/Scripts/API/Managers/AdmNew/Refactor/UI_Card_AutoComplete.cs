using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Card_AutoComplete : UI_Card_New<string> {
        
    [SerializeField] private TMP_Text textName;
        
    private void Awake()
    {
        // Garante que o clique no botão deste componente irá disparar o evento base
        GetComponent<Button>().onClick.AddListener(RaiseClickEvent);
    }

    // Implementação para atualizar o texto quando a 'Data' for definida
    protected override void OnDataSet()
    {
        if (textName != null)
        {
            textName.text = Data;
        }
    }
        
    // A validação foi simplificada para apenas checar se a string tem conteúdo.
    public override void ValidateCard(bool active)
    {
        if(gameObject != null)
        {
            gameObject.SetActive(!string.IsNullOrEmpty(Data) && active);
        }
    }
}