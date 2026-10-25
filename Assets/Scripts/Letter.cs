using UnityEngine;

public enum LetterState
{
    Scattered, // lying around the map
    Held,      // in the player's hand
    Pinned     // on the cork board
}

/// Put this on each letter in the map. The letter also needs a Collider.
public class Letter : MonoBehaviour, IInteractable
{
    [Tooltip("The image shown when the player reads this letter (import it as Sprite (2D and UI)).")]
    [SerializeField] private Sprite content;

    [Header("Optional: a key attached to this letter")]
    [Tooltip("If ticked, a key is shown on this letter in the reader until the player takes it.")]
    [SerializeField] private bool hasAttachedKey = false;

    [Header("Sounds (optional, leave empty for silence)")]
    [Tooltip("Plays when the player picks this letter up.")]
    [SerializeField] private SoundSet pickUpSound;
    [Tooltip("Plays when this letter gets pinned on the cork board.")]
    [SerializeField] private SoundSet pinSound;

    private bool keyTaken;

    public Sprite Content => content;
    public LetterState State { get; private set; } = LetterState.Scattered;

    /// True while the key is still on the letter. The key lives on the Letter itself, so it stays
    /// there whether the letter is lying around, in the hand, or pinned, until the player takes it.
    public bool KeyAvailable => hasAttachedKey && !keyTaken;

    public void TakeKey()
    {
        keyTaken = true;
    }

    public void Interact(PlayerInteractor interactor)
    {
        // Only scattered letters can be picked up
        if (State != LetterState.Scattered) return;

        LetterHolder holder = interactor.GetComponent<LetterHolder>();
        if (holder != null) holder.PickUp(this);
    }

    /// Called by LetterHolder: hide the world copy, since the hand model is shown instead.
    public void SetHeld()
    {
        State = LetterState.Held;

        // The sound plays from its own temporary object, so switching this letter off doesn't cut it short
        if (pickUpSound != null) pickUpSound.PlayAt(transform.position);

        gameObject.SetActive(false);
    }

    /// Called by CorkBoard: snap the letter onto a pin slot.
    public void PinTo(Transform slot)
    {
        State = LetterState.Pinned;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(true);

        if (pinSound != null) pinSound.PlayAt(transform.position);
    }
}