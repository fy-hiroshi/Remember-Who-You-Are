using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Add this to any UI Button. It squashes a little while pressed, then pops back with a bounce.
/// It also pops in whenever the button is shown (e.g. the Read button appearing).
/// Tip: set the button's pivot to (0.5, 0.5) so it pops from its center.
public class ButtonPop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Press")]
    [Tooltip("How small the button gets while held down (1 = no change).")]
    [SerializeField] private float pressedScale = 0.88f;
    [Tooltip("Extra push when you let go, so it overshoots into a 'pop'. 0 = just a soft return.")]
    [SerializeField] private float releaseKick = 3f;

    [Header("Appear")]
    [SerializeField] private bool popWhenShown = true;
    [SerializeField] private float shownStartScale = 0.5f;

    [Header("Spring feel")]
    [SerializeField] private float stiffness = 350f;
    [Tooltip("Lower = bouncier, higher = calmer.")]
    [SerializeField] private float damping = 14f;

    [Header("Sound (optional, leave empty for silence)")]
    [Tooltip("Plays the moment the button is pressed. Always a plain 2D sound, so it's the same volume wherever the button is.")]
    [SerializeField] private SoundSet pressSound;

    private Vector3 baseScale;
    private float scale = 1f;
    private float velocity;
    private float target = 1f;
    private Selectable selectable;

    private void Awake()
    {
        baseScale = transform.localScale;
        selectable = GetComponent<Selectable>();
    }

    private void OnEnable()
    {
        scale = popWhenShown ? shownStartScale : 1f;
        velocity = 0f;
        target = 1f;
        Apply();
    }

    private void OnDisable()
    {
        // Never leave a hidden button stuck at a squashed size
        scale = 1f;
        velocity = 0f;
        target = 1f;
        Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        target = pressedScale;
        PlayPressSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (target < 1f) velocity += releaseKick;
        target = 1f;
    }

    private void PlayPressSound()
    {
        if (pressSound == null) return;

        // A greyed-out (not interactable) button shouldn't click
        if (selectable != null && !selectable.IsInteractable()) return;

        pressSound.Play2D();
    }

    private void Update()
    {
        UISpring.Step(ref scale, ref velocity, target, stiffness, damping, Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        transform.localScale = baseScale * scale;
    }
}