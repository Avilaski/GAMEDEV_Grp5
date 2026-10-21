using System.Collections.Generic;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform despawnPoint;

    private readonly List<LaneMover> activeObjects = new List<LaneMover>();
    public bool IsConfigured => startPoint != null && despawnPoint != null;

    // Spawn immediately. SpawnController decides WHEN to call this method.
    public LaneMover Spawn(LaneMover prefab, double spawnTime, double arrivalTime)
    {
        if (!IsConfigured || prefab == null || arrivalTime <= spawnTime)
        {
            Debug.LogError("Assign lane endpoints, a prefab, and a positive travel duration.", this);
            return null;
        }

        LaneMover instance = Instantiate(prefab, startPoint.position, prefab.transform.rotation);
        instance.Initialize(startPoint.position, despawnPoint.position, spawnTime, arrivalTime);
        activeObjects.Add(instance);
        return instance;
    }

    public void UpdateObjects(double songTime)
    {
        // Iterate backwards so removing destroyed objects is safe.
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
