using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CpfInputField : MonoBehaviour
{
    [Header("Referências")]
    private TMP_InputField _inputField;

    [Header("Feedback Visual com Sprites")]
    [Tooltip("Obrigatório: Imagem de contorno ou fundo para trocar o sprite.")]
    [SerializeField] private Image _feedbackImage;
    [SerializeField] private Sprite _neutralSprite;
    [SerializeField] private Sprite _invalidSprite;
    
    [SerializeField] private Button continueButton;

    private bool _isFormatting = false;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        
        _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        _inputField.characterLimit = 14;

        _inputField.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnDestroy()
    {
        _inputField.onValueChanged.RemoveListener(OnInputChanged);
    }

    private void OnInputChanged(string text)
    {
        if (_isFormatting) return;
        _isFormatting = true;
        
        string numbersOnly = new string(text.Where(char.IsDigit).ToArray());

        if (numbersOnly.Length > 11)
        {
            numbersOnly = numbersOnly.Substring(0, 11);
        }
        
        string formattedText = "";
        if (numbersOnly.Length == 11)
        {
            formattedText = $"{numbersOnly.Substring(0, 3)}.{numbersOnly.Substring(3, 3)}.{numbersOnly.Substring(6, 3)}-{numbersOnly.Substring(9)}";
            _inputField.text = formattedText;
        }

        _isFormatting = false;
    }
    
    public bool IsCpfValid()
    {
        var cpf = _inputField.text;
        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        cpf = cpf.Trim().Replace(".", "").Replace("-", "");

        if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            return false;

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        
        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        
        string digito = resto.ToString();
        tempCpf += digito;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        
        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        
        digito += resto.ToString();

        var validCpf = cpf.EndsWith(digito);
        
        _feedbackImage.sprite = !validCpf ? _invalidSprite : _neutralSprite;
        
        return validCpf;
    }
}