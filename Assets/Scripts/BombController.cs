using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombController : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField]
    private Transform player;
    [SerializeField, Min(0.01f)]
    private float placementRadius = 0.75f;

    [Header("Fuse")]
    [SerializeField]
    private bool useBeatTiming;
    [SerializeField, Min(0.1f)]
    private float fuseSeconds = 1f;
    [SerializeField]
    private SpawnController songClock;
    [SerializeField]
    private Transform bombPoint;
    [SerializeField]
    private GameObject bombPrefab;

    [Header("Visual effects")]
    [SerializeField]
    private GameObject explosionVfxPrefab;
    [SerializeField]
    private Key placementKey = Key.Space;
    [SerializeField, Min(0f)]
    private float beatWindowSeconds = 0.12f;
    public event Action<Vector3> Placed;
    public event Action<Vector3> Exploded;
    public event Action<GameObject> ExplosionSpawned;
    private GameObject activeBomb;
    private double explosionTime;
    private double lastPlacementBeat = double.NegativeInfinity;
    private bool activeBombUsesBeatTiming;
    public float FuseSeconds => Mathf.Max(0.1f, fuseSeconds);
    public PlayerHealth PlayerHealth => player != null ? player.GetComponentInParent<PlayerHealth>() : null;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bombPlaceSound;
    [SerializeField] private AudioClip bombExplosionSound;

    public void BindPlayer(Transform scenePlayer)
    {
        if (player == null)
        {
            player = scenePlayer;
        }
    }

    public void BindSongClock(SpawnController clock)
    {
        if (songClock != null)
        {
            songClock.MapStopped -= ClearBomb;
        }

        songClock = clock;
        if (isActiveAndEnabled && songClock != null)
        {
            songClock.MapStopped += ClearBomb;
        }
    }

    public bool TryGetBlastEntry(Vector3 start, Vector3 end, out float progress)
    {
        progress = 0f;
        BoxCollider box = explosionVfxPrefab != null ? explosionVfxPrefab.GetComponent<BoxCollider>() : null;
        if (box == null || bombPoint == null)
        {
            return false;
        }

        Quaternion inverse = Quaternion.Inverse(bombPoint.rotation);
        Vector3 origin = inverse * (start - bombPoint.position);
        Vector3 delta = inverse * (end - start);
        Vector3 size = Vector3.Scale(box.size, explosionVfxPrefab.transform.localScale);
        Vector3 center = Vector3.Scale(box.center, explosionVfxPrefab.transform.localScale);
        origin -= center;
        float enter = 0f, exit = 1f;
        for (int axis = 0; axis < 3; axis += 2)
        {
            float half = Mathf.Abs(size[axis]) * 0.5f;
            if (Mathf.Abs(delta[axis]) < 0.00001f)
            {
                if (Mathf.Abs(origin[axis]) > half)
                {
                    return false;
                }

                continue;
            }

            float a = (-half - origin[axis]) / delta[axis];
            float b = (half - origin[axis]) / delta[axis];
            enter = Mathf.Max(enter, Mathf.Min(a, b));
            exit = Mathf.Min(exit, Mathf.Max(a, b));
        }

        if (enter >= exit || enter <= 0f || enter >= 1f)
        {
            return false;
        }

        progress = Mathf.Min(enter + 0.25f / Vector3.Distance(start, end), (enter + exit) * 0.5f);
        return true;
    }

    private void OnEnable()
    {
        if (songClock != null)
        {
            songClock.MapStopped += ClearBomb;
        }
    }

    private void LateUpdate()
    {
        double now = activeBombUsesBeatTiming && songClock != null ? songClock.SongTime : Time.timeAsDouble;
        if (activeBomb != null && now >= explosionTime)
        {
            Vector3 position = activeBomb.transform.position;
            BombPopAnimation pop = activeBomb.GetComponent<BombPopAnimation>();
            if (pop != null)
            {
                pop.Despawn();
            }
            else
            {
                Destroy(activeBomb);
            }

            activeBomb = null;
            if (explosionVfxPrefab != null)
            {
                GameObject explosion = Instantiate(explosionVfxPrefab, position, bombPoint.rotation);
                ExplosionSpawned?.Invoke(explosion);
            }

            if (audioSource != null && bombExplosionSound != null)
            {
                audioSource.PlayOneShot(bombExplosionSound);
            }

            Exploded?.Invoke(position);
        }

        if (Keyboard.current != null && Keyboard.current[placementKey].wasPressedThisFrame)
        {
            TryPlaceBomb();
        }
    }

    public bool TryPlaceBomb()
    {
        if (PlayerHealth != null && PlayerHealth.IsDead())
        {
            return false;
        }

        if (activeBomb != null)
        {
            return false;
        }

        if (player == null || bombPoint == null || bombPrefab == null)
        {
            Debug.LogWarning("Bomb placement needs Player, Bomb Point, and Bomb Prefab assigned.", this);
            return false;
        }

        Vector3 offset = player.position - bombPoint.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > placementRadius * placementRadius)
        {
            return false;
        }

        if (!useBeatTiming)
        {
            activeBombUsesBeatTiming = songClock != null && songClock.IsRunning;
            double placementClock = activeBombUsesBeatTiming ? songClock.SongTime : Time.timeAsDouble;
            explosionTime = placementClock + FuseSeconds;
            SpawnBomb();
            return true;
        }

        if (songClock == null || !songClock.IsRunning || songClock.Map == null)
        {
            return false;
        }

        RhythmMap map = songClock.Map;
        if (float.IsNaN(map.beatsPerMinute) || float.IsInfinity(map.beatsPerMinute) || map.beatsPerMinute < 1f || float.IsNaN(map.firstBeatTime) || float.IsInfinity(map.firstBeatTime))
        {
            return false;
        }

        double now = songClock.SongTime;
        if (now < 0)
        {
            return false;
        }

        double beatDuration = 60.0 / map.beatsPerMinute;
        double beatIndex = Math.Round((now - map.firstBeatTime) / beatDuration);
        double placementTime = map.firstBeatTime + beatIndex * beatDuration;
        double window = Math.Min(beatWindowSeconds, beatDuration * 0.25);
        if (beatIndex < 0 || beatIndex <= lastPlacementBeat || Math.Abs(now - placementTime) > window)
        {
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
        activeBomb = Instantiate(
            bombPrefab,
            bombPoint.position,
            bombPoint.rotation
        );

        foreach (Collider bombCollider in
            activeBomb.GetComponentsInChildren<Collider>())
        {
            bombCollider.enabled = false;
        }

        if (activeBomb.GetComponent<BombPopAnimation>() == null)
        {
            activeBomb.AddComponent<BombPopAnimation>();
        }

        if (audioSource != null && bombPlaceSound != null)
        {
            audioSource.PlayOneShot(bombPlaceSound);
        }

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
        {
            songClock.MapStopped -= ClearBomb;
        }

        ClearBomb();
    }
}
