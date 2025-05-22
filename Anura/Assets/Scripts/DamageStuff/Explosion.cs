using UnityEngine;
using System.Collections.Generic;

public class Explosion : MonoBehaviour
{
    public float duration = 0.5f;           // How long the explosion visual/effect lasts
    // public int damage = 10;              // REMOVED - Damage will be handled by enemy.takeDamage() using gameState.stats.damage
    public float baseExplosionRadius = 1.5f; // Initial radius of the explosion
    public LayerMask enemyLayerMask;        // Set this in the Inspector to your Enemy layer
    public float sizeMultiplier = 1f;       // Multiplier for the radius, set by the bullet

    private HashSet<Collider2D> alreadyDamagedEnemies;

    void Start()
    {
        Destroy(gameObject, duration); 
        alreadyDamagedEnemies = new HashSet<Collider2D>();
        DealAreaDamage();
        // Optional: Play explosion sound
        // AudioManager.Instance.PlaySound("ExplosionSoundEffectName");
    }

    void DealAreaDamage()
    {
        float currentRadius = baseExplosionRadius * sizeMultiplier; // Use the multiplier
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, currentRadius, enemyLayerMask);

        foreach (var hitCollider in hitColliders)
        {
            if (alreadyDamagedEnemies.Contains(hitCollider))
            {
                continue; 
            }

            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.takeDamage(); // Enemy's takeDamage uses gameState.stats.damage
                alreadyDamagedEnemies.Add(hitCollider);
            }

            BossEnemy bossEnemy = hitCollider.GetComponent<BossEnemy>();
            if (bossEnemy != null)
            {
                if (ItemManager.Instance != null && ItemManager.Instance.gameState != null && ItemManager.Instance.gameState.stats != null) 
                {
                    bossEnemy.TakeDamage(ItemManager.Instance.gameState.stats.damage); // Boss uses gameState.stats.damage
                }
                else
                {
                    Debug.LogError("Cannot determine damage for BossEnemy: GameState or stats not found via ItemManager.");
                }
                alreadyDamagedEnemies.Add(hitCollider);
            }
        }
    }

    // Visualize the explosion radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // This will show the base radius unless sizeMultiplier is set at edit time,
        // or if you select an explosion instance at runtime.
        Gizmos.DrawWireSphere(transform.position, baseExplosionRadius * sizeMultiplier);
    }
}