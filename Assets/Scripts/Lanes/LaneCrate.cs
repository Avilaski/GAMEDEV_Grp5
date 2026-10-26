using UnityEngine;

public enum CrateType
{
    HP,
    Speed,
    Light
}

[RequireComponent(typeof(LaneMover))]
public class LaneCrate : MonoBehaviour
{
    [SerializeField]
    private CrateType crateType;
    [SerializeField, Min(0f)]
    private float duration = 8f;
    [SerializeField, Min(1f)]
    private float multiplier = 1.25f;
    private const float CollectionTolerance = 0.9f;
    private LaneMover mover;
    private PlayerCrateEffects player;
    private float collectionLaneX;

    private void Awake()
    {
        mover = GetComponent<LaneMover>();
    }

    private void OnEnable()
    {
        mover.ReachedEnd += Collect;
    }

    private void OnDisable()
    {
        mover.ReachedEnd -= Collect;
    }

    public void Initialize(PlayerHealth health, Vector3 collectionPoint)
    {
        player = health != null ? health.GetComponent<PlayerCrateEffects>() : null;
        collectionLaneX = collectionPoint.x;
    }

    private void Collect()
    {
        if (player == null || Mathf.Abs(player.transform.position.x - collectionLaneX) > CollectionTolerance)
        {
            return;
        }

        player.Collect(crateType, multiplier, duration);
    }
}
