using UnityEngine;
using UnityEngine.EventSystems;

/// Attach to the joystick's background circle (the object the finger presses on).
/// The handle must be a child of it, anchored to the center.
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.1f;

    [Header("Smooth movement")]
    [Tooltip("How quickly the walking input ramps up and down. Higher = snappier, lower = floatier.")]
    [SerializeField] private float inputSmoothing = 14f;

    [Header("Handle feel")]
    [Tooltip("How tightly the handle follows your finger.")]
    [SerializeField] private float handleFollowSpeed = 30f;
    [Tooltip("Spring pulling the handle back to the center when you let go.")]
    [SerializeField] private float returnStiffness = 300f;
    [Tooltip("Lower = more bounce when released, higher = calmer return.")]
    [SerializeField] private float returnDamping = 13f;

    [Header("Press pop")]
    [Tooltip("The circle grows slightly while touched (1 = no change).")]
    [SerializeField] private float pressedScale = 1.08f;

    private RectTransform background;
    private Vector3 originalScale;
    private int activePointerId = int.MinValue;
    private bool held;

    private Vector2 rawInput;       // what the finger is asking for (after dead zone)
    private Vector2 handleTarget;   // where the finger puts the handle (pixels from center)
    private Vector2 handlePos;
    private Vector2 handleVelocity;

    private float circleScale = 1f;
    private float circleScaleVelocity;
    private float circleScaleTarget = 1f;

    /// X = strafe (-1 left, 1 right), Y = forward (-1 back, 1 forward). Smoothed, never longer than 1.
    public Vector2 Direction { get; private set; }

    private void Awake()
    {
        background = (RectTransform)transform;
        originalScale = background.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue) return; // already held by another finger
        activePointerId = eventData.pointerId;
        held = true;
        circleScaleTarget = pressedScale;
        UpdateFinger(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        UpdateFinger(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        activePointerId = int.MinValue;
        held = false;
        rawInput = Vector2.zero;
        handleVelocity = Vector2.zero;
        circleScaleTarget = 1f;
    }

    private void UpdateFinger(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        float radius = background.rect.width * 0.5f;
        Vector2 normalized = Vector2.ClampMagnitude(localPoint / radius, 1f);

        handleTarget = normalized * radius;
        rawInput = normalized.magnitude < deadZone ? Vector2.zero : normalized;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // Walking input eases toward the finger's position, so starting and stopping feel smooth.
        // It never overshoots, so the bounce below can't make the player walk backwards.
        Direction = Vector2.Lerp(Direction, rawInput, 1f - Mathf.Exp(-inputSmoothing * dt));
        if (rawInput == Vector2.zero && Direction.sqrMagnitude < 0.0001f) Direction = Vector2.zero;

        if (held)
        {
            // Handle glides after the finger
            handlePos = Vector2.Lerp(handlePos, handleTarget, 1f - Mathf.Exp(-handleFollowSpeed * dt));
        }
        else
        {
            // Let go: the handle springs back to the center and bounces
            UISpring.Step(ref handlePos, ref handleVelocity, Vector2.zero, returnStiffness, returnDamping, dt);
        }
        handle.anchoredPosition = handlePos;

        // The whole circle pops slightly when touched
        UISpring.Step(ref circleScale, ref circleScaleVelocity, circleScaleTarget, 400f, 16f, dt);
        background.localScale = originalScale * circleScale;
    }

    private void OnDisable()
    {
        activePointerId = int.MinValue;
        held = false;
        rawInput = Vector2.zero;
        Direction = Vector2.zero;
        handlePos = Vector2.zero;
        handleVelocity = Vector2.zero;
        circleScale = 1f;
        circleScaleVelocity = 0f;
        circleScaleTarget = 1f;

        if (handle != null) handle.anchoredPosition = Vector2.zero;
        if (background != null) background.localScale = originalScale;
    }
}