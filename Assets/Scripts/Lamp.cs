using UnityEngine;

/// Put this on a lamp or a wall switch (it needs a Collider).
/// Interact once to turn ALL assigned lights on, again to turn them all off.
public class Lamp : MonoBehaviour, IInteractable
{
    [Tooltip("Every Light this lamp/switch controls. They can be anywhere in the scene.")]
    [SerializeField] private Light[] lights;
    [SerializeField] private bool startsOn = false;

    [Header("Sounds (optional, leave empty for silence)")]
    [Tooltip("Plays when the lights are switched on, e.g. a click.")]
    [SerializeField] private SoundSet turnOnSound;
    [Tooltip("Plays when the lights are switched off.")]
    [SerializeField] private SoundSet turnOffSound;

    private bool isOn;

    private void Start()
    {
        // Setting the starting state is silent
        SetLights(startsOn, false);
    }

    public void Interact(PlayerInteractor interactor)
    {
        SetLights(!isOn, true);
    }

    private void SetLights(bool on, bool playSound)
    {
        isOn = on;

        foreach (Light l in lights)
        {
            if (l != null) l.enabled = on;
        }

        if (!playSound) return;

        SoundSet sound = on ? turnOnSound : turnOffSound;
        if (sound != null) sound.PlayAt(transform.position);
    }
}