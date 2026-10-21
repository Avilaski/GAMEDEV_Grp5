using System;
using UnityEngine;

public class LaneMover : MonoBehaviour
{
    public event Action ReachedEnd;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private double spawnTime;
    private double arrivalTime;
    private bool isMoving;

    public void Initialize(Vector3 start, Vector3 end, double spawnAt, double arriveAt)
    {
        startPosition = start;
        endPosition = end;
        spawnTime = spawnAt;
        arrivalTime = arriveAt;
        transform.position = start;
        isMoving = arrivalTime > spawnTime;
    }

    // The controller supplies the same song timestamp to every object.
    public void UpdatePosition(double songTime)
    {
        if (!isMoving)
            return;

        float progress = Mathf.Clamp01((float)((songTime - spawnTime) / (arrivalTime - spawnTime)));

        transform.position = Vector3.Lerp(startPosition, endPosition, progress);

        if (songTime >= arrivalTime)
        {
            isMoving = false;
            ReachedEnd?.Invoke();
            Destroy(gameObject);
        }
    }
}
