using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Put this on the Canvas of the main menu scene.
/// Open(): the menu eases out, the camera turns to the first letter on the corkboard, and the player can
/// browse the letters with Previous / Next (wrapping around), read each one, and go Back to the menu.
public class LetterGallery : MonoBehaviour
{
    [Header("Camera views")]
    [SerializeField] private MenuCamera menuCamera;
    [Tooltip("The camera's normal main menu view.")]
    [SerializeField] private Transform menuView;
    [Tooltip("One per letter, in the order you want to browse them. Frame each letter in the center of the screen.")]
    [SerializeField] private Transform[] letterViews;

    [Header("Letter images (same order as the views)")]
    [SerializeField] private Sprite[] letterSprites;

    [Header("Who each letter is from (optional, same order as the views)")]
    [Tooltip("One name per letter. It's free text, so you can write just 'Alex' or something like 'From Alex'.")]
    [SerializeField] private string[] letterAuthors;
    [Tooltip("The text shown above the letter. It needs a UIPanelTransition on the same object (tick Start Hidden). Leave empty to turn names off.")]
    [SerializeField] private TMP_Text authorText;

    [Header("Voice lines (optional, same order as the views)")]
    [Tooltip("One recording per letter, in the same order as Letter Views. A letter with an empty slot simply has no voice button.")]
    [SerializeField] private AudioClip[] letterVoiceLines;
    [Tooltip("The button that plays the current letter's voice line. Put it beside the Read button, NOT in the reader panel. It needs a UIPanelTransition on the same object (tick Start Hidden). Leave empty to turn voice lines off.")]
    [SerializeField] private Button voiceButton;
    [Tooltip("Optional. If left empty, one is created automatically (plain 2D sound).")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;

    [Header("UI that eases in and out")]
    [Tooltip("The group holding the Play / Letter Gallery / Exit buttons.")]
    [SerializeField] private UIPanelTransition mainMenuUI;
    [Tooltip("The group holding Previous, Next, Back and the Read button. Tick Start Hidden on it.")]
    [SerializeField] private UIPanelTransition galleryUI;
    [Tooltip("The Read button, placed below the letter. It hides while the camera moves. Tick Start Hidden on it.")]
    [SerializeField] private UIPanelTransition readButtonUI;

    [Header("Buttons")]
    [SerializeField] private Button readButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Reader panel")]
    [SerializeField] private GameObject readerPanel;
    [SerializeField] private Image readerImage;
    [SerializeField] private Button closeButton;

    private int index;
    private bool isOpen;
    private bool readerOpen;
    private bool configValid;
    private UIPanelTransition authorUI; // the author text's own show/hide animation
    private UIPanelTransition voiceUI;  // the voice button's own show/hide animation
    private bool waitingToShowRead; // camera is heading to a letter
    private bool waitingToShowMenu; // camera is heading back to the menu view

    private int Count => letterViews.Length;

    private void Awake()
    {
        // Always start with the reader closed, even if the setup check below finds a problem
        if (readerPanel != null) readerPanel.SetActive(false);

        configValid = CheckSetup();
        if (!configValid) return;

        readerImage.preserveAspect = true;

        // Voice lines play through a plain 2D audio source
        if (voiceButton != null && voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;
        }
    }

    private void OnEnable()
    {
        if (!configValid) return;

        readButton.onClick.AddListener(OpenReader);
        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);
        backButton.onClick.AddListener(Back);
        closeButton.onClick.AddListener(CloseReader);
        if (voiceButton != null) voiceButton.onClick.AddListener(PlayVoice);
    }

    private void OnDisable()
    {
        if (!configValid) return;

        readButton.onClick.RemoveListener(OpenReader);
        previousButton.onClick.RemoveListener(Previous);
        nextButton.onClick.RemoveListener(Next);
        backButton.onClick.RemoveListener(Back);
        closeButton.onClick.RemoveListener(CloseReader);
        if (voiceButton != null) voiceButton.onClick.RemoveListener(PlayVoice);
    }

