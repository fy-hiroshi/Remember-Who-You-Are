using UnityEngine;
using UnityEngine.Events;

/// Put this on the cork board (it needs a Collider).
/// Create one empty child object per letter and assign them to Pin Slots.
public class CorkBoard : MonoBehaviour, IInteractable
{
    [Tooltip("Where each letter gets pinned, in order. The slot's position and rotation decide how the letter sits.")]
    [SerializeField] private Transform[] pinSlots;

    [Tooltip("Fires once, when the last slot is filled.")]
    public UnityEvent onBoardComplete;

    private int pinnedCount;

    public void Interact(PlayerInteractor interactor)
    {
        LetterHolder holder = interactor.GetComponent<LetterHolder>();
        if (holder == null || holder.HeldLetter == null) return;
        if (pinnedCount >= pinSlots.Length) return;

        Letter letter = holder.Release();
        letter.PinTo(pinSlots[pinnedCount]);
        pinnedCount++;

        if (pinnedCount == pinSlots.Length) onBoardComplete.Invoke();
    }
}
