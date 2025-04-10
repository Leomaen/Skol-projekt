using System.Collections;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    // Boss States
    private enum BossState 
    { 
        Moving,     // Moving in a pattern
        Charging,   // Charging up before shooting
        Shooting,   // Standing still and shooting
        Rushing,    // Rushing toward player
        Resting     // Brief recovery period
    }
    
    [Header("Boss Settings")]
    [SerializeField] private int bossmaxHealth = 30;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] movePoints; // Assign waypoints in the editor
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushDamage = 20f;
    [SerializeField] private float rushCooldown = 5f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private int burstCount = 5;
    [SerializeField] private float burstDelay = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.5f; // Distance to check for walls
    [SerializeField] private LayerMask wallLayer; // Layer for walls
    [SerializeField] private float rushDistance = 15f; // Maximum rush distance
    
    [Header("Laser Settings")]
    [SerializeField] private float chargeUpTime = 1.5f; // Time to charge before firing
    [SerializeField] private float laserDuration = 3.0f; // How long the laser firing lasts
    [SerializeField] private GameObject chargingEffectPrefab; // Optional visual effect for charging
    
    // Collision detection
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float collisionRadius = 1.5f;
    
    // State tracking
    private BossState currentState = BossState.Moving;
    private int currentMovePointIndex = 0;
    private float lastStateChangeTime = 0f;
    private float lastRushTime = 0f;
    private float lastShootTime = 0f;
    private Vector2 rushDirection;
    private GameObject currentChargingEffect;
    private Vector2 targetShootDirection;
    
    protected override void Awake()
    {
        base.Awake();
        currentHealth = bossmaxHealth;
    }
    
    protected override void Start()
    {
        base.Start();
        
    }
    
    protected override void Update()
    {
        if (playerTransform == null) return;
        
        // Handle state transitions
        UpdateState();
        
        // Handle animations
        if (animator != null)
        {
            animator.SetBool("IsMoving", rb.linearVelocity.sqrMagnitude > 0.1f);
            animator.SetInteger("State", (int)currentState);
            
            // Normal sprite flipping based on movement direction or target
            Vector2 directionToFace = currentState == BossState.Rushing ? 
                rushDirection : (playerTransform.position - transform.position);
                
            if (directionToFace.x != 0)
            {
                spriteRenderer.flipX = (directionToFace.x < 0);
            }
        }
    }
    
    protected override void FixedUpdate()
    {
        // Override base behavior completely
        switch (currentState)
        {
            case BossState.Moving:
                MoveToNextPoint();
                break;
                
            case BossState.Charging:
            case BossState.Shooting:
                // Stand still when charging or shooting
                rb.linearVelocity = Vector2.zero;
                break;
                
            case BossState.Rushing:
                // Check if we're about to hit a wall
                if (WouldHitWall(rushDirection))
                {
                    // Stop rushing if we're about to hit a wall
                    SwitchState(BossState.Resting);
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    // Continue rushing in the set direction
                    rb.linearVelocity = rushDirection * rushSpeed;
                    
                    // Check if we hit the player during rush
                    Collider2D playerHit = Physics2D.OverlapCircle(transform.position, collisionRadius, playerLayer);
                    if (playerHit != null)
                    {
                        PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage((int)rushDamage);
                        }
                        
                        // End rush after hitting player
                        SwitchState(BossState.Resting);
                    }
                }
                break;
                
            case BossState.Resting:
                // Stand still when resting
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
    
    private void UpdateState()
    {
        float timeInCurrentState = Time.time - lastStateChangeTime;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        switch (currentState)
        {
            case BossState.Moving:
                // After moving for a while, switch to charging
                if (timeInCurrentState > 5f && Time.time > lastShootTime + shootCooldown)
                {
                    targetShootDirection = (playerTransform.position - transform.position).normalized;
                    SwitchState(BossState.Charging);
                    StartChargeUp();
                }
                // Occasionally rush at player if enough time has passed
                else if (distanceToPlayer < detectionRange && Time.time > lastRushTime + rushCooldown)
                {
                    SwitchState(BossState.Rushing);
                    PrepareRush();
                }
                break;
                
            case BossState.Charging:
                // After charge-up time, switch to shooting
                if (timeInCurrentState > chargeUpTime)
                {
                    SwitchState(BossState.Shooting);
                    StartCoroutine(FireLaserBeam());
                }
                break;
                
            case BossState.Shooting:
                // Shooting state duration is handled by the FireLaserBeam coroutine
                // No state transitions here - the coroutine will handle it
                break;
                
            case BossState.Rushing:
                // Rush for a limited time or until hitting a wall
                if (timeInCurrentState > 1.5f || IsOutsideRoom())
                {
                    SwitchState(BossState.Resting);
                }
                break;
                
            case BossState.Resting:
                // Rest briefly after rushing or shooting
                if (timeInCurrentState > 1f)
                {
                    SwitchState(BossState.Moving);
                }
                break;
        }
    }
    
    private void SwitchState(BossState newState)
    {
        currentState = newState;
        lastStateChangeTime = Time.time;
        
        // Handle specific state entry actions
        switch (newState)
        {
            case BossState.Rushing:
                lastRushTime = Time.time;
                if (animator != null)
                {
                    animator.SetTrigger("Rush");
                }
                break;
                
            case BossState.Shooting:
                lastShootTime = Time.time;
                if (animator != null)
                {
                    animator.SetTrigger("Shoot");
                }
                break;
                
            case BossState.Charging:
                if (animator != null)
                {
                    animator.SetTrigger("Charge");
                }
                break;
        }
    }
    
    private void MoveToNextPoint()
    {
        if (movePoints == null || movePoints.Length == 0) return;
        
        // Move toward current target point
        Vector2 targetPoint = movePoints[currentMovePointIndex].position;
        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
        
        // Check for walls before moving
        if (!WouldHitWall(direction))
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // If wall detected, try a different waypoint
            currentMovePointIndex = (currentMovePointIndex + 1) % movePoints.Length;
            rb.linearVelocity = Vector2.zero;
        }
        
        // Check if we reached the current point
        if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
        {
            // Move to next point
            currentMovePointIndex = (currentMovePointIndex + 1) % movePoints.Length;
        }
    }
    
    private void PrepareRush()
    {
        // Calculate rush path
        if (playerTransform != null)
        {
            // Get direction to player
            rushDirection = (playerTransform.position - transform.position).normalized;
            
            // Calculate rush endpoint
            Vector2 potentialEndpoint = (Vector2)transform.position + rushDirection * rushDistance;
            
            // Check if we'll hit a wall before the endpoint
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rushDirection, rushDistance, wallLayer);
            if (wallHit.collider != null)
            {
                // We'll hit a wall, so rush only to that point (with a small offset to avoid collision)
                float safeDistance = wallHit.distance - 0.5f;
                if (safeDistance < 1f) safeDistance = 1f; // Minimum rush distance
                
                Debug.Log($"Rush will hit wall at distance {wallHit.distance}, using safe distance {safeDistance}");
            }
            
            // Prepare for rush (visual/sound effects would go here)
            if (animator != null)
            {
                animator.SetTrigger("PrepareRush");
            }
        }
    }
    
    private void StartChargeUp()
    {
        // Spawn charging effect
        if (chargingEffectPrefab != null)
        {
            currentChargingEffect = Instantiate(chargingEffectPrefab, transform.position, Quaternion.identity);
            currentChargingEffect.transform.parent = transform;
        }
    }
    
    private IEnumerator FireLaserBeam()
    {
        // Clean up charging effect
        if (currentChargingEffect != null)
        {
            Destroy(currentChargingEffect);
        }
        
        // Fire multiple laser beams over time
        for (int i = 0; i < burstCount; i++)
        {
            ShootLaserBeam();
            yield return new WaitForSeconds(burstDelay);
        }
        
        // Wait for the laser duration to complete
        yield return new WaitForSeconds(laserDuration - (burstCount * burstDelay));
        
        // Return to moving state
        SwitchState(BossState.Resting);
    }
    
    private void ShootLaserBeam()
    {
        if (projectilePrefab == null) return;
        
        // Use the saved target direction with slight randomness
        Vector2 shootDirection = RotateVector(targetShootDirection, Random.Range(-5f, 5f));
        
        // Create projectile and position it properly
        Vector2 spawnPosition = (Vector2)transform.position + shootDirection * 1f; // Offset slightly
        GameObject projectile = Instantiate(
            projectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );
        
        // Set projectile direction and speed
        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(shootDirection, projectileSpeed);
        }
    }
      
    // Check if the boss is outside the expected room boundaries
    private bool IsOutsideRoom()
    {
        // Check if we've already gone too far
        return false;
    }
    
    // New methods to check for walls
    private bool WouldHitWall(Vector2 direction)
    {
        // Increase check distance during rush state to stop earlier
        float checkDistance = currentState == BossState.Rushing ? wallCheckDistance * 3 : wallCheckDistance;
        
        // Cast a ray in the movement direction to check for walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, wallLayer);
        return hit.collider != null;
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
    
    
    // New method to create custom waypoint pattern
    public void CreateCustomWaypoints(Vector2[] positions)
    {
        // Clear any existing waypoints first
        if (movePoints != null)
        {
            foreach (Transform point in movePoints)
            {
                if (point != null)
                {
                    Destroy(point.gameObject);
                }
            }
        }
        
        // Create new waypoints from positions
        movePoints = new Transform[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = new GameObject($"BossMovePoint_{i}");
            point.transform.parent = transform.parent;
            point.transform.position = positions[i];
            movePoints[i] = point.transform;
        }
    }
    
    // Override the Die method to potentially trigger special effects or game events
    protected override void Die()
    {
        // Play death animation/effect
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Disable enemy components but don't destroy immediately
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // Disable all colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // You might want to trigger special effects or level progression here
        
        // Destroy after animation completes
        Destroy(gameObject, 3.0f);
    }
    
}