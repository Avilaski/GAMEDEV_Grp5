
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public TMP_Text healthText;
    public Transform spawnPoint;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth, 0, maxHealth
        );

        UpdateHealthUI();

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        currentHealth = Mathf.Clamp(
            currentHealth, 0, maxHealth
        );

        UpdateHealthUI();
    }

    void Die()
    {
        Debug.Log("Game Over! Respawning...");

        if (spawnPoint != null)
        {
            if (controller != null)
                controller.enabled = false;

            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            if (controller != null)
                controller.enabled = true;
        }

        ResetHealth();
    }

    void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }
}
