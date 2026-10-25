using UnityEngine;
using UnityEngine.UI;

/// For the Android tablet build. Left joystick moves, dragging anywhere else looks around,
/// and the bottom-right button interacts (it only shows while the reticle is on something interactable).
[RequireComponent(typeof(CharacterController))]
public class TouchPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private VirtualJoystick joystick;
    [SerializeField] private TouchLookArea lookArea;
    [SerializeField] private Button interactButton;

    [Header("Interact button")]
    [Tooltip("The button stays this long (seconds) after the reticle leaves something, so it doesn't flicker when your aim drifts across an edge.")]
    [SerializeField] private float hideDelay = 0.15f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [Tooltip("Degrees of rotation per pixel dragged")]
    [SerializeField] private float lookSensitivity = 0.15f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;
    private float lastTargetTime = -999f; // last moment the reticle was on something interactable

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        interactButton.onClick.AddListener(interactor.TryInteract);
    }

    private void OnDisable()
    {
        interactButton.onClick.RemoveListener(interactor.TryInteract);
    }

    private void Update()
    {
        bool frozen = IsFrozen();

        UpdateInteractButton(frozen);

        if (frozen) return;

        HandleLook();
        HandleMovement();
    }

    private static bool IsFrozen()
    {
        return LetterHolder.IsReading || CodeLockPanel.IsOpen || EndingSequence.IsPlaying;
    }

    // Shows the Interact button only while the reticle is on something the player can interact with.
    private void UpdateInteractButton(bool frozen)
    {
        if (!frozen && ReticleOnInteractable())
            lastTargetTime = Time.time;

        // Appears instantly, disappears after a short grace period (or right away while frozen)
        bool show = !frozen && Time.time - lastTargetTime <= hideDelay;

        // Only switch it when it changes, so the button's pop-in plays once per appearance
        if (interactButton.gameObject.activeSelf != show)
            interactButton.gameObject.SetActive(show);
    }

    private bool ReticleOnInteractable()
    {
        IInteractable target = interactor.CurrentTarget;
        if (target == null) return false;

        // A letter in your hand or pinned on the board does nothing when interacted with
        // (you use the Read button for those), so it doesn't count.
        if (target is Letter letter && letter.State != LetterState.Scattered) return false;

        return true;
    }

    private void HandleLook()
    {
        Vector2 drag = lookArea.ConsumeDelta() * lookSensitivity;

        transform.Rotate(Vector3.up * drag.x);

        pitch = Mathf.Clamp(pitch - drag.y, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 input = joystick.Direction;

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move *= moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}