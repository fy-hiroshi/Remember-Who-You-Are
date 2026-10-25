using UnityEngine;
using UnityEngine.UI;

/// Put this on the Player (next to PlayerInteractor).
/// Handles: the hand model, the Read button, the letter reader panel, and a key attached to a letter.
/// The player carries one letter at a time: pick it up, pin it, then go find the next.
[RequireComponent(typeof(PlayerInteractor))]
public class LetterHolder : MonoBehaviour
{
    [Header("Hand (holds the letter)")]
    [SerializeField] private GameObject handModel;

    [Header("Read button + reader UI")]
    [SerializeField] private Button readButton;
    [SerializeField] private GameObject readerPanel;
    [SerializeField] private Image readerImage;
    [SerializeField] private Button closeButton;

    [Header("Key attached to a letter (optional)")]
    [Tooltip("The clickable key picture inside the reader panel (a RawImage with a Button on it).")]
    [SerializeField] private Button keyButton;
    [Tooltip("The far-away rig with the key camera and 3D key. Only switched on while the key is on screen.")]
    [SerializeField] private GameObject keyViewerRig;
    [Tooltip("The left hand holding the key. Switched on once the player takes the key.")]
    [SerializeField] private GameObject leftHandKeyModel;

    private PlayerInteractor interactor;
    private Letter heldLetter;
    private Letter shownLetter; // the letter currently open in the reader

    /// True while a letter is open on screen. Controllers use this to freeze movement.
    public static bool IsReading { get; private set; }

    public Letter HeldLetter => heldLetter;

    /// True once the player has taken the key (other scripts can check this, e.g. a locked door).
    public bool HasKey { get; private set; }

    private void Awake()
    {
        interactor = GetComponent<PlayerInteractor>();
        readerImage.preserveAspect = true;

        handModel.SetActive(false);
        readButton.gameObject.SetActive(false);
        readerPanel.SetActive(false);

        if (leftHandKeyModel != null) leftHandKeyModel.SetActive(false);
        ShowKeyUI(false);
    }

    private void OnEnable()
    {
        readButton.onClick.AddListener(ToggleReader);
        closeButton.onClick.AddListener(CloseReader);
        if (keyButton != null) keyButton.onClick.AddListener(TakeKey);
    }

    private void OnDisable()
    {
        readButton.onClick.RemoveListener(ToggleReader);
        closeButton.onClick.RemoveListener(CloseReader);
        if (keyButton != null) keyButton.onClick.RemoveListener(TakeKey);
        IsReading = false;
    }

    private void Update()
    {
        // PC testing shortcuts (the tablet uses the on-screen buttons)
        if (Input.GetKeyDown(KeyCode.R)) ToggleReader();
        if (IsReading && Input.GetKeyDown(KeyCode.K)) TakeKey();

        // Show the Read button when there's something to read
        bool show = IsReading || GetReadTarget() != null;
        readButton.gameObject.SetActive(show);
    }

    /// Returns false if the hand is already full.
    public bool PickUp(Letter letter)
    {
        if (heldLetter != null) return false;

        heldLetter = letter;
        letter.SetHeld();
        handModel.SetActive(true);
        return true;
    }

    /// Empties the hand and returns the letter (used when pinning to the cork board).
    public Letter Release()
    {
        Letter letter = heldLetter;
        heldLetter = null;
        handModel.SetActive(false);
        if (IsReading) CloseReader();
        return letter;
    }

    // A pinned letter you're looking at takes priority, otherwise the letter in your hand.
    private Letter GetReadTarget()
    {
        Letter looked = interactor.CurrentTarget as Letter;
        if (looked != null && looked.State == LetterState.Pinned) return looked;
        return heldLetter;
    }

    private void ToggleReader()
    {
        if (IsReading)
        {
            CloseReader();
            return;
        }

        Letter target = GetReadTarget();
        if (target == null) return;

        shownLetter = target;
        readerImage.sprite = target.Content;
        readerPanel.SetActive(true);
        IsReading = true;

        // The key is shown every time the letter is opened, until the player takes it
        ShowKeyUI(target.KeyAvailable);
    }

    private void CloseReader()
    {
        readerPanel.SetActive(false);
        IsReading = false;
        shownLetter = null;
        ShowKeyUI(false);
    }

    private void TakeKey()
    {
        if (!IsReading || shownLetter == null || !shownLetter.KeyAvailable) return;

        shownLetter.TakeKey();   // the key is now gone from the letter for good
        HasKey = true;
        if (leftHandKeyModel != null) leftHandKeyModel.SetActive(true);
        ShowKeyUI(false);
    }

    /// Called by a lock when the key is used up: the key leaves the player's left hand.
    public void UseKey()
    {
        HasKey = false;
        if (leftHandKeyModel != null) leftHandKeyModel.SetActive(false);
    }

    private void ShowKeyUI(bool show)
    {
        if (keyButton != null) keyButton.gameObject.SetActive(show);
        if (keyViewerRig != null) keyViewerRig.SetActive(show);
    }
}