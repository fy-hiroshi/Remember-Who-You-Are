using UnityEngine;

/// Put this on anything the player can interact with (letters, cabinet doors, the cork board...).
public interface IInteractable
{
    void Interact(PlayerInteractor interactor);
}

/// Shared by both the PC and Touch controllers. Casts a ray from the center of the camera.
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    /// Whatever the reticle is currently pointing at (null if nothing interactable).
    public IInteractable CurrentTarget { get; private set; }

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void Update()
    {
        CurrentTarget = FindTarget();
    }

    public void TryInteract()
    {
        IInteractable target = FindTarget();
        if (target != null) target.Interact(this);
    }

    private IInteractable FindTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<IInteractable>();

        return null;
    }
}