using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Put this on an object that is always active (e.g. the Canvas).
/// Begin() fades the screen to white, shows the final text, then loads the main menu.
public class EndingSequence : MonoBehaviour
{
    [Header("UI (keep both ACTIVE in the Hierarchy; the script hides them with transparency)")]
    [Tooltip("A full-screen white Image with a CanvasGroup, placed LAST in the Canvas so it covers everything.")]
    [SerializeField] private CanvasGroup whiteScreen;
    [Tooltip("The \"Don't Forget.\" text with a CanvasGroup, placed AFTER the white screen so it draws on top.")]
    [SerializeField] private CanvasGroup endText;

    [Header("Optional")]
    [Tooltip("The \"go home\" hint. It fades away when the ending starts.")]
    [SerializeField] private MessagePopup goHomeMessage;

    [Header("Timing")]
    [SerializeField] private float fadeToWhiteSeconds = 1.5f;
    [SerializeField] private float pauseBeforeText = 0.5f;
    [SerializeField] private float textFadeSeconds = 1f;
    [SerializeField] private float textHoldSeconds = 3f;

    [Header("Afterwards")]
    [Tooltip("Exact name of your main menu scene. It must be added in Build Settings.")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    /// True once the ending has started. Controllers use this to freeze the player.
    public static bool IsPlaying { get; private set; }

    private void Awake()
    {
        IsPlaying = false; // fresh start every time the scene loads
        Prepare(whiteScreen);
        Prepare(endText);
    }

    private void OnDestroy()
    {
        IsPlaying = false; // so replaying from the main menu isn't frozen
    }

    public void Begin()
    {
        if (IsPlaying) return;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        IsPlaying = true;

        // Finishing the game unlocks the Letter Gallery. Saved right away, so quitting
        // during the white screen can't lose it.
        GameProgress.GalleryUnlocked = true;

        if (goHomeMessage != null) goHomeMessage.Hide();

        whiteScreen.blocksRaycasts = true; // nothing underneath can be tapped any more

        yield return Fade(whiteScreen, fadeToWhiteSeconds);
        yield return new WaitForSeconds(pauseBeforeText);
        yield return Fade(endText, textFadeSeconds);
        yield return new WaitForSeconds(textHoldSeconds);

        // Back to the main menu (free the mouse for PC testing)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            Debug.LogError("EndingSequence: scene '" + mainMenuScene + "' wasn't found. " +
                           "Check the name, and add it in File > Build Profiles (Scenes in Build).");
            yield break;
        }

        SceneManager.LoadScene(mainMenuScene);
    }

    private static void Prepare(CanvasGroup group)
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private static IEnumerator Fade(CanvasGroup group, float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            group.alpha = Mathf.Clamp01(t / seconds);
            yield return null;
        }
        group.alpha = 1f;
    }
}