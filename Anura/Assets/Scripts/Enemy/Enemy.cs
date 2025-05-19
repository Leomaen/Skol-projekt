using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameState gameState;
    [SerializeField] int health;

    public void takeDamage()
    {
        health -= gameState.stats.damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
