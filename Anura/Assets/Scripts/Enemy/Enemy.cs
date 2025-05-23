// filepath: c:\Users\leowik23\Documents\GitHub\Unityprojekt\Skol-projekt\Anura\Assets\Scripts\Enemy\Enemy.cs
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameState gameState;
    [SerializeField] protected int health; // Changed to protected

    // Made virtual so derived classes can override
    public virtual void takeDamage()
    {
        if (gameState == null)
        {
            Debug.LogError("GameState not assigned to Enemy: " + gameObject.name);
            return;
        }
        if (gameState.stats == null)
        {
            Debug.LogError("GameState.stats not initialized for Enemy: " + gameObject.name);
            return;
        }

        health -= gameState.stats.damage;
        // Debug.Log($"{gameObject.name} took {gameState.stats.damage} damage, health is now {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    // Made protected virtual so derived classes can override
    protected virtual void Die()
    {
        // Debug.Log($"{gameObject.name} died (base).");
        Destroy(gameObject);
    }
}