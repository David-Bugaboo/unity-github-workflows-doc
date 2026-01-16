using UnityEngine;
using UnityEngine.Events;

public class EnableDisableAnimation : MonoBehaviour {
    public CanvasGroup canvasGroup;
    public float animationDuration = 1f;

    private Coroutine currentCoroutine;

    public UnityEvent OnContentInEvent;
    public UnityEvent OnContentOffEvent;

    public void OnContentIn()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        currentCoroutine = StartCoroutine(EnableCoroutine());
    }

    public void OnContentOff()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        currentCoroutine = StartCoroutine(DisableCoroutine());
    }

    private System.Collections.IEnumerator EnableCoroutine()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            float endAlpha = 1f;

            while (elapsedTime < animationDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / animationDuration);
                canvasGroup.alpha = alpha;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            canvasGroup.interactable = true;

            // Chamar o evento OnContentInEvent
            OnContentInEvent.Invoke();
        }
    }

    private System.Collections.IEnumerator DisableCoroutine()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            float endAlpha = 0f;

            while (elapsedTime < animationDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / animationDuration);
                canvasGroup.alpha = alpha;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            OnContentOffEvent.Invoke();
        }
    }
}
