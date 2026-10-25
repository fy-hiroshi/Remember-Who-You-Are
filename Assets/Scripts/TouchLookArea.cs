using UnityEngine;
using UnityEngine.EventSystems;

/// Attach to a full-screen transparent UI Image placed BEHIND the joystick and button.
/// Collects finger drag distance (in pixels) for the camera to read each frame.
public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private int activePointerId = int.MinValue;
    private Vector2 accumulatedDelta;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue) return;
        activePointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        accumulatedDelta += eventData.delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        activePointerId = int.MinValue;
    }

    /// Returns the drag since the last call, then resets it.
    public Vector2 ConsumeDelta()
    {
        Vector2 d = accumulatedDelta;
        accumulatedDelta = Vector2.zero;
        return d;
    }

    private void OnDisable()
    {
        activePointerId = int.MinValue;
        accumulatedDelta = Vector2.zero;
    }
}
