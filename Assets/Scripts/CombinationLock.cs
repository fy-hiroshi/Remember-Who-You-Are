using UnityEngine;

/// Anything that can block an Openable from opening (combination lock now, maybe a key lock later).
public interface ILockable
{
    bool IsLocked { get; }
    void OnLockedInteract(PlayerInteractor interactor);
}

/// Put this on the SAME object as the cabinet's Openable script.
/// While locked, interacting shows the code pad instead of opening the cabinet.
[RequireComponent(typeof(Openable))]
public class CombinationLock : MonoBehaviour, ILockable
{
    [Tooltip("Digits only, e.g. 4271. Any length works.")]
    [SerializeField] private string code = "1234";

    [Tooltip("The object in your Canvas that has the CodeLockPanel script.")]
    [SerializeField] private CodeLockPanel panel;

    private Openable openable;

    public bool IsLocked { get; private set; } = true;
    public int CodeLength => code.Length;

    private void Awake()
    {
        openable = GetComponent<Openable>();
    }

    public void OnLockedInteract(PlayerInteractor interactor)
    {
        panel.Show(this);
    }

    /// Returns true (and unlocks for good) if the attempt matches the code.
    public bool CheckCode(string attempt)
    {
        if (attempt != code) return false;
        IsLocked = false;
        return true;
    }

    /// Called by the panel after a correct code, so the cabinet swings open right away.
    public void OpenCabinet()
    {
        openable.Open();
    }
}
