using UnityEngine;
using TMPro;

public class ShowVersion : MonoBehaviour
{
    public TextMeshProUGUI versionText;

    void Start()
    {
        // Certifique-se de definir o componente TextMeshProUGUI no Editor Unity para a variável versionText
        if (versionText != null)
        {
            // Obtém a versão do projeto
            string version = Application.version;

            // Define o texto do TMP para exibir a versão
            versionText.text = $"Versão: {version}";
        }
        else
        {
            Debug.LogError("O componente TextMeshProUGUI não foi atribuído ao script.");
        }
    }
}
