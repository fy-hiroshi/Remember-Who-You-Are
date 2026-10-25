using UnityEngine;

public enum OpenType
{
    Rotate, // hinged: doors, cabinet flaps
    Slide   // drawers, shutters
}

/// One script for every openable object. Configure it per object in the Inspector.
/// Interact once to open, again to close.
public class Openable : MonoBehaviour, IInteractable
{
    [Header("What moves")]
    [Tooltip("The part that moves. Leave empty to move this object.")]
    [SerializeField] private Transform target;

    [Header("How it opens")]
    [SerializeField] private OpenType openType = OpenType.Rotate;
    [Tooltip("Local axis: (1,0,0) = X, (0,1,0) = Y, (0,0,1) = Z")]
    [SerializeField] private Vector3 axis = Vector3.up;
    [Tooltip("Rotate: degrees. Slide: distance in meters. Use a negative value to flip direction.")]
    [SerializeField] private float amount = 90f;
    [SerializeField] private float duration = 0.6f;

    [Header("Sounds (optional, leave empty for silence)")]
    [Tooltip("Plays whenever it starts opening, no matter what triggered it (player, lock, or a scripted sequence).")]
    [SerializeField] private SoundSet openSound;
    [Tooltip("Plays whenever it starts closing.")]
    [SerializeField] private SoundSet closeSound;

    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private float progress; // 0 = closed, 1 = open
    private bool isOpen;

    public bool IsOpen => isOpen;

    private ILockable lockable;

    private void Awake()
    {
        if (target == null) target = transform;
        closedPosition = target.localPosition;
        closedRotation = target.localRotation;

        // Optional: a lock on the same object (e.g. CombinationLock)
        lockable = GetComponent<ILockable>();
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (lockable != null && lockable.IsLocked)
        {
            lockable.OnLockedInteract(interactor);
            return;
        }

        SetOpen(!isOpen);
    }

    /// Opens without toggling (used by locks right after they unlock).
    public void Open()
    {
        SetOpen(true);
    }

    /// Closes without toggling (used by scripted sequences).
    public void Close()
    {
        SetOpen(false);
    }

    // Every open/close goes through here, so the sounds play for all of them.
    // Asking for the state it's already in does nothing (and makes no sound).
    private void SetOpen(bool open)
    {
        if (isOpen == open) return;
        isOpen = open;

        SoundSet sound = open ? openSound : closeSound;
        if (sound != null) sound.PlayAt(target != null ? target.position : transform.position);
    }

    /// True while the open/close animation is still playing.
    public bool IsMoving => !Mathf.Approximately(progress, isOpen ? 1f : 0f);

    private void Update()
    {
        float goal = isOpen ? 1f : 0f;
        if (Mathf.Approximately(progress, goal)) return;

        progress = Mathf.MoveTowards(progress, goal, Time.deltaTime / Mathf.Max(duration, 0.01f));
        float eased = Mathf.SmoothStep(0f, 1f, progress);
        Vector3 dir = axis.normalized;

        if (openType == OpenType.Rotate)
            target.localRotation = closedRotation * Quaternion.AngleAxis(amount * eased, dir);
        else
            target.localPosition = closedPosition + dir * (amount * eased);
    }
}