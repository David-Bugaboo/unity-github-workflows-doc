using TMPro;
using UnityEngine;

public static class TMPExtensions
{
    public static void TrySetText(this TMP_Text target, string source, bool deactivateIfnoSource = false)
    {
        if (target == null) return;
        if (string.IsNullOrEmpty(source.Trim()))
        {
            target.gameObject.SetActive(deactivateIfnoSource);
            return;
        }

        target.gameObject.SetActive(true);
        target.text = source;
    }
}