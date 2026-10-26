using System;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> HealthChanged;
    public event Action Died;
    public HealthBar healthBar;
    public GameOverManager gameOverManager;
    private bool isDead;

    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;
    public TMP_Text healthText;
    public Transform spawnPoint;
    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        ResetHealth();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        Died?.Invoke();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
    }

    void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }
}
