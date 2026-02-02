using UnityEngine;
using System.Collections;

public class UIJuice : MonoBehaviour
{
    public float duration = 0.25f;
    public Vector3 targetScale = Vector3.one;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // We use a CanvasGroup to handle fading the whole group at once
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void PlayEntrance()
    {
        StopAllCoroutines();

        // MOVE THE RESET HERE: Outside the coroutine.
        // This ensures that the moment this method is called, the state is reset,
        // even if the coroutine takes a frame to start.
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null) rect.localScale = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 0;

        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        // Ensure we have the RectTransform reference
        RectTransform rect = GetComponent<RectTransform>();

        // 1. Reset state
        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;

            // Back-Out Cubic formula for a professional "pop"
            // It starts fast, overshoots 1.0, and settles back.
            float s = 1.70158f;
            float t = percentage - 1;
            float curve = (t * t * ((s + 1) * t + s) + 1);

            // Use a curve or "SmoothStep" for a snappier feel
            // float curve = Mathf.Sin(percentage * Mathf.PI * 0.5f);

            // USE LERPUNCLAMPED: This is the key to seeing the bounce!
            rect.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, curve);

            // Alpha doesn't need to bounce, so regular Lerp is fine here
            canvasGroup.alpha = Mathf.Lerp(0, 1, percentage * 2); // Fades in twice as fast as the grow

            yield return null;
        }

        // Final safety set
        rect.localScale = targetScale;
        canvasGroup.alpha = 1;
    }

    public void PlayExit(System.Action onComplete)
    {
        StopAllCoroutines();
        StartCoroutine(ExitRoutine(onComplete));
    }

    private IEnumerator ExitRoutine(System.Action onComplete)
    {
        RectTransform rect = GetComponent<RectTransform>();
        float elapsed = 0;
        Vector3 startScale = rect.localScale;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;

            // "Ease In Back" formula for shrinking away
            float s = 1.70158f;
            float t = percentage;
            float curve = t * t * ((s + 1) * t - s);

            rect.localScale = Vector3.LerpUnclamped(startScale, Vector3.zero, percentage);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, percentage);

            yield return null;
        }

        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        // This tells the InteractionButton it's safe to turn the object off now
        onComplete?.Invoke();
    }

    public void PlayButtonClick()
    {
        StopAllCoroutines();
        StartCoroutine(ButtonClickRoutine());
    }

    private IEnumerator ButtonClickRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        float elapsed = 0;
        float clickDuration = 0.15f; // Faster than the entrance
        Vector3 originalScale = targetScale;
        Vector3 compressedScale = originalScale * 0.9f; // Shrink to 90%

        // Phase 1: Squish down fast
        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / clickDuration;
            rect.localScale = Vector3.Lerp(originalScale, compressedScale, percentage);
            yield return null;
        }

        // Phase 2: Pop back with a bounce
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;

            // Using that same "Back-Out" formula for the pop back
            float s = 1.70158f;
            float t = percentage - 1;
            float curve = (t * t * ((s + 1) * t + s) + 1);

            rect.localScale = Vector3.LerpUnclamped(compressedScale, originalScale, curve);
            yield return null;
        }

        rect.localScale = originalScale;
    }

    public void PlayHover(bool isHovering)
    {
        StopAllCoroutines();
        StartCoroutine(HoverRoutine(isHovering));
    }

    private IEnumerator HoverRoutine(bool isHovering)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 target = isHovering ? targetScale * 1.1f : targetScale; // Grow 10% on hover
        float elapsed = 0;
        float hoverDuration = 0.1f; // Very snappy
        Vector3 startScale = rect.localScale;

        while (elapsed < hoverDuration)
        {
            elapsed += Time.deltaTime;
            rect.localScale = Vector3.Lerp(startScale, target, elapsed / hoverDuration);
            yield return null;
        }
        rect.localScale = target;
    }

    public void PlayPulse()
    {
        StopAllCoroutines();
        StartCoroutine(PulseRoutine());
    }

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 originalScale = targetScale;
        float duration = 0.2f;
        float elapsed = 0;

        // Grow then shrink
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float curve = Mathf.Sin((elapsed / duration) * Mathf.PI);
            rect.localScale = originalScale + (Vector3.one * (curve * 0.2f));
            yield return null;
        }
        rect.localScale = originalScale;
    }

    private IEnumerator ShakeRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 originalPos = rect.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0;
        float strength = 10f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 50f) * strength * (1 - (elapsed / duration));
            rect.anchoredPosition = originalPos + new Vector3(x, 0, 0);
            yield return null;
        }
        rect.anchoredPosition = originalPos;
    }
}