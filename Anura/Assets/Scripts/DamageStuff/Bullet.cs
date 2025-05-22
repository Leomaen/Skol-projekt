using UnityEngine;
using System.Linq; // Required for Count()

public class Bullet : MonoBehaviour
{
    public GameState gameState;
    public Rigidbody2D rb;
    public Animator animator;
    public GameObject explosionPrefab; // Assign your explosion prefab in the Inspector

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Assuming you have an Animator component
    }

    void Start()
    {
        if (gameState == null || gameState.stats == null)
        {
            Debug.LogError("GameState or GameState.stats is not assigned/initialized in Bullet. Destroying bullet.");
            Destroy(gameObject);
            return;
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on the bullet. Destroying bullet.");
            Destroy(gameObject);
            return;
        }
        rb.linearVelocity = transform.right * gameState.stats.bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Vector3 impactPosition = transform.position;
        bool hitSomethingSignificant = false; // Renamed for clarity

        // Determine if explosive and how many stacks
        int explosiveModifierCount = 0;
        if (ItemManager.Instance != null && ItemManager.Instance.gameState != null && ItemManager.Instance.gameState.activeWeaponModifiers != null)
        {
            explosiveModifierCount = ItemManager.Instance.gameState.activeWeaponModifiers.Count(m => m.modifierType == WeaponModifierType.Explosive);
        }
        bool isExplosive = explosiveModifierCount > 0;

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (animator != null) animator.SetTrigger("hitWall");
            if (rb != null) rb.linearVelocity = Vector2.zero;
            // Bullet will be destroyed after a short delay for animation.
            // Explosion (if any) will be handled by HandleImpactEffects.
            Destroy(gameObject, 0.25f); 
            hitSomethingSignificant = true;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!isExplosive) // Bullet deals direct damage ONLY IF NOT explosive
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.takeDamage(); 
                }

                BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
                if (bossEnemy != null)
                {
                    if (gameState != null && gameState.stats != null)
                    {
                        bossEnemy.TakeDamage(gameState.stats.damage);
                    }
                    else
                    {
                        Debug.LogError("Cannot deal damage to BossEnemy: GameState or stats missing.");
                    }
                }
            }
            // Bullet is always destroyed on enemy hit, explosion handles damage if explosive
            Destroy(gameObject);
            hitSomethingSignificant = true;
        }

        if (hitSomethingSignificant && isExplosive) // Only create explosion if it's an explosive bullet and hit something
        {
            HandleImpactEffects(impactPosition, explosiveModifierCount);
        }
    }

    void HandleImpactEffects(Vector3 position, int explosiveCount)
    {
        // This method is now only called if explosiveCount > 0 by the OnTriggerEnter2D logic
        if (explosiveCount > 0 && explosionPrefab != null)
        {
            GameObject explosionGO = Instantiate(explosionPrefab, position, Quaternion.identity);
            Explosion explosionScript = explosionGO.GetComponent<Explosion>();
            if (explosionScript != null)
            {
                // Scale explosion size: 1st item = base size, each additional item increases size by 20% of base
                explosionScript.sizeMultiplier = 1f + ((explosiveCount - 1) * 0.20f); 
            }
            else
            {
                Debug.LogError("Explosion prefab is missing the Explosion script component!");
            }
        }
        else if (explosiveCount > 0 && explosionPrefab == null)
        {
            Debug.LogWarning("Explosive modifier active, but no explosionPrefab assigned to the Bullet.");
        }
    }
}