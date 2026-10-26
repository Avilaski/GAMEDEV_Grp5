using System.Collections.Generic;
using UnityEngine;

public class BombCueMarker : MonoBehaviour
{
    [SerializeField]
    private SpawnController songClock;
    [SerializeField]
    private int laneIndex;
    [SerializeField, Min(0.1f)]
    private float anticipationSeconds = 0.5f;
    [SerializeField, Min(0f)]
    private float cueHoldSeconds = 0.1f;
    private readonly List<double> cues = new List<double>();
    private readonly List<Material> materials = new List<Material>();
    private readonly List<Material> sourceMaterials = new List<Material>();
    private Renderer[] renderers;
    private bool[] rendererStates;
    private RhythmMap cachedMap;
    private int nextCue;
    private double lastSongTime = double.NegativeInfinity;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rendererStates = new bool[renderers.Length];
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer renderer = renderers[rendererIndex];
            rendererStates[rendererIndex] = renderer.enabled;
            Material[] copies = renderer.sharedMaterials;
            for (int i = 0; i < copies.Length; i++)
            {
                if (copies[i] == null || !copies[i].HasProperty(EmissionColor))
                {
                    continue;
                }

                sourceMaterials.Add(copies[i]);
                Material material = new Material(copies[i]);
                material.EnableKeyword("_EMISSION");
                copies[i] = material;
                materials.Add(material);
            }

            renderer.sharedMaterials = copies;
        }

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        SetGlow(0f);
    }

    private void Update()
    {
        if (songClock == null || !songClock.IsRunning || songClock.Map == null)
        {
            nextCue = 0;
            lastSongTime = double.NegativeInfinity;
            SetGlow(0f);
            return;
        }

        if (cachedMap != songClock.Map)
        {
            ReadCues();
        }

        double now = songClock.SongTime;
        if (now < lastSongTime)
        {
            nextCue = 0;
        }

        lastSongTime = now;
        while (nextCue < cues.Count && now > cues[nextCue] + cueHoldSeconds)
        {
            nextCue++;
        }

        float glow = 0f;
        if (nextCue < cues.Count)
        {
            double remaining = cues[nextCue] - now;
            if (remaining <= anticipationSeconds)
            {
                float progress = Mathf.Clamp01(1f - (float)remaining / anticipationSeconds);
                glow = progress * progress * progress * progress;
            }
        }

        SetGlow(glow);
    }

    private void ReadCues()
    {
        cachedMap = songClock.Map;
        cues.Clear();
        nextCue = 0;
        if (cachedMap.entries == null)
        {
            return;
        }

        foreach (LaneSpawnEntry entry in cachedMap.entries)
        {
            if (entry != null && entry.laneIndex == laneIndex && entry.bombPlacementTime >= 0f)
            {
                cues.Add(entry.bombPlacementTime);
            }
        }

        cues.Sort();
    }

    private void SetGlow(float glow)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = rendererStates[i] && glow > 0f;
        }

        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetColor(EmissionColor, sourceMaterials[i].GetColor(EmissionColor) * glow);
        }
    }

    private void OnDisable()
    {
        if (renderers != null)
        {
            SetGlow(0f);
        }
    }

    private void OnDestroy()
    {
        foreach (Material material in materials)
        {
            Destroy(material);
        }
    }
}
