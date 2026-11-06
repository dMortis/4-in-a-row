using System.Collections;
using UnityEngine;

public class RestartGameInput : MonoBehaviour
{
    private GameManager manager;
    private Vector3 restingScale;
    private Quaternion restingRotation;
    private bool isAnimating;

    public void Initialize(GameManager gameManager)
    {
        manager = gameManager;
        restingScale = transform.localScale;
        restingRotation = transform.localRotation;
    }

    public void Activate()
    {
        if (manager == null || isAnimating) return;
        StartCoroutine(PressAnimation());
    }

    private IEnumerator PressAnimation()
    {
        isAnimating = true;
        const float duration = 0.32f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale;
            if (t < 0.28f)
            {
                float down = Mathf.SmoothStep(0f, 1f, t / 0.28f);
                scale = Mathf.Lerp(1f, 0.88f, down);
            }
            else if (t < 0.68f)
            {
                float bounce = Mathf.SmoothStep(0f, 1f, (t - 0.28f) / 0.4f);
                scale = Mathf.Lerp(0.88f, 1.1f, bounce);
            }
            else
            {
                float settle = Mathf.SmoothStep(0f, 1f, (t - 0.68f) / 0.32f);
                scale = Mathf.Lerp(1.1f, 1f, settle);
            }

            transform.localScale = restingScale * scale;
            float twist = Mathf.Sin(t * Mathf.PI) * 16f;
            transform.localRotation = restingRotation * Quaternion.Euler(0f, 0f, twist);
            yield return null;
        }

        transform.localScale = restingScale;
        transform.localRotation = restingRotation;
        isAnimating = false;
        manager.RestartGame();
    }
}
