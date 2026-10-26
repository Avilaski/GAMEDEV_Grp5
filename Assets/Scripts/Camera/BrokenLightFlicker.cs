using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class BrokenLightFlicker : MonoBehaviour
{
    [Header("Time Between Flicker Bursts (Seconds)")]
    [SerializeField]
    private Vector2 steadyDuration = new Vector2(1.5f, 5f);

    [Header("Flicker Bursts")]
    [Tooltip("Number of dim/bright pulses per burst (inclusive).")]
    [SerializeField]
    private Vector2Int pulsesPerBurst = new Vector2Int(2, 6);
    [SerializeField]
    private Vector2 pulseDuration = new Vector2(0.03f, 0.15f);
    [Tooltip("Brightness during a failing pulse, relative to the original intensity.")]
    [SerializeField]
    private Vector2 dimMultiplier = new Vector2(0.05f, 0.45f);
    [Tooltip("Chance that a failing pulse goes completely dark.")]
    [SerializeField, Range(0f, 1f)]
    private float blackoutChance = 0.25f;

    [Header("Subtle Power Fluctuation")]
    [SerializeField, Range(0f, 1f)]
    private float brightnessWaver = 0.08f;
    [SerializeField, Min(0f)]
    private float waverSpeed = 12f;

    private Light targetLight;
    private float originalIntensity;
    private float remainingTime;
    private float noiseSeed;
    private float noiseTime;
    private float pulseMultiplier = 1f;
    private int pulsesRemaining;
    private bool isDim;

    private void OnEnable()
    {
        targetLight = GetComponent<Light>();
        originalIntensity = targetLight.intensity;
        noiseSeed = Random.Range(0f, 10000f);
        noiseTime = 0f;
        pulsesRemaining = 0;
        isDim = false;
        pulseMultiplier = 1f;
        remainingTime = RandomDuration(steadyDuration);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        remainingTime -= deltaTime;
        if (remainingTime <= 0f)
        {
            AdvancePulse();
        }

        noiseTime += deltaTime * waverSpeed;
        float waver = Mathf.Lerp(1f - brightnessWaver, 1f, Mathf.PerlinNoise(noiseSeed, noiseTime));
        targetLight.intensity = originalIntensity * pulseMultiplier * waver;
    }

    private void AdvancePulse()
    {
        if (isDim)
        {
            isDim = false;
            pulseMultiplier = 1f;
            remainingTime = RandomDuration(pulsesRemaining > 0 ? pulseDuration : steadyDuration);
            return;
        }

        if (pulsesRemaining == 0)
        {
            pulsesRemaining = Random.Range(pulsesPerBurst.x, pulsesPerBurst.y + 1);
        }

        pulsesRemaining--;
        isDim = true;
        pulseMultiplier = Random.value < blackoutChance ? 0f : Random.Range(dimMultiplier.x, dimMultiplier.y);
        remainingTime = RandomDuration(pulseDuration);
    }

    private void OnDisable()
    {
        if (targetLight != null)
        {
            targetLight.intensity = originalIntensity;
        }
    }

    private static float RandomDuration(Vector2 range)
    {
        return Mathf.Max(0.01f, Random.Range(range.x, range.y));
    }

    private void OnValidate()
    {
        steadyDuration.x = Mathf.Max(0.01f, steadyDuration.x);
        steadyDuration.y = Mathf.Max(steadyDuration.x, steadyDuration.y);
        pulseDuration.x = Mathf.Max(0.01f, pulseDuration.x);
        pulseDuration.y = Mathf.Max(pulseDuration.x, pulseDuration.y);
        pulsesPerBurst.x = Mathf.Clamp(pulsesPerBurst.x, 1, 100);
        pulsesPerBurst.y = Mathf.Clamp(pulsesPerBurst.y, pulsesPerBurst.x, 100);
        dimMultiplier.x = Mathf.Clamp01(dimMultiplier.x);
        dimMultiplier.y = Mathf.Clamp(dimMultiplier.y, dimMultiplier.x, 1f);
    }
}
