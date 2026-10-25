using System;
using UnityEngine;
using UnityEngine.InputSystem;

// One fixed bomb tile per controller. Test movement with a fixed fuse first.
// BombBlast and lighting can subscribe to Exploded.
public class BombController : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0.01f)] private float placementRadius = 0.75f;
    [Header("Fuse")]
    [SerializeField] private bool useBeatTiming;
    [SerializeField, Min(0.1f)] private float fuseSeconds = 1f;
    [SerializeField] private SpawnController songClock;
    [SerializeField] private Transform bombPoint;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Key placementKey = Key.Space;
    [SerializeField, Min(0f)] private float beatWindowSeconds = 0.12f;

    public event Action<Vector3> Placed;
    public event Action<Vector3> Exploded;

    private GameObject activeBomb;
    private double explosionTime;
    private double lastPlacementBeat = double.NegativeInfinity;
    private bool activeBombUsesBeatTiming;

    private void OnEnable()
    {
        if (songClock != null)
            songClock.MapStopped += ClearBomb;
    }

    private void LateUpdate()
    {
        double now = activeBombUsesBeatTiming && songClock != null
            ? songClock.SongTime : Time.timeAsDouble;
        if (activeBomb != null && now >= explosionTime)
        {
            Vector3 position = activeBomb.transform.position;
            Destroy(activeBomb);
            activeBomb = null;
            Debug.Log("Bomb detonated.", this);
            Exploded?.Invoke(position);
        }

        if (Keyboard.current != null && Keyboard.current[placementKey].wasPressedThisFrame)
            TryPlaceBomb();
    }

    public bool TryPlaceBomb()
    {
        if (activeBomb != null)
            return false;
        if (player == null || bombPoint == null || bombPrefab == null)
        {
            Debug.LogWarning("Bomb placement needs Player, Bomb Point, and Bomb Prefab assigned.", this);
            return false;
        }

        Vector3 offset = player.position - bombPoint.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > placementRadius * placementRadius)
        {
            Debug.Log($"Move onto this lane's bomb tile: distance {offset.magnitude:F2}, " +
                $"allowed {placementRadius:F2} world units.", this);
            return false;
        }

        if (!useBeatTiming)
        {
            activeBombUsesBeatTiming = false;
            explosionTime = Time.timeAsDouble + Math.Max(0.1, fuseSeconds);
            SpawnBomb();
            return true;
        }

        if (songClock == null || !songClock.IsRunning || songClock.Map == null)
            return false;

        RhythmMap map = songClock.Map;
        if (float.IsNaN(map.beatsPerMinute) || float.IsInfinity(map.beatsPerMinute) ||
            map.beatsPerMinute < 1f || float.IsNaN(map.firstBeatTime) ||
            float.IsInfinity(map.firstBeatTime))
            return false;

        double now = songClock.SongTime;
        if (now < 0)
            return false;

        double beatDuration = 60.0 / map.beatsPerMinute;
        double beatIndex = Math.Round((now - map.firstBeatTime) / beatDuration);
        double placementTime = map.firstBeatTime + beatIndex * beatDuration;
        double window = Math.Min(beatWindowSeconds, beatDuration * 0.25);
        if (beatIndex < 0 || beatIndex <= lastPlacementBeat || Math.Abs(now - placementTime) > window)
        {
            Debug.Log("Bomb placement missed the beat window.", this);
            return false;
        }

        lastPlacementBeat = beatIndex;
        activeBombUsesBeatTiming = true;
        explosionTime = placementTime + beatDuration;
        SpawnBomb();
        return true;
    }

    private void SpawnBomb()
    {
        activeBomb = Instantiate(bombPrefab, bombPoint.position, bombPoint.rotation);
        // The player must be able to step out of the newly occupied tile.
        // Bomb blocking can be added separately from the blast overlap query.
        foreach (Collider bombCollider in activeBomb.GetComponentsInChildren<Collider>())
            bombCollider.enabled = false;
        Debug.Log("Bomb placed. Move away!", this);
        Placed?.Invoke(bombPoint.position);
    }

    private void ClearBomb()
    {
        if (activeBomb != null)
        {
            activeBomb.SetActive(false);
            Destroy(activeBomb);
        }
        activeBomb = null;
        lastPlacementBeat = double.NegativeInfinity;
    }

    private void OnDisable()
    {
        if (songClock != null)
            songClock.MapStopped -= ClearBomb;
        ClearBomb();
    }
}
