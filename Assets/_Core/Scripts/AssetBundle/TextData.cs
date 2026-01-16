using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(fileName = "AssetBundleTextUI", menuName = "ScriptableObjects/AssetBundleTextUI", order = 1)]
public class TextData : ScriptableObject
{
    [SerializeField]
    private List<string> storedTexts = new List<string>();

    public List<string> StoredTexts
    {
        get { return storedTexts; }
    }

    public void AddText(string text)
    {
        storedTexts.Add(text);
    }

    public void SetTextUI(int index, TextMeshProUGUI textMeshPro)
    {
        if (index >= 0 && index < storedTexts.Count)
        {
            textMeshPro.text = storedTexts[index];
        }
        else
        {
            Debug.LogError("Invalid index: " + index);
        }
    }
}