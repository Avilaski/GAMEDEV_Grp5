using UnityEngine;

// Only monster prefabs carry this component; future crates can have their own response.
[RequireComponent(typeof(LaneMover))]
public class LaneEnemy : MonoBehaviour
{
    [SerializeField, Min(0)] private int arrivalDamage = 1;
    private LaneMover mover;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        mover = GetComponent<LaneMover>();
    }

    public void Initialize(PlayerHealth health)
    {
        playerHealth = health;
    }

    private void OnEnable()
    {
        mover.ReachedEnd += DamagePlayer;
    }

    private void OnDisable()
    {
        mover.ReachedEnd -= DamagePlayer;
    }

    private void DamagePlayer()
    {
        if (playerHealth != null)
            playerHealth.TakeDamage(arrivalDamage);
    }
}
