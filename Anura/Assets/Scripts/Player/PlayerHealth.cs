using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;

    void Start()
    {
        // Get initial health from StatsManager
        if (StatsManager.Instance != null)
        {
            maxHealth = StatsManager.Instance.PlayerHealth;
            currentHealth = maxHealth;
        }
        else
        {
            Debug.LogError("StatsManager instance not found!");
            maxHealth = 5; // Default fallback value
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Update the StatsManager
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.PlayerHealth = currentHealth;
        }
        
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Update the StatsManager
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.PlayerHealth = currentHealth;
        }
    }
    
    private void Die()
    {
        Debug.Log("Player died!");
        // Handle player death (game over, respawn, etc.)
    }
}
