using UnityEngine;

public class DespawnMarker : MonoBehaviour
{
    [Header("Player Health")]
    public PlayerHealth playerHealth;
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crate"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(other.gameObject);
        }
    }
}
