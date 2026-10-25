
using UnityEngine;

public class DespawnMarker : MonoBehaviour
{
    [Header("Player Health")]
    public PlayerHealth playerHealth;

    public int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Crate"))
            return;

        Debug.Log("DespawnMarker detected a Crate.");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("Player took " + damage + " damage.");
        }
        else
        {
            Debug.LogError(
                "PlayerHealth is not assigned to DespawnMarker!"
            );
        }

        Destroy(other.gameObject);
    }
}
