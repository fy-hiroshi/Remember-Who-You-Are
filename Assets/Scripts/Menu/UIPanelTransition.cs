using System.Collections;
using UnityEngine;

/// Put this on any UI group (an empty parent holding buttons, or a single button).
/// Show() eases it in and Hide() eases it out with a fade and a slide. While hidden it can't be tapped.
/// Keep the object ACTIVE in the Hierarchy; the script hides it with transparency.
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelTransition : MonoBehaviour
{
    [Tooltip("How far it slides away when hidden (pixels from its normal spot).")]
    [SerializeField] private Vector2 hiddenOffset = new Vector2(-400f, 0f);
    [SerializeField] private float duration = 0.5f;
    [Tooltip("Tick this for things that should start out of sight.")]
    [SerializeField] private bool startHidden = false;

    private CanvasGroup group;
    private RectTransform rect;
    private Vector2 shownPosition;
    private float progress = 1f; // 1 = fully shown, 0 = fully hidden
    private Coroutine routine;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        rect = (RectTransform)transform;
        shownPosition = rect.anchoredPosition;

        progress = startHidden ? 0f : 1f;
        Apply();
        group.interactable = group.blocksRaycasts = !startHidden;
    }

    public void Show() => GoTo(1f);
    public void Hide() => GoTo(0f);

    private void GoTo(float target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(target));
    }

    private IEnumerator Animate(float target)
    {
        // Can't be tapped while it's moving
        group.interactable = false;
        group.blocksRaycasts = false;

        float start = progress;
        float seconds = Mathf.Abs(target - start) * duration;

        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            progress = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / seconds));
            Apply();
            yield return null;
        }

        progress = target;
        Apply();

        bool shown = target >= 1f;
        group.interactable = shown;
        group.blocksRaycasts = shown;
        routine = null;
    }

    private void Apply()
    {
        group.alpha = progress;
        rect.anchoredPosition = shownPosition + hiddenOffset * (1f - progress);
    }
}
