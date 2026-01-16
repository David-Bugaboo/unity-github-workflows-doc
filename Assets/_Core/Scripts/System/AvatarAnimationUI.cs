using UnityEngine;

public class AvatarAnimationUI : MonoBehaviour
{
    [SerializeField] private Animator childAnimator;
    [SerializeField] private string animationName = "DefaultAnimation"; // Nome padr�o da anima��o
    [SerializeField] private float delayBeforeSearch = 2.0f; // Atraso em segundos antes de procurar o childAnimator

    public void AvatarAnimation()
    {
        Invoke("StartAvatarAnimation", delayBeforeSearch);
    }

    public void StartAvatarAnimation()
    {
        if (childAnimator == null)
        {
            childAnimator = GetComponentInChildren<Animator>();
        }

        if (childAnimator != null)
        {
            Debug.Log("Animator found.");
            childAnimator.Play(animationName);
        }
        else
        {
            Debug.LogWarning("Child Animator not found.");
        }
    }
}