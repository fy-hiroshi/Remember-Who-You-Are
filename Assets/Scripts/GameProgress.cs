using UnityEngine;

/// Saved progress that survives closing the game (stored on the device with PlayerPrefs).
public static class GameProgress
{
    private const string GalleryKey = "GalleryUnlocked";

    /// Becomes true the first time the player finishes the game, and stays true.
    public static bool GalleryUnlocked
    {
        get => PlayerPrefs.GetInt(GalleryKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(GalleryKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    /// For testing: forgets everything.
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(GalleryKey);
        PlayerPrefs.Save();
    }
}
