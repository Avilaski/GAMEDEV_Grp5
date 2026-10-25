using UnityEngine;

// Visual scaling only. The BombController still decides when damage happens.
public class BombPopAnimation : MonoBehaviour
{
    private Vector3 fullScale;
    private Vector3 startScale;
    private float elapsed;
    private bool despawning;
    private const float PlacementSeconds = 0.12f;
    private const float DespawnSeconds = 0.08f;

    private void Awake()
    {
        fullScale = transform.localScale;
        transform.localScale = fullScale * 0.05f;
    }

    public void Despawn()
    {
        despawning = true;
        elapsed = 0f;
        startScale = transform.localScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (despawning)
        {
            float t = Mathf.Clamp01(elapsed / DespawnSeconds);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            if (t >= 1f)
                Destroy(gameObject);
        }
        else
        {
            float t = Mathf.Clamp01(elapsed / PlacementSeconds);
            // A quick overshoot gives placement a little weight before settling.
            float pop = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) +
                1.70158f * Mathf.Pow(t - 1f, 2f);
            transform.localScale = fullScale * Mathf.LerpUnclamped(0.05f, 1f, pop);
        }
    }
}
