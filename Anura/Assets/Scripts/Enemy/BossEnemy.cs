using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float randomMoveMinDuration = 1.5f;
    public float randomMoveMaxDuration = 3.5f;
    public float randomIdleMinDuration = 1.0f;
    public float randomIdleMaxDuration = 2.5f;

    [Header("Attack Properties")]
    public float attackCooldown = 3f;
    public float attackChance = 0.7f;
    public float rushSpeed = 8f;
    public float rushCooldown = 5f;
    public float rushDistance = 10f;
    public float laserChargeDuration = 1.5f;
    public float laserActiveDuration = 1.0f;
    public float laserCooldown = 4f;
    public GameObject laserPrefab;
    public Transform firePoint;
    
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("References")]
    public Transform player;
    public LayerMask wallLayer;
    public Animator animator;
    
    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float contactDamageCooldown = 1.0f;
    private float lastContactDamageTime = 0f;
    private HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();
    
    [Header("Damage Visual")]
    public float flashDuration = 0.15f;
    public Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f); // Red tint

    // Components
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D bossCollider;
    
    // State variables
    private float lastAttackTime;
    private float lastRushTime;
    private float lastLaserTime;
    private bool isAttacking = false;
    private float movementStateTimer; 
    private Vector2 currentMoveDirection;
    private bool isCurrentlyMoving; // True if in moving state,

    // Add a field for damage flash effect
    private Color originalColor;
    private bool isFlashing = false;

    // Add a new class variable to track when laser is active
    private bool isLaserActive = false;

    void Start()
    {
        InitializeComponents();
        InitializeTimers();
    }

    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<BoxCollider2D>();
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color; // Store original color
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void InitializeTimers()
    {
        lastAttackTime = -attackCooldown;
        lastRushTime = -rushCooldown;
        lastLaserTime = -laserCooldown;

        // Initialize random movement state
        movementStateTimer = Random.Range(randomIdleMinDuration, randomIdleMaxDuration);
        isCurrentlyMoving = false; // Start with idling
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (player == null) return;

        HandleFacing();
        
        if (!isAttacking)
        {
            TryAttack();
            HandleMovement();
        }
    }
    
    void HandleFacing()
    {
        // Don't change facing during laser attacks
        if (isLaserActive)
            return;
            
        bool faceLeft = player.position.x < transform.position.x;
        spriteRenderer.flipX = faceLeft;
        
        // Update firePoint position based on facing
        if (firePoint != null)
        {
            Vector3 localPos = firePoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (faceLeft ? -1 : 1);
            firePoint.localPosition = localPos;
        }
    }
    
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown || Random.value > attackChance)
            return;
            
        bool rushAvailable = Time.time - lastRushTime >= rushCooldown;
        bool laserAvailable = Time.time - lastLaserTime >= laserCooldown;
        
        // Choose attack based on availability and last used
        if (rushAvailable && laserAvailable)
        {
            // Favor the attack we haven't used recently
            float rushWeight = lastRushTime < lastLaserTime ? 0.7f : 0.3f;
            
        if (Random.value < rushWeight)
           {StartCoroutine(RushAttack());}
        else
            {StartCoroutine(LaserAttack());}
        }
        else if (rushAvailable)
            {StartCoroutine(RushAttack());}
        else if (laserAvailable)
            {StartCoroutine(LaserAttack());}
    }
    
    void FixedUpdate()
    {
        // if (isMovingToWaypoint && !isAttacking && waypoints.Length > 0) // Removed
        //     MoveTowardWaypoint(); // Removed

        if (isCurrentlyMoving && !isAttacking)
        {
            // Check for walls in path
            // Use bossCollider.size.x * 0.5f (or an average of x and y if not square) as a base for raycast distance to avoid getting stuck
            float raycastDistance = moveSpeed * Time.fixedDeltaTime + (bossCollider != null ? bossCollider.size.x * 0.5f : 0.1f);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentMoveDirection, raycastDistance, wallLayer);

            if (hit.collider == null)
            {
                if (rb != null)
                    rb.MovePosition(rb.position + currentMoveDirection * moveSpeed * Time.fixedDeltaTime);
            }
            else
            {
                // Hit a wall, transition to idling immediately
                isCurrentlyMoving = false;
                movementStateTimer = Random.Range(randomIdleMinDuration, randomIdleMaxDuration);
                if (animator != null)
                    animator.SetBool("isWalking", false);
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }
        else if (!isAttacking && rb != null) // Ensure stopped if not attacking and not in moving state
        {
             rb.linearVelocity = Vector2.zero;
        }
    }
    
    void HandleMovement()
    {
        // if (waypoints.Length == 0) return; // Removed
        
        if (isAttacking)
        {
            if (rb != null) 
                rb.linearVelocity = Vector2.zero;
            if (animator != null)
                animator.SetBool("isWalking", false);
            isCurrentlyMoving = false; // Stop random movement logic if an attack starts
            return;
        }

        movementStateTimer -= Time.deltaTime;

        if (movementStateTimer <= 0)
        {
            // Switch state
            isCurrentlyMoving = !isCurrentlyMoving;

            if (isCurrentlyMoving)
            {
                // Switched to Moving state
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                currentMoveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                movementStateTimer = Random.Range(randomMoveMinDuration, randomMoveMaxDuration);
                if (animator != null)
                    animator.SetBool("isWalking", true);
            }
            else
            {
                // Switched to Idling state
                movementStateTimer = Random.Range(randomIdleMinDuration, randomIdleMaxDuration);
                if (animator != null)
                    animator.SetBool("isWalking", false);
                if (rb != null)
                    rb.linearVelocity = Vector2.zero; // Stop movement when starting to idle
            }
        }
    }
    
    IEnumerator RushAttack()
    {
        PrepareAttack();
        lastRushTime = Time.time;
        
        animator.SetBool("isWalking", false);
        
        // Pre-animation delay then trigger rush
        yield return new WaitForSeconds(0.2f);
        animator.SetTrigger("rushTrigger");
        yield return new WaitForSeconds(0.3f);
        
        // Calculate rush path toward player
        Vector3 targetPos = player.position;
        Vector2 rushDirection = (targetPos - transform.position).normalized;
        
        // Check for obstacles and adjust distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rushDirection, rushDistance, wallLayer);
        float actualDistance = hit.collider != null ? hit.distance * 0.9f : rushDistance;
        
        // Perform rush
        animator.SetBool("isRushing", true);
        yield return PerformRushMovement(rushDirection, actualDistance);
        animator.SetBool("isRushing", false);
        
        // Cooldown period
        yield return new WaitForSeconds(0.7f);
        
        FinishAttack();
    }
    
    IEnumerator PerformRushMovement(Vector2 direction, float distance)
    {
        float duration = distance / rushSpeed;
        float timer = 0;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (direction * distance);
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            // Check for walls during rush
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, bossCollider.size.x * 0.5f + 0.1f, wallLayer);
            if (hit.collider != null) break;
            
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, timer / duration));
            yield return null;
        }
    }
    
    IEnumerator LaserAttack()
    {
        PrepareAttack();
        lastLaserTime = Time.time;
        
        // Lock orientation when starting laser
        bool facingLeft = spriteRenderer.flipX;
        
        // Set flag to prevent rotation during the entire laser sequence
        isLaserActive = true;
        
        // Record player position before starting the charge animation
        Vector3 targetPosition = player.position;
        
        // Start laser charge
        animator.SetBool("isWalking", false);
        animator.SetTrigger("laserChargeTrigger");

        // Create a more visible indicator with a primitive shape
        GameObject indicatorObj = new GameObject("LaserTargetIndicator");
        indicatorObj.transform.position = targetPosition;
        
        // Add a sprite renderer and use a simple circle sprite
        SpriteRenderer indicator = indicatorObj.AddComponent<SpriteRenderer>();
        indicator.color = new Color(1f, 0f, 0f, 0.7f); // More visible red
        
        // Create a simple circle sprite dynamically
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                colors[y * 32 + x] = distanceFromCenter <= 14 ? 
                    new Color(1, 0, 0, 0.7f) : // Red inside circle
                    new Color(0, 0, 0, 0); // Transparent outside
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite circleSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        indicator.sprite = circleSprite;
        indicator.sortingOrder = 10; // Ensure it appears above other elements
        
        // Scale it for better visibility
        indicatorObj.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Make it pulse for better visibility
        StartCoroutine(PulseIndicator(indicatorObj));
        
        // Wait for charge duration
        yield return new WaitForSeconds(laserChargeDuration);
        
        // Use the recorded position from before the charge
        Vector2 laserDirection = (targetPosition - transform.position).normalized;
        FireLaser(laserDirection);
        
        // Destroy the indicator
        if (indicatorObj != null)
            Destroy(indicatorObj);
        
        yield return new WaitForSeconds(laserActiveDuration);
        
        // End laser sequence
        animator.SetTrigger("laserEndTrigger");
        yield return new WaitForSeconds(1.0f);
        
        // Reset the laser active flag
        isLaserActive = false;
        
        FinishAttack();
    }
    
    // Helper coroutine to make the indicator pulse
    private IEnumerator PulseIndicator(GameObject indicator)
    {
        if (indicator == null) yield break;
        
        float duration = laserChargeDuration;
        float elapsed = 0f;
        Vector3 originalScale = indicator.transform.localScale;
        
        while (elapsed < duration && indicator != null)
        {
            float scale = 0.8f + 0.4f * Mathf.PingPong(elapsed * 4f, 1f);
            indicator.transform.localScale = originalScale * scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    void FireLaser(Vector2 direction)
    {
        if (laserPrefab == null) return;
        
        // Add a slight randomness to make it more forgiving
        float randomAngleOffset = Random.Range(-5f, 5f);
        
        // Apply the rotation to the direction vector
        float radians = randomAngleOffset * Mathf.Deg2Rad;
        Vector2 offsetDirection = new Vector2(
            direction.x * Mathf.Cos(radians) - direction.y * Mathf.Sin(radians),
            direction.x * Mathf.Sin(radians) + direction.y * Mathf.Cos(radians)
        );
        
        // Create and orient laser with the offset direction
        float angle = Mathf.Atan2(offsetDirection.y, offsetDirection.x) * Mathf.Rad2Deg;
        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        
        // Configure laser behavior
        Laser laserScript = laser.GetComponent<Laser>();
        if (laserScript != null)
        {
            laserScript.direction = offsetDirection;
            laserScript.duration = laserActiveDuration;
            laserScript.damage = 1;
        }
        else
        {
            Destroy(laser, laserActiveDuration);
        }
    }
    
    void PrepareAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;
    }
    
    void FinishAttack()
    {
        isAttacking = false;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Add visual feedback - red flash when taking damage
        if (!isFlashing)
        {
            StartCoroutine(DamageFlashEffect());
        }
        
        if (currentHealth <= 0)
            Die();
    }
    
    // Add a new coroutine for the flash effect
    private IEnumerator DamageFlashEffect()
    {
        isFlashing = true;
        
        // Change to damage flash color
        spriteRenderer.color = damageFlashColor;
        
        // Wait for flash duration
        yield return new WaitForSeconds(flashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
        
        isFlashing = false;
    }
    
    void Die()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider is the player and enough time has passed since last damage
        if (other.CompareTag("Player") && Time.time - lastContactDamageTime >= contactDamageCooldown)
        {
            // Reset the timer for contact damage
            lastContactDamageTime = Time.time;
            
            // Try to get player controller and apply damage
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
                Debug.Log("Boss contact hit player for " + contactDamage + " damage");
            }
        }
    }
}
