using System.Collections;
using UnityEngine;

/// Put this on any UI text (or panel). Call Show() and it fades in, stays for a moment, then fades out.
/// It hides itself with a CanvasGroup, so leave the object ACTIVE in the Hierarchy.
[RequireComponent(typeof(CanvasGroup))]
public class MessagePopup : MonoBehaviour
{
    [SerializeField] private float visibleSeconds = 2.5f;
    [SerializeField] private float fadeSeconds = 0.3f;

    private CanvasGroup group;
    private Coroutine routine;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false; // never blocks touches
        group.interactable = false;
    }

    /// Hook this up to a UnityEvent (e.g. KeyLock's "On Locked No Key").
    public void Show()
    {
        // Safety: if the object was left disabled in the editor, switch it on first
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Restarting instead of stacking, so tapping the door repeatedly just refreshes the message
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    /// Fades in and STAYS until Hide() is called (e.g. the "go home" hint).
    public void ShowPersistent()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeTo(1f));
    }

    /// Fades the message out right away.
    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeTo(0f));
    }

    private IEnumerator FadeTo(float target)
    {
        yield return Fade(group.alpha, target);
        routine = null;
    }

    private IEnumerator Run()
    {
        yield return Fade(group.alpha, 1f);
        yield return new WaitForSeconds(visibleSeconds);
        yield return Fade(1f, 0f);
        routine = null;
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / fadeSeconds);
            yield return null;
        }
        group.alpha = to;
    }
}