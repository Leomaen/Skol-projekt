using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameState gameState;
    public Rigidbody2D rb;
    public Animator animator;
    
    [Header("Homing Settings")]
    public float homingRange = 10f;
    public float homingStrength = 5f;
    
    private bool hasHomingModifier;
    private bool hasHitWall;
    private Transform targetEnemy;

    void Start()
    {
        // Check if homing modifier is active
        CheckForHomingModifier();
        
        rb.linearVelocity = transform.right * gameState.stats.bulletSpeed;
    }
    
    void Update()
    {
        if (hasHomingModifier && !hasHitWall)
        {
            HandleHoming();
        }
    }
    
    private void CheckForHomingModifier()
    {
        if (ItemManager.Instance != null && 
            ItemManager.Instance.gameState != null && 
            ItemManager.Instance.gameState.activeWeaponModifiers != null)
        {
            foreach (var modifier in ItemManager.Instance.gameState.activeWeaponModifiers)
            {
                if (modifier.modifierType == WeaponModifierType.Homing)
                {
                    hasHomingModifier = true;
                    break;
                }
            }
        }
    }
    
    private void HandleHoming()
    {
        // Find target if we don't have one or if current target is destroyed
        if (targetEnemy == null)
        {
            FindNearestEnemy();
        }
        
        // Home towards target if we have one
        if (targetEnemy != null)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            Vector2 currentVelocity = rb.linearVelocity;
            
            // Gradually adjust velocity towards target
            Vector2 newVelocity = Vector2.Lerp(currentVelocity.normalized, direction, homingStrength * Time.deltaTime);
            rb.linearVelocity = newVelocity * gameState.stats.bulletSpeed;
            
            // Rotate bullet to face movement direction
            float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= homingRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
        
        targetEnemy = nearestEnemy;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            hasHitWall = true;
            animator.SetTrigger("hitWall");
            Destroy(gameObject, 0.5f);
            rb.linearVelocity = transform.right * 0;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Check for regular enemy
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.takeDamage();
                Destroy(gameObject);
                return;
            }

            // Check for boss enemy
            BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
            if (bossEnemy != null)
            {
                bossEnemy.TakeDamage(gameState.stats.damage);
                Destroy(gameObject);
                return;
            }
        }
    }
}