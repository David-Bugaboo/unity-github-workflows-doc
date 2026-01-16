using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ToggleFogDensity : MonoBehaviour
{
    public float fogDensityA = 0.02f; // Initial fog density value A
    public float fogDensityB = 0.05f; // Initial fog density value B
    public float transitionSpeedA = 2.0f; // Speed at which the fog density transitions to value A
    public float transitionSpeedB = 4.0f; // Speed at which the fog density transitions to value B

    private float targetDensity; // Target fog density value
    private float currentDensity; // Current fog density value
    private bool isTransitioning; // Flag to indicate if a transition is in progress
    private Coroutine transitionCoroutine; // Reference to the active transition coroutine

    private void Start()
    {
        RenderSettings.fogDensity = fogDensityA;
        currentDensity = fogDensityA;
        targetDensity = fogDensityA;
    }

    public void ToggleFogA()
    {
        if (isTransitioning)
            SkipTransitionAndToggleFog(fogDensityA, transitionSpeedA);
        else if (!Mathf.Approximately(currentDensity, fogDensityA))
            StartTransition(fogDensityA, transitionSpeedA);
    }

    public void ToggleFogB()
    {
        if (isTransitioning)
            SkipTransitionAndToggleFog(fogDensityB, transitionSpeedB);
        else if (!Mathf.Approximately(currentDensity, fogDensityB))
            StartTransition(fogDensityB, transitionSpeedB);
    }

    private void StartTransition(float target, float speed)
    {
        if (isTransitioning)
            StopCoroutine(transitionCoroutine);

        targetDensity = target;
        transitionCoroutine = StartCoroutine(TransitionFogDensity(targetDensity, speed));
    }

    private IEnumerator TransitionFogDensity(float target, float speed)
    {
        isTransitioning = true;
        float start = currentDensity;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            currentDensity = Mathf.Lerp(start, target, t);
            SetFogDensity(currentDensity);
            yield return null;
        }

        currentDensity = target;
        SetFogDensity(target);

        isTransitioning = false;
    }

    private void SetFogDensity(float density)
    {
        currentDensity = density;
        RenderSettings.fogDensity = density;
    }

    public void SkipTransitionAndToggleFog(float target, float speed)
    {
        if (isTransitioning)
        {
            StopCoroutine(transitionCoroutine);
            isTransitioning = false;
        }

        targetDensity = target;
        StartTransition(targetDensity, speed);
    }
}