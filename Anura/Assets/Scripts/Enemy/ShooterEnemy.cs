using System.Collections;
using UnityEngine;

public class ShooterEnemy : EnemyBase
{
    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootInterval = 2.0f;
    [SerializeField] private float shootSpeed = 5.0f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.2f;
    [SerializeField] private float preferredDistance = 5.0f;
    
    private float lastShootTime;

    protected override void Start()
    {
        base.Start();
        lastShootTime = -shootInterval; // Allow shooting soon after spawn
    }

    protected override void Update()
    {
        base.Update();
        
        if (isChasing && playerTransform != null)
        {
            // Check if it's time to shoot
            if (Time.time > lastShootTime + shootInterval)
            {
                StartCoroutine(ShootBurst());
                lastShootTime = Time.time;
            }
        }
    }

    protected override void ChooseNewPath()
    {
        if (playerTransform == null) return;
        
        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - transform.position);
        float distanceToPlayer = directionToPlayer.magnitude;
        directionToPlayer.Normalize();
        
        // Choose path based on preferred distance
        if (distanceToPlayer < preferredDistance - 0.5f)
        {
            // Too close, move away
            currentMoveDirection = -directionToPlayer;
        }
        else if (distanceToPlayer > preferredDistance + 0.5f)
        {
            // Too far, move closer
            currentMoveDirection = directionToPlayer;
        }
        else
        {
            // At a good distance, strafe around player
            currentMoveDirection = new Vector2(-directionToPlayer.y, directionToPlayer.x);
            // Randomly flip strafing direction
            if (Random.value > 0.5f)
                currentMoveDirection *= -1;
        }
        
        // Add some randomness
        float randomAngle = Random.Range(-20f, 20f);
        currentMoveDirection = RotateVector(currentMoveDirection, randomAngle);
    }
    
    private IEnumerator ShootBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            ShootProjectile();
            yield return new WaitForSeconds(burstDelay);
        }
    }
    
    private void ShootProjectile()
    {
        if (projectilePrefab == null || playerTransform == null) return;
        
        // Calculate direction to player with slight inaccuracy
        Vector2 shootDirection = (playerTransform.position - transform.position).normalized;
        shootDirection = RotateVector(shootDirection, Random.Range(-10f, 10f));
        
        // Create projectile
        GameObject projectile = Instantiate(
            projectilePrefab, 
            transform.position, 
            Quaternion.identity
        );
        
        // Set projectile velocity
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = shootDirection * shootSpeed;
        }
        
        // Set projectile direction if it has a custom script
        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(shootDirection, shootSpeed);
        }
        
        // Play shoot animation
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
    }
    
    // Helper method to rotate a vector by angle in degrees
    private Vector2 RotateVector(Vector2 v, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);
        
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}
