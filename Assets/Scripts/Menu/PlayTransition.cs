using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Put this on the Canvas. Begin() zooms the view in while fading to black, then the black screen
/// becomes the loading screen (animated dots) while the game scene loads in the background.
public class PlayTransition : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Exact name of the game scene. It must be added in Build Settings.")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("Zoom")]
    [Tooltip("The main menu's camera.")]
    [SerializeField] private Camera cam;
    [Tooltip("The lens narrows to this value, which feels like zooming in. The menu starts around 60.")]
    [SerializeField] private float zoomedFieldOfView = 30f;
    [SerializeField] private float zoomSeconds = 1.3f;
    [Tooltip("Optional: the menu buttons ease out while the view zooms.")]
    [SerializeField] private UIPanelTransition menuUI;

    [Header("Black screen + loading")]
    [Tooltip("A full-screen black Image with a CanvasGroup, placed LAST in the Canvas so it covers everything.")]
    [SerializeField] private CanvasGroup blackScreen;
    [Tooltip("The three dots (a child of the black screen). Shown once the screen is fully black.")]
    [SerializeField] private GameObject loadingDots;
    [Tooltip("The loading screen stays at least this long, so the dots are seen even if loading is instant.")]
    [SerializeField] private float minLoadingSeconds = 2f;

    private bool started;

    private void Awake()
    {
        blackScreen.alpha = 0f;
        blackScreen.blocksRaycasts = false;
        blackScreen.interactable = false;
        if (loadingDots != null) loadingDots.SetActive(false);
    }

    public void Begin()
    {
        if (started) return; // ignore double taps

        if (!Application.CanStreamedLevelBeLoaded(gameScene))
        {
            Debug.LogError("PlayTransition: scene '" + gameScene + "' wasn't found. " +
                           "Check the name, and add it in File > Build Profiles (Scenes in Build).");
            return;
        }

        started = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (menuUI != null) menuUI.Hide();
        blackScreen.blocksRaycasts = true; // nothing underneath can be tapped any more

        // 1. Zoom in while fading to black
        float startFov = cam.fieldOfView;
        for (float t = 0f; t < zoomSeconds; t += Time.deltaTime)
        {
            float k = t / zoomSeconds;
            cam.fieldOfView = Mathf.Lerp(startFov, zoomedFieldOfView, Mathf.SmoothStep(0f, 1f, k));
            blackScreen.alpha = k * k; // starts slowly, ends quickly
            yield return null;
        }
        blackScreen.alpha = 1f;

        // 2. Loading screen
        if (loadingDots != null) loadingDots.SetActive(true);

        AsyncOperation load = SceneManager.LoadSceneAsync(gameScene);
        load.allowSceneActivation = false; // wait until we say go

        float elapsed = 0f;
        while (load.progress < 0.9f || elapsed < minLoadingSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Go
        load.allowSceneActivation = true;
    }
}
