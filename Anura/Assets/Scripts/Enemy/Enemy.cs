using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] int health;

    public void takeDamage() {
        health -= StatsManager.Instance.damage;

        if(health <= 0)
        {
            Die();
        }
    }

    void Die() 
    {
        Destroy(gameObject);
    }
}
