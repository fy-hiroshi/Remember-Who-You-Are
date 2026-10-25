using UnityEngine;

/// For testing in the editor / on PC. WASD to move, mouse to look, E to interact, Esc to free the cursor.
[RequireComponent(typeof(CharacterController))]
public class PCPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerInteractor interactor;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;
    private bool cursorLocked;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable() => SetCursorLocked(true);
    private void OnDisable() => SetCursorLocked(false);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SetCursorLocked(false);

        if (!cursorLocked)
        {
            // Click the game window to grab the mouse again
            if (Input.GetMouseButtonDown(0)) SetCursorLocked(true);
            return;
        }

        if (LetterHolder.IsReading || CodeLockPanel.IsOpen || EndingSequence.IsPlaying) return;

        HandleLook();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.E))
            interactor.TryInteract();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch = Mathf.Clamp(pitch - mouseY, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z);
        move = Vector3.ClampMagnitude(move, 1f) * moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}