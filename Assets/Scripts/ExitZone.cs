using UnityEngine;

/// Put this on an empty object with a Collider just outside the exit door (in the white space).
/// The Collider must have Is Trigger ticked. When the player walks in, the ending starts (once).
[RequireComponent(typeof(Collider))]
public class ExitZone : MonoBehaviour
{
    [SerializeField] private EndingSequence ending;

    private bool triggered;

    private void Reset()
    {
        // Makes the collider a trigger automatically when you first add this script
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.GetComponentInParent<PlayerInteractor>() == null) return; // only the player counts

        triggered = true;
        ending.Begin();
    }
}
