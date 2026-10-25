
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;

    private int currentHealth;
    private bool isDead = false;

    [Header("References")]
    public HealthBar healthBar;
    public GameOverManager gameOverManager;

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(true);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth - damage,
            0,
            maxHealth
        );

        Debug.Log("Player Health: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth + amount,
            0,
            maxHealth
        );

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Game Over! Player has run out of health.");

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogError(
                "GameOverManager is not assigned to PlayerHealth!"
            );
        }
    }
}
