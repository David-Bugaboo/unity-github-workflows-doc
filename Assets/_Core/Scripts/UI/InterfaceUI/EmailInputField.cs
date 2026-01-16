using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailInputField : MonoBehaviour
{
    [Header("Referências")]
    private TMP_InputField _inputField;

    [Header("Feedback Visual com Sprites")]
    [Tooltip("Obrigatório: Imagem de contorno ou fundo para trocar o sprite.")]
    [SerializeField] private Image _feedbackImage;
    [SerializeField] private Sprite _invalidSprite;
    [SerializeField] private Sprite _neutralSprite;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.contentType = TMP_InputField.ContentType.EmailAddress;
        
        if (_feedbackImage != null)
        {
            _feedbackImage.sprite = _neutralSprite;
        }
    }
    
    public bool IsEmailValid()
    {
        var email = _inputField.text;
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        const string pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                               + "@"
                               + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
        try
        {
            if (Regex.IsMatch(email, pattern))
            {
                _feedbackImage.sprite = _neutralSprite;
                return true;
            }

            return false;
        }
        catch (FormatException)
        {
            _feedbackImage.sprite = _invalidSprite;
            return false;
        }
    }
}