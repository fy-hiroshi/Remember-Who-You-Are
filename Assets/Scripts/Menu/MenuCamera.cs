using UnityEngine;

/// Put this on the main menu's camera.
/// MoveTo() glides the camera to another view (position + rotation) with a smooth start and stop.
public class MenuCamera : MonoBehaviour
{
    [Tooltip("How long each camera move takes, in seconds.")]
    [SerializeField] private float duration = 0.9f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float progress;

    public bool IsMoving { get; private set; }

    public void MoveTo(Transform view)
    {
        // Starts from wherever the camera is right now, so it's safe to call mid-move
        startPosition = transform.position;
        startRotation = transform.rotation;
        targetPosition = view.position;
        targetRotation = view.rotation;

        progress = 0f;
        IsMoving = true;
    }

    private void Update()
    {
        if (!IsMoving) return;

        progress += Time.unscaledDeltaTime / Mathf.Max(duration, 0.01f);
        float eased = Mathf.SmoothStep(0f, 1f, progress);

        transform.SetPositionAndRotation(
            Vector3.Lerp(startPosition, targetPosition, eased),
            Quaternion.Slerp(startRotation, targetRotation, eased));

        if (progress >= 1f) IsMoving = false;
    }
}
