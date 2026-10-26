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

    public bool DestroyByBlast()
    {
        if (!isMoving || !gameObject.activeInHierarchy)
        {
            return false;
        }

        isMoving = false;
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }

    public void Initialize(Vector3 start, Vector3 end, double spawnAt, double arriveAt)
    {
        startPosition = start;
        endPosition = end;
        spawnTime = spawnAt;
        arrivalTime = arriveAt;
        transform.position = start;
        isMoving = arrivalTime > spawnTime;
    }

    public void UpdatePosition(double songTime)
    {
        if (!isMoving)
        {
            return;
        }

        float progress = Mathf.Clamp01((float)((songTime - spawnTime) / (arrivalTime - spawnTime)));
        transform.position = Vector3.Lerp(startPosition, endPosition, progress);

        if (songTime >= arrivalTime)
        {

            isMoving = false;
            ReachedEnd?.Invoke();
            Destroy(gameObject);

            if (CompareTag("RedMonster") ||
                    CompareTag("BlueMonster"))
            {
                ScoreManager.Instance.AddScore(-100);

            }
        }
    }
}
