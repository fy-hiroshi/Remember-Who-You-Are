using UnityEngine;
using UnityEngine.UI;

/// Put this on a parent object holding three dot Images. The dots hop one after another, forever.
public class LoadingDots : MonoBehaviour
{
    [Tooltip("The dots, left to right.")]
    [SerializeField] private RectTransform[] dots;

    [SerializeField] private float speed = 7f;
    [Tooltip("Delay between one dot and the next.")]
    [SerializeField] private float stagger = 0.8f;
    [SerializeField] private float hopHeight = 28f;
    [Tooltip("How much a dot swells while it hops.")]
    [SerializeField] private float growAmount = 0.3f;
    [Tooltip("How faded a dot is while resting.")]
    [SerializeField, Range(0f, 1f)] private float restingAlpha = 0.35f;

    private Vector2[] restPositions;
    private Graphic[] graphics;

    private void Awake()
    {
        restPositions = new Vector2[dots.Length];
        graphics = new Graphic[dots.Length];

        for (int i = 0; i < dots.Length; i++)
        {
            restPositions[i] = dots[i].anchoredPosition;
            graphics[i] = dots[i].GetComponent<Graphic>();
        }
    }

    private void Update()
    {
        float t = Time.unscaledTime * speed;

        for (int i = 0; i < dots.Length; i++)
        {
            // Only the upward half of the wave, so each dot hops, then rests while the next one hops
            float wave = Mathf.Max(0f, Mathf.Sin(t - i * stagger));

            dots[i].anchoredPosition = restPositions[i] + Vector2.up * (wave * hopHeight);
            dots[i].localScale = Vector3.one * (1f + wave * growAmount);

            if (graphics[i] != null)
            {
                Color c = graphics[i].color;
                c.a = Mathf.Lerp(restingAlpha, 1f, wave);
                graphics[i].color = c;
            }
        }
    }
}
