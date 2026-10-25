using System;
using UnityEngine;

[Serializable]
public class LaneSpawnEntry
{
    [Min(0)] public int laneIndex;
    [Tooltip("The GameObject prefab to spawn: a monster, crate, or another lane object.")]
    public GameObject prefab;
    [Min(0f)] public float spawnTime;
    [Min(0f)] public float arrivalTime;
    [Tooltip("Song time to place a bomb. Negative uses the original endpoint-arrival timing.")]
    public float bombPlacementTime = -1f;
}

[CreateAssetMenu(fileName = "RhythmMap", menuName = "Bomber Rhythm/Rhythm Map")]
public class RhythmMap : ScriptableObject
{
    public AudioClip song;
    [Header("Beat clock (set these to match the song)")]
    [Min(1f)] public float beatsPerMinute = 120f;
    [Min(0f)] public float firstBeatTime;
    [Tooltip("Time from detonation until FlameVFX reaches full range (frame 5 at 60 FPS).")]
    [Min(0f)] public float blastExpansionSeconds = 5f / 60f;
    public LaneSpawnEntry[] entries = new LaneSpawnEntry[0];
}
