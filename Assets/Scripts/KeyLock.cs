using UnityEngine;
using UnityEngine.Events;

/// Put this on the SAME object as the door's Openable script.
/// While locked, interacting checks whether the player has the key. If so, the door unlocks and opens.
[RequireComponent(typeof(Openable))]
public class KeyLock : MonoBehaviour, ILockable
{
    [Tooltip("If ticked, the key disappears from the player's left hand once it's used.")]
    [SerializeField] private bool consumeKey = true;

    [Header("Hooks (optional): play a sound, show a message, etc.")]
    [Tooltip("Fires when the player tries the door without the key.")]
    public UnityEvent onLockedNoKey;
    [Tooltip("Fires once, when the key unlocks the door.")]
    public UnityEvent onUnlocked;

    private Openable openable;

    public bool IsLocked { get; private set; } = true;

    private void Awake()
    {
        openable = GetComponent<Openable>();
    }

    public void OnLockedInteract(PlayerInteractor interactor)
    {
        LetterHolder holder = interactor.GetComponent<LetterHolder>();

        if (holder == null || !holder.HasKey)
        {
            onLockedNoKey.Invoke();
            return;
        }

        IsLocked = false;
        if (consumeKey) holder.UseKey();

        onUnlocked.Invoke();
        openable.Open();
    }
}