    /// Called by the main menu's Letter Gallery button.
    public void Open()
    {
        if (!configValid)
        {
            Debug.LogError("LetterGallery: can't open, because something isn't set up. " +
                           "Scroll up in the Console to the red messages from when the scene started.", this);
            return;
        }

        if (isOpen) return;

        isOpen = true;
        waitingToShowMenu = false;

        mainMenuUI.Hide();
        galleryUI.Show();
        GoToLetter(0);
    }

    private void Update()
    {
        if (!configValid) return;

        // Once the camera settles, bring back the Read button / the main menu
        if (waitingToShowRead && !menuCamera.IsMoving)
        {
            waitingToShowRead = false;
            ShowAuthor();
            ShowVoiceButton();
            readButtonUI.Show();
        }

        if (waitingToShowMenu && !menuCamera.IsMoving)
        {
            waitingToShowMenu = false;
            mainMenuUI.Show();
        }

        if (!isOpen) return;

        // Android's back button / Esc, and arrow keys for PC testing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (readerOpen) CloseReader();
            else Back();
        }
        else if (!readerOpen)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) Next();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Previous();
        }
    }

    private void GoToLetter(int newIndex)
    {
        index = newIndex;
        readButtonUI.Hide();
        if (authorUI != null) authorUI.Hide();
        if (voiceUI != null) voiceUI.Hide();
        StopVoice(); // moving on to another letter ends the current recording
        waitingToShowRead = true;
        menuCamera.MoveTo(letterViews[index]);
    }

    // Swaps in the name for the current letter and shows it (called once the camera has settled,
    // so the old name has already faded out and the player never sees the text change)
    private void ShowAuthor()
    {
        if (authorUI == null) return;

        string who = (letterAuthors != null && index < letterAuthors.Length) ? letterAuthors[index] : "";
        authorText.text = who;

        if (!string.IsNullOrWhiteSpace(who)) authorUI.Show();
    }

    // Shows the voice button, but only if the current letter has a recording
    private void ShowVoiceButton()
    {
        if (voiceUI == null) return;

        if (CurrentVoiceLine() != null) voiceUI.Show();
    }

    private AudioClip CurrentVoiceLine()
    {
        if (letterVoiceLines == null || index >= letterVoiceLines.Length) return null;
        return letterVoiceLines[index];
    }

    // Plays the current letter's recording. Pressing again restarts it,
    // so quick taps never pile voices on top of each other.
    private void PlayVoice()
    {
        if (!isOpen || voiceSource == null) return;

        AudioClip clip = CurrentVoiceLine();
        if (clip == null) return;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.volume = voiceVolume;
        voiceSource.Play();
    }

    private void StopVoice()
    {
        if (voiceSource != null) voiceSource.Stop();
    }

    // Wraps around at both ends, like a carousel
    private void Next()
    {
        if (!isOpen || readerOpen) return;
        GoToLetter((index + 1) % Count);
    }

    private void Previous()
    {
        if (!isOpen || readerOpen) return;
        GoToLetter((index - 1 + Count) % Count);
    }

    private void Back()
    {
        if (!isOpen) return;

        CloseReader();
        isOpen = false;
        waitingToShowRead = false;

        readButtonUI.Hide();
        if (authorUI != null) authorUI.Hide();
        if (voiceUI != null) voiceUI.Hide();
        StopVoice();
        galleryUI.Hide();
        menuCamera.MoveTo(menuView);
        waitingToShowMenu = true;
    }

    private void OpenReader()
    {
        if (!isOpen || readerOpen) return;

        readerImage.sprite = letterSprites[index];
        readerPanel.SetActive(true);
        readerOpen = true;
    }

    private void CloseReader()
    {
        readerPanel.SetActive(false);
        readerOpen = false;
    }

    // ---- Setup check: tells you in the Console exactly what is missing ----

    private bool CheckSetup()
    {
        bool ok = true;

        ok &= Assigned(menuCamera, "Menu Camera");
        ok &= Assigned(menuView, "Menu View");
        ok &= Assigned(mainMenuUI, "Main Menu UI");
        ok &= Assigned(galleryUI, "Gallery UI");
        ok &= Assigned(readButtonUI, "Read Button UI");
        ok &= Assigned(readButton, "Read Button");
        ok &= Assigned(previousButton, "Previous Button");
        ok &= Assigned(nextButton, "Next Button");
        ok &= Assigned(backButton, "Back Button");
        ok &= Assigned(readerPanel, "Reader Panel");
        ok &= Assigned(readerImage, "Reader Image");
        ok &= Assigned(closeButton, "Close Button");

        if (letterViews == null || letterViews.Length == 0)
        {
            Debug.LogError("LetterGallery: 'Letter Views' is empty. Add one view object per letter.", this);
            ok = false;
        }
        else
        {
            for (int i = 0; i < letterViews.Length; i++)
                ok &= Assigned(letterViews[i], "Letter Views, element " + i);
        }

        if (letterSprites == null || letterSprites.Length == 0)
        {
            Debug.LogError("LetterGallery: 'Letter Sprites' is empty. Add one sprite per letter.", this);
            ok = false;
        }
        else
        {
            for (int i = 0; i < letterSprites.Length; i++)
                ok &= Assigned(letterSprites[i], "Letter Sprites, element " + i);
        }

        if (letterViews != null && letterSprites != null && letterViews.Length != letterSprites.Length)
        {
            Debug.LogError("LetterGallery: 'Letter Views' has " + letterViews.Length + " entries but 'Letter Sprites' has " +
                           letterSprites.Length + ". They need the same number.", this);
            ok = false;
        }

        // Optional: who each letter is from
        if (authorText != null)
        {
            authorUI = authorText.GetComponent<UIPanelTransition>();

            if (authorUI == null)
            {
                Debug.LogError("LetterGallery: the 'Author Text' object needs a UIPanelTransition component on it " +
                               "(tick 'Start Hidden'). Add it to the same object as the text.", this);
                ok = false;
            }
            else
            {
                ok &= IsActive(authorUI, "Author Text");
            }

            if (letterViews != null && (letterAuthors == null || letterAuthors.Length != letterViews.Length))
            {
                Debug.LogWarning("LetterGallery: 'Letter Authors' should have one name per letter (" + letterViews.Length +
                                 "). Letters without a name will show no text above them.", this);
            }
        }

        // Optional: the voice line button
        if (voiceButton != null)
        {
            voiceUI = voiceButton.GetComponent<UIPanelTransition>();

            if (voiceUI == null)
            {
                Debug.LogError("LetterGallery: the 'Voice Button' needs a UIPanelTransition component on it " +
                               "(tick 'Start Hidden'), the same as the Read button.", this);
                ok = false;
            }
            else
            {
                ok &= IsActive(voiceUI, "Voice Button");
            }

            if (letterVoiceLines == null || letterVoiceLines.Length == 0)
            {
                Debug.LogWarning("LetterGallery: 'Letter Voice Lines' is empty, so the voice button will never show.", this);
            }
            else if (letterViews != null && letterVoiceLines.Length != letterViews.Length)
            {
                Debug.LogWarning("LetterGallery: 'Letter Voice Lines' has " + letterVoiceLines.Length + " entries but there are " +
                                 letterViews.Length + " letters. Letters without a recording get no voice button.", this);
            }
        }

        // The objects that animate must be active, or their coroutines can't run
        ok &= IsActive(mainMenuUI, "Main Menu UI");
        ok &= IsActive(galleryUI, "Gallery UI");
        ok &= IsActive(readButtonUI, "Read Button UI");

        return ok;
    }

    private bool Assigned(UnityEngine.Object obj, string label)
    {
        if (obj != null) return true;
        Debug.LogError("LetterGallery: '" + label + "' isn't assigned in the Inspector.", this);
        return false;
    }

    private bool IsActive(Component c, string label)
    {
        if (c == null || c.gameObject.activeInHierarchy) return true;
        Debug.LogError("LetterGallery: '" + label + "' is switched off in the Hierarchy. Switch it ON and tick " +
                       "'Start Hidden' on its UIPanelTransition instead. The script hides it with transparency.", this);
        return false;
    }
}