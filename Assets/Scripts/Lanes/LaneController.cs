using System.Collections.Generic;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    private Transform despawnPoint;
    private readonly List<LaneMover> activeObjects = new List<LaneMover>();
    public bool IsConfigured => startPoint != null && despawnPoint != null;

    public bool TryGetBombArrival(LaneSpawnEntry entry, float expansionSeconds, out float arrival)
    {
        arrival = entry.arrivalTime;
        BombController bomb = GetComponent<BombController>();
        if (!IsConfigured || bomb == null || !bomb.TryGetBlastEntry(startPoint.position, despawnPoint.position, out float progress))
        {
            return false;
        }

        float impactTime = entry.bombPlacementTime + bomb.FuseSeconds + expansionSeconds;
        arrival = entry.spawnTime + (impactTime - entry.spawnTime) / progress;
        return arrival > impactTime && !float.IsInfinity(arrival) && !float.IsNaN(arrival);
    }

    public LaneMover Spawn(GameObject prefab, double spawnTime, double arrivalTime)
    {
        if (!IsConfigured || prefab == null || arrivalTime <= spawnTime)
        {
            Debug.LogError("Assign lane endpoints, a prefab, and a positive travel duration.", this);
            return null;
        }

        GameObject spawned = Instantiate(prefab, startPoint.position, prefab.transform.rotation);
        LaneMover instance = spawned.GetComponent<LaneMover>();
        if (instance == null)
        {
            instance = spawned.AddComponent<LaneMover>();
        }

        instance.Initialize(startPoint.position, despawnPoint.position, spawnTime, arrivalTime);
        LaneEnemy enemy = spawned.GetComponent<LaneEnemy>();
        if (enemy != null)
        {
            BombController bomb = GetComponent<BombController>();
            enemy.Initialize(bomb != null ? bomb.PlayerHealth : null);
        }

        LaneCrate crate = spawned.GetComponent<LaneCrate>();
        if (crate != null)
        {
            BombController bomb = GetComponent<BombController>();
            crate.Initialize(bomb != null ? bomb.PlayerHealth : null, despawnPoint.position);
        }

        activeObjects.Add(instance);
        return instance;
    }

    public void UpdateObjects(double songTime)
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (activeObjects[i] == null)
            {
                activeObjects.RemoveAt(i);
                continue;
            }

            activeObjects[i].UpdatePosition(songTime);
        }
    }

    public void ClearObjects()
    {
        foreach (LaneMover instance in activeObjects)
        {
            if (instance != null)
            {
                instance.gameObject.SetActive(false);
                Destroy(instance.gameObject);
            }
        }

        activeObjects.Clear();
    }

    private void OnDestroy()
    {
        ClearObjects();
    }
}
