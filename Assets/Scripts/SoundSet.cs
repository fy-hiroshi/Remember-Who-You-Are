using UnityEngine;

/// A reusable "sound slot" for the Inspector: drag in one or more clips, set the volume, done.
/// If you add several clips, one is picked at random each time (never the same one twice in a row).
/// The sound is played from a temporary object, so it still finishes even if the object that
/// triggered it is switched off right afterwards (like a letter being picked up).
[System.Serializable]
public class SoundSet
{
    [Tooltip("One or more clips. With several, a random one plays each time.")]
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("A little random pitch so repeats don't sound identical. Set both numbers to 1 for none.")]
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [Tooltip("1 = comes from the object and gets quieter with distance. 0 = plain sound, same volume everywhere. (Ignored for button sounds, which are always plain.)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    [System.NonSerialized] private int lastIndex = -1;

    /// Plays from a spot in the world, using this slot's Spatial Blend.
    public void PlayAt(Vector3 position)
    {
        Play(position, spatialBlend);
    }

    /// Plays as a plain 2D sound with the same volume everywhere (used for UI clicks).
    public void Play2D()
    {
        Play(Vector3.zero, 0f);
    }

    private void Play(Vector3 position, float blend)
    {
        if (clips == null || clips.Length == 0) return;

        int index = 0;
        if (clips.Length > 1)
        {
            do { index = Random.Range(0, clips.Length); }
            while (index == lastIndex);
        }
        lastIndex = index;

        AudioClip clip = clips[index];
        if (clip == null) return;

        float pitch = Random.Range(pitchRange.x, pitchRange.y);

        GameObject go = new GameObject("Sound (" + clip.name + ")");
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = blend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1.5f;
        source.maxDistance = 25f;
        source.Play();

        Object.Destroy(go, clip.length / Mathf.Max(0.1f, Mathf.Abs(pitch)) + 0.1f);
    }
}