using System;
using UnityEngine;

[Serializable]
public class LaneSpawnEntry
{
    [Min(0)] public int laneIndex;
    public LaneMover prefab;
    [Min(0f)] public float spawnTime;
    [Min(0f)] public float arrivalTime;
}

[CreateAssetMenu(fileName = "RhythmMap", menuName = "Bomber Rhythm/Rhythm Map")]
public class RhythmMap : ScriptableObject
{
    public AudioClip song;
    public LaneSpawnEntry[] entries = new LaneSpawnEntry[0];
}
