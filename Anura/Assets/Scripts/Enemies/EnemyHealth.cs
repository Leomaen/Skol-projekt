using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        
        // Make sure this object has the "Enemy" tag
        if (gameObject.tag != "Enemy")
        {
            gameObject.tag = "Enemy";
            Debug.LogWarning($"Set missing 'Enemy' tag on {gameObject.name}");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // You can add death animation, sound effects, or particle effects here
        
        // Destroy the enemy
        Destroy(gameObject);
    }
}
