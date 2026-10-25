using System.Collections;
using UnityEngine;

/// Put this on the Light itself and keep its GameObject ACTIVE (the script switches off just the Light).
/// Hook TurnOn() to a UnityEvent, e.g. EventLock's "On Revealed". The light stays off until then,
/// then fades in to the intensity you set on the Light in the Inspector.
[RequireComponent(typeof(Light))]
public class RevealLight : MonoBehaviour
{
    [Tooltip("Wait this long after the event before the light starts, so it comes on as the furniture slides.")]
    [SerializeField] private float delay = 0.2f;
    [Tooltip("How long the light takes to fade in. 0 = instant.")]
    [SerializeField] private float fadeSeconds = 0.8f;

    private Light lightSource;
    private float targetIntensity;
    private bool turnedOn;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        targetIntensity = lightSource.intensity; // whatever you set in the Inspector
        lightSource.enabled = false;
    }

    public void TurnOn()
    {
        if (turnedOn) return;
        turnedOn = true;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        lightSource.intensity = 0f;
        lightSource.enabled = true;

        for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
        {
            lightSource.intensity = Mathf.Lerp(0f, targetIntensity, t / fadeSeconds);
            yield return null;
        }

        lightSource.intensity = targetIntensity;
    }
}
