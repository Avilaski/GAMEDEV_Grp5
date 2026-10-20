using System;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private LaneController[] lanes;
    [SerializeField] private RhythmMap rhythmMap;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool playOnStart = true;
    [SerializeField, Min(0.1f)] private float startDelay = 1f;

    private LaneSpawnEntry[] scheduledEntries;
    private int nextEntry;
    private double songStartDspTime;
    private bool isRunning;

    // Seconds relative to the scheduled beginning of the audio clip
    public double SongTime => isRunning ? AudioSettings.dspTime - songStartDspTime: 0.0;

    private void Start()
    {
        if (playOnStart)
            PlayMap();
    }

    public void PlayMap()
    {
        StopMap();

        if (!ValidateMap())
            return;

        scheduledEntries = (LaneSpawnEntry[])rhythmMap.entries.Clone();
        Array.Sort(scheduledEntries, (a, b) => a.spawnTime.CompareTo(b.spawnTime));
        nextEntry = 0;

        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.pitch = 1f;
        musicSource.clip = rhythmMap.song;
        songStartDspTime = AudioSettings.dspTime + Math.Max(0.1, startDelay);
        musicSource.PlayScheduled(songStartDspTime);
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        double songTime = SongTime;

        // Process ALL due entries, including simultaneous notes or a late frame.
        while (nextEntry < scheduledEntries.Length &&
               scheduledEntries[nextEntry].spawnTime <= songTime)
        {
            LaneSpawnEntry entry = scheduledEntries[nextEntry];
            lanes[entry.laneIndex].Spawn(entry.prefab, entry.spawnTime, entry.arrivalTime);
            nextEntry++;
        }

        foreach (LaneController lane in lanes)
            lane.UpdateObjects(songTime);
    }

    public void StopMap()
    {
        isRunning = false;

        if (musicSource != null)
            musicSource.Stop();

        if (lanes == null)
            return;

        foreach (LaneController lane in lanes)
        {
            if (lane != null)
                lane.ClearObjects();
        }
    }

    private bool ValidateMap()
    {
        if (rhythmMap == null || rhythmMap.song == null ||
            rhythmMap.entries == null || musicSource == null ||
            lanes == null || lanes.Length == 0)
        {
            Debug.LogError("Assign a rhythm map with a song, an AudioSource, and lanes.", this);
            return false;
        }

        foreach (LaneController lane in lanes)
        {
            if (lane == null || !lane.IsConfigured)
            {
                Debug.LogError("Every lane needs a controller and both endpoints.", this);
                return false;
            }
        }

        foreach (LaneSpawnEntry entry in rhythmMap.entries)
        {
            if (entry == null || entry.prefab == null ||
                entry.laneIndex < 0 || entry.laneIndex >= lanes.Length ||
                float.IsNaN(entry.spawnTime) || float.IsInfinity(entry.spawnTime) ||
                float.IsNaN(entry.arrivalTime) || float.IsInfinity(entry.arrivalTime) ||
                entry.spawnTime < 0f || entry.arrivalTime <= entry.spawnTime)
            {
                Debug.LogError("Map entries need a valid lane, prefab, and finite times with arrival after spawn.", this);
                return false;
            }
        }

        return true;
    }

    private void OnDisable()
    {
        StopMap();
    }
}
