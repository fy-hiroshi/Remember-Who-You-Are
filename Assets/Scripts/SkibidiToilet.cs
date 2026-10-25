using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// Put this on the toilet LID, on the same object as its Openable script.
/// The first interaction (instead of just opening the lid) plays the whole sequence:
/// lid opens -> head rises -> random voice line -> head retreats -> letter pops out -> lid closes.
///
/// It plugs into Openable the same way the locks do (ILockable): while "locked", Openable hands the
/// interaction over to this script instead of toggling the lid itself.
[RequireComponent(typeof(Openable))]
public class SkibidiToilet : MonoBehaviour, ILockable
{
    [Header("Lid timing")]
    [SerializeField] private float pauseAfterLidOpens = 0.3f;
    [SerializeField] private float pauseBeforeLidCloses = 0.4f;

    [Header("Head (must NOT be a child of the lid)")]
    [Tooltip("Place it where the head should be when fully risen. The script hides it below that spot.")]
    [SerializeField] private Transform head;
    [Tooltip("How far below its risen spot the head hides inside the toilet (in the head's parent space).")]
    [SerializeField] private Vector3 hiddenOffset = new Vector3(0f, -0.6f, 0f);
    [SerializeField] private float riseSeconds = 0.5f;
    [SerializeField] private float retreatSeconds = 0.4f;
    [SerializeField] private bool faceThePlayer = true;
    [Tooltip("If the head faces the wrong way, try 180, 90 or -90.")]
    [SerializeField] private float facingOffsetDegrees = 0f;
    [Tooltip("Little up-and-down bounce while he talks. Set to 0 for none.")]
    [SerializeField] private float bobAmount = 0.03f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("Voice lines")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("One clip per friend. A random one plays (never the same one twice in a row).")]
    [SerializeField] private AudioClip[] voiceLines;
    [SerializeField] private float pauseAfterVoice = 0.5f;
    [Tooltip("How long the head stays up if no voice lines are assigned.")]
    [SerializeField] private float fallbackStaySeconds = 2f;

    [Header("Letter that pops out")]
    [SerializeField] private Letter letter;
    [Tooltip("Where the letter starts, hidden inside the toilet (position and rotation are used).")]
    [SerializeField] private Transform letterStart;
    [Tooltip("Where the letter lands (position and rotation are used). Keep it clear of the lid's swing.")]
    [SerializeField] private Transform letterLanding;
    [SerializeField] private float popSeconds = 0.7f;
    [SerializeField] private float popHeight = 0.6f;

    [Header("Replay")]
    [Tooltip("If ticked, every interaction replays the greeting with a new random voice line. The letter only pops out the first time.")]
    [SerializeField] private bool replayable = false;

    [Header("Hooks")]
    [Tooltip("Fires once, the first time the whole sequence has finished (lid closed). Use it to unlock the furniture.")]
    public UnityEvent onFirstSequenceComplete;

    private Openable lid;
    private Vector3 headShownPos;
    private Vector3 headHiddenPos;
    private bool playing;
    private bool letterPopped;
    private bool firstSequenceDone;
    private int lastClipIndex = -1;

    // While true, Openable hands interactions to OnLockedInteract (below) instead of toggling the lid.
    public bool IsLocked { get; private set; } = true;

    private void Awake()
    {
        lid = GetComponent<Openable>();

        if (head != null)
        {
            headShownPos = head.localPosition;
            headHiddenPos = headShownPos + hiddenOffset;
            head.localPosition = headHiddenPos;
            head.gameObject.SetActive(false);
        }

        // The letter can't be grabbed before the sequence has played
        if (letter != null) letter.gameObject.SetActive(false);
    }

    public void OnLockedInteract(PlayerInteractor interactor)
    {
        if (playing) return; // ignore button mashing during the sequence
        StartCoroutine(Sequence(interactor.transform));
    }

    private IEnumerator Sequence(Transform player)
    {
        playing = true;

        // 1. Lid opens
        lid.Open();
        yield return new WaitUntil(() => !lid.IsMoving);
        yield return new WaitForSeconds(pauseAfterLidOpens);

        // 2. Head rises
        if (head != null)
        {
            head.gameObject.SetActive(true);
            if (faceThePlayer) FacePlayer(player);
            yield return MoveHead(headHiddenPos, headShownPos, riseSeconds);
        }

        // 3. Voice line (with a little bounce while he talks)
        AudioClip clip = PickClip();
        float stay = fallbackStaySeconds;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            stay = clip.length + pauseAfterVoice;
        }

        for (float t = 0f; t < stay; t += Time.deltaTime)
        {
            if (head != null)
                head.localPosition = headShownPos + Vector3.up * (Mathf.Sin(t * bobSpeed) * bobAmount);
            yield return null;
        }

        // 4. Head retreats into the toilet
        if (head != null)
        {
            yield return MoveHead(head.localPosition, headHiddenPos, retreatSeconds);
            head.gameObject.SetActive(false);
        }

        // 5. Letter pops out (first time only)
        if (!letterPopped && letter != null)
        {
            letterPopped = true;
            yield return PopLetter();
        }

        // 6. Lid closes
        yield return new WaitForSeconds(pauseBeforeLidCloses);
        lid.Close();
        yield return new WaitUntil(() => !lid.IsMoving);

        // Afterwards the lid behaves like a normal Openable, unless the greeting should replay
        if (!replayable) IsLocked = false;
        playing = false;

        if (!firstSequenceDone)
        {
            firstSequenceDone = true;
            onFirstSequenceComplete.Invoke();
        }
    }

    private IEnumerator MoveHead(Vector3 from, Vector3 to, float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            head.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds));
            yield return null;
        }
        head.localPosition = to;
    }

    private IEnumerator PopLetter()
    {
        Transform lt = letter.transform;

        // No physics while it flies, and no grabbing it mid-air
        Rigidbody rb = letter.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        SetLetterColliders(false);

        lt.SetPositionAndRotation(letterStart.position, letterStart.rotation);
        letter.gameObject.SetActive(true);

        for (float t = 0f; t < popSeconds; t += Time.deltaTime)
        {
            float k = t / popSeconds;
            Vector3 pos = Vector3.Lerp(letterStart.position, letterLanding.position, k);
            pos.y += popHeight * 4f * k * (1f - k); // arcs up and back down
            lt.SetPositionAndRotation(pos, Quaternion.Slerp(letterStart.rotation, letterLanding.rotation, k));
            yield return null;
        }

        lt.SetPositionAndRotation(letterLanding.position, letterLanding.rotation);
        SetLetterColliders(true); // now the player can pick it up
    }

    private void SetLetterColliders(bool on)
    {
        foreach (Collider c in letter.GetComponentsInChildren<Collider>(true))
            c.enabled = on;
    }

    private AudioClip PickClip()
    {
        if (voiceLines == null || voiceLines.Length == 0) return null;
        if (voiceLines.Length == 1) return voiceLines[0];

        int index;
        do { index = Random.Range(0, voiceLines.Length); }
        while (index == lastClipIndex);

        lastClipIndex = index;
        return voiceLines[index];
    }

    // Turns the head sideways-only so it looks at the player (keeps the model's own tilt)
    private void FacePlayer(Transform player)
    {
        Vector3 toPlayer = player.position - head.position;
        toPlayer.y = 0f;
        Vector3 forward = head.forward;
        forward.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f) return;

        float angle = Vector3.SignedAngle(forward, toPlayer, Vector3.up) + facingOffsetDegrees;
        head.Rotate(0f, angle, 0f, Space.World);
    }
}