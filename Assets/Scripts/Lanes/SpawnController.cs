using System;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField]
    private LaneController[] lanes;
    [SerializeField, Tooltip("Default player for lanes without their own Player assignment.")]
    private Transform player;
    [SerializeField]
    private RhythmMap rhythmMap;
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField]
    private bool playOnStart = true;
    [SerializeField, Min(0.1f)]
    private float startDelay = 1f;
    private LaneSpawnEntry[] scheduledEntries;
    private int nextEntry;
    private double songStartDspTime;
    private bool isRunning;
    public double SongTime => isRunning ? AudioSettings.dspTime - songStartDspTime : 0.0;
    public bool IsRunning => isRunning;
    public RhythmMap Map => rhythmMap;

    public event Action MapStopped;
    public event Action SongFinished;

    private void Start()
    {
        if (playOnStart)
        {
            PlayMap();
        }
    }

    public void PlayMap()
    {
        StopMap();
        if (lanes != null && player != null)
        {
            foreach (LaneController lane in lanes)
            {
                if (lane == null)
                {
                    continue;
                }

                BombController bomb = lane.GetComponent<BombController>();
                if (bomb != null)
                {
                    bomb.BindPlayer(player);
                }
            }
        }

        if (!ValidateMap())
        {
            return;
        }

        scheduledEntries = new LaneSpawnEntry[rhythmMap.entries.Length];
        for (int i = 0; i < scheduledEntries.Length; i++)
        {
            LaneSpawnEntry entry = rhythmMap.entries[i];
            float arrival = entry.arrivalTime;
            if (entry.bombPlacementTime >= 0f && !lanes[entry.laneIndex].TryGetBombArrival(entry, rhythmMap.blastExpansionSeconds, out arrival))
            {
                Debug.LogError($"Lane {entry.laneIndex}: bomb range must intersect the object's path before its endpoint.", this);
                return;
            }

            scheduledEntries[i] = new LaneSpawnEntry
            {
                laneIndex = entry.laneIndex,
                prefab = entry.prefab,
                spawnTime = entry.spawnTime,
                arrivalTime = arrival,
                bombPlacementTime = entry.bombPlacementTime
            };
        }

        Array.Sort(scheduledEntries, (a, b) => a.spawnTime.CompareTo(b.spawnTime));
        nextEntry = 0;
        foreach (LaneController lane in lanes)
        {
            BombController bomb = lane.GetComponent<BombController>();
            if (bomb != null)
            {
                bomb.BindSongClock(this);
            }
        }

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
        {
            return;
        }

        double songTime = SongTime;

        if (songTime >= rhythmMap.song.length)
        {
            FinishSong();
            return;
        }

        while (nextEntry < scheduledEntries.Length && scheduledEntries[nextEntry].spawnTime <= songTime)
        {
            LaneSpawnEntry entry = scheduledEntries[nextEntry];
            lanes[entry.laneIndex].Spawn(entry.prefab, entry.spawnTime, entry.arrivalTime);
            nextEntry++;
        }

        foreach (LaneController lane in lanes)
        {
            lane.UpdateObjects(songTime);
        }
    }

    public void StopMap()
    {
        isRunning = false;
        MapStopped?.Invoke();
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (lanes == null)
        {
            return;
        }

        foreach (LaneController lane in lanes)
        {
            if (lane != null)
            {
                lane.ClearObjects();
            }
        }
    }

    private bool ValidateMap()
    {
        if (rhythmMap == null || rhythmMap.song == null || rhythmMap.entries == null || musicSource == null || lanes == null || lanes.Length == 0)
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
            if (entry == null || entry.prefab == null || entry.laneIndex < 0 || entry.laneIndex >= lanes.Length || float.IsNaN(entry.spawnTime) || float.IsInfinity(entry.spawnTime) || float.IsNaN(entry.arrivalTime) || float.IsInfinity(entry.arrivalTime) || entry.spawnTime < 0f || entry.arrivalTime <= entry.spawnTime)
            {
                Debug.LogError("Map entries need a valid lane, prefab, and finite times with arrival after spawn.", this);
                return false;
            }

            if (float.IsNaN(entry.bombPlacementTime) || float.IsInfinity(entry.bombPlacementTime) || (entry.bombPlacementTime >= 0f && entry.bombPlacementTime < entry.spawnTime) || float.IsNaN(rhythmMap.blastExpansionSeconds) || float.IsInfinity(rhythmMap.blastExpansionSeconds) || rhythmMap.blastExpansionSeconds < 0f)
            {
                Debug.LogError("Bomb placement cues must be finite and at or after spawn; expansion time must be finite and nonnegative.", this);
                return false;
            }
        }

        return true;
    }

    private void FinishSong()
    {
        if (!isRunning) return;

        isRunning = false;

        if (musicSource != null)
            musicSource.Stop();

        SongFinished?.Invoke();
    }

    private void OnDisable()
    {
        StopMap();
    }
}
