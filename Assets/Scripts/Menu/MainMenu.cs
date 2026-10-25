using UnityEngine;
using UnityEngine.UI;

/// Put this on an object in the MainMenu scene (e.g. the Canvas).
/// Play starts the zoom + loading screen, Letter Gallery opens once the game has been finished,
/// and Exit closes the app.
public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button galleryButton;
    [SerializeField] private Button exitButton;

    [Header("Play")]
    [SerializeField] private PlayTransition playTransition;

    [Header("Letter gallery")]
    [SerializeField] private LetterGallery gallery;
    [Tooltip("Optional: a padlock image that shows on the gallery button while it's locked.")]
    [SerializeField] private GameObject galleryLockIcon;
    [Tooltip("Optional: a hint shown when the player taps the gallery while it's locked (e.g. 'Finish the game to unlock').")]
    [SerializeField] private MessagePopup galleryLockedMessage;

    private void Start()
    {
        // The game scene may have left these changed
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshGalleryLock();
    }

    private void OnEnable()
    {
        playButton.onClick.AddListener(Play);
        galleryButton.onClick.AddListener(OpenGallery);
        exitButton.onClick.AddListener(Quit);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(Play);
        galleryButton.onClick.RemoveListener(OpenGallery);
        exitButton.onClick.RemoveListener(Quit);
    }

    private void Play()
    {
        playTransition.Begin();
    }

    private void OpenGallery()
    {
        // The button stays tappable while locked, so the player gets a hint instead of silence
        if (!GameProgress.GalleryUnlocked)
        {
            if (galleryLockedMessage != null) galleryLockedMessage.Show();
            return;
        }

        if (gallery == null)
        {
            Debug.LogError("MainMenu: the 'Gallery' slot isn't assigned. Drag the object with the LetterGallery script into it.", this);
            return;
        }

        gallery.Open();
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Application.Quit does nothing in the editor
#else
        Application.Quit();
#endif
    }

    private void RefreshGalleryLock()
    {
        if (galleryLockIcon != null) galleryLockIcon.SetActive(!GameProgress.GalleryUnlocked);
    }

    // Right-click the component's title in the Inspector to use these while testing
    [ContextMenu("Testing: unlock gallery")]
    private void TestUnlockGallery()
    {
        GameProgress.GalleryUnlocked = true;
        RefreshGalleryLock();
    }

    [ContextMenu("Testing: reset progress")]
    private void TestResetProgress()
    {
        GameProgress.ResetAll();
        RefreshGalleryLock();
    }
}