using UnityEngine;
using UnityEngine.Events;

/// Put this on the furniture, on the SAME object as its Openable script.
/// The furniture can't be moved until something calls Unlock() (e.g. the toilet sequence finishing).
/// The first interaction after that moves it and reveals the hidden letter.
[RequireComponent(typeof(Openable))]
public class EventLock : MonoBehaviour, ILockable
{
    [Tooltip("Optional: the letter hidden behind the furniture. It's switched off until the furniture first moves, so it can't be grabbed early.")]
    [SerializeField] private Letter letterToReveal;

    [Tooltip("Off = once moved, the furniture stays moved (so it can't cover the letter again). On = it can be pushed back and forth.")]
    [SerializeField] private bool canCloseAgain = false;

    [Header("Hooks (optional): play a sound, show a message, etc.")]
    [Tooltip("Fires when the player tries the furniture before it's unlocked.")]
    public UnityEvent onTriedWhileLocked;
    [Tooltip("Fires once, when the furniture moves and the letter is revealed.")]
    public UnityEvent onRevealed;

    private Openable openable;
    private bool unlocked;
    private bool opened;

    // Stays true so Openable hands the interaction to this script (see OnLockedInteract)
    public bool IsLocked { get; private set; } = true;

    private void Awake()
    {
        openable = GetComponent<Openable>();
        if (letterToReveal != null) letterToReveal.gameObject.SetActive(false);
    }

    /// Hook this up to the toilet's "On First Sequence Complete" event.
    public void Unlock()
    {
        unlocked = true;
    }

    public void OnLockedInteract(PlayerInteractor interactor)
    {
        if (opened) return; // already moved, and it stays that way

        if (!unlocked)
        {
            onTriedWhileLocked.Invoke();
            return;
        }

        opened = true;
        if (letterToReveal != null) letterToReveal.gameObject.SetActive(true);
        openable.Open();
        onRevealed.Invoke();

        // If it may be moved again, hand control back to Openable's normal toggling
        if (canCloseAgain) IsLocked = false;
    }
}
