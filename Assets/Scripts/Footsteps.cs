using UnityEngine;

/// Put this on the Player (next to the CharacterController).
/// Plays a random footstep sound every time the player has walked a certain distance.
/// It watches how far the player actually moves, so it works with both the PC and the touch controller,
/// and stays silent whenever the player is frozen (reading a letter, using the keypad, the ending).
[RequireComponent(typeof(CharacterController))]
public class Footsteps : MonoBehaviour
{
    [Header("Sounds")]
    [Tooltip("Drag ALL of your footstep files in here. One is picked at random for every step, never the same one twice in a row.")]
    [SerializeField] private AudioClip[] footstepClips;
    [Tooltip("Optional. If left empty, one is created automatically (2D, so it's always clear and centered).")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    [Header("Timing")]
    [Tooltip("How far the player walks between steps, in meters. Bigger = slower footsteps, smaller = faster.")]
    [SerializeField] private float stepDistance = 1.8f;
    [Tooltip("Slower than this (meters per second) doesn't count as walking.")]
    [SerializeField] private float minSpeed = 0.3f;

    [Header("Variation (keeps it from sounding robotic)")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);
    [Tooltip("Each step's volume is multiplied by a random number in this range.")]
    [SerializeField] private Vector2 volumeVariation = new Vector2(0.85f, 1f);

    private CharacterController controller;
    private Vector3 lastPosition;
    private float distanceSinceStep;
    private int lastClipIndex = -1;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D
        }
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    // LateUpdate so the controller has already moved the player this frame
    private void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        delta.y = 0f; // only sideways movement counts

        float distance = delta.magnitude;
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);

        // distance > 2 m in one frame means a teleport, not a walk
        bool walking = controller.isGrounded && speed >= minSpeed && distance < 2f;

        if (!walking)
        {
            // Standing still: get ready so the first step comes quickly when walking starts
            distanceSinceStep = stepDistance * 0.6f;
            return;
        }

        distanceSinceStep += distance;
        if (distanceSinceStep >= stepDistance)
        {
            distanceSinceStep -= stepDistance;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index = 0;
        if (footstepClips.Length > 1)
        {
            do { index = Random.Range(0, footstepClips.Length); }
            while (index == lastClipIndex);
        }
        lastClipIndex = index;

        AudioClip clip = footstepClips[index];
        if (clip == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, volume * Random.Range(volumeVariation.x, volumeVariation.y));
    }
}
