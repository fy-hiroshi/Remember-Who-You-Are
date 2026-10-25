using UnityEngine;

/// Put this on any object to give it a sound you can trigger from a UnityEvent.
/// Example: on KeyLock's "On Locked No Key" event, click +, drag in this object, and pick SoundEffect > Play ().
public class SoundEffect : MonoBehaviour
{
    [SerializeField] private SoundSet sound = new SoundSet();

    public void Play()
    {
        sound.PlayAt(transform.position);
    }
}
