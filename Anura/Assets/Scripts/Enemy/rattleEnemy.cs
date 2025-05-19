using UnityEngine;
using System.Collections;

public class rattleEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
    
    [Header("Rush Attack")]
    public float rushSpeed = 6f;
    public float rushDistance = 8f;
    public float rushCooldown = 3f;
    public float rushChargeTime = 0.5f;
    private float lastRushTime;
    private bool isRushing = false;
    
    [Header("Health")]
    public int health = 3;
    
    [Header("Damage")]
    public int contactDamage = 1;
    public float damageInterval = 1.0f;
    private float lastDamageTime;
    
    [Header("References")]
    public LayerMask wallLayer;
    
    // Components
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    
    // State tracking
    private bool isDead = false;
    private Vector2 moveDirection;
    private bool canRush = true;
    
    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Initialize timers
        lastRushTime = -rushCooldown; // Allow immediate rush
        lastDamageTime = -damageInterval;
        
        // Set default animation
        if (animator != null)
            animator.SetBool("isIdle", true);
    }

    void Update()
    {
        if (isDead || player == null) return;
        
        // Check if ready for another rush
        if (Time.time - lastRushTime >= rushCooldown && !isRushing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                StartCoroutine(PerformRushAttack());
            }
        }
        
        // Handle regular movement when not rushing
        if (!isRushing)
        {
            HandleMovement();
        }
        
        // Handle sprite facing direction
        UpdateFacingDirection();
    }
    
    void HandleMovement()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Only move if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            moveDirection = (player.position - transform.position).normalized;
            
            // Set animation to running
            animator.SetBool("isIdle", false);
            animator.SetBool("isRun", true);
            
            // Move toward player
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            // Set to idle if player not detected
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isRun", false);
            animator.SetBool("isIdle", true);
        }
    }
    
    void UpdateFacingDirection()
    {
        if (player == null) return;
        
        // Flip sprite based on player position
        spriteRenderer.flipX = transform.position.x > player.position.x;
    }
    
    IEnumerator PerformRushAttack()
    {
        // Start rush sequence
        isRushing = true;
        canRush = false;
        lastRushTime = Time.time;
        
        // Store original velocity and calculate rush direction
        Vector2 originalVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero;
        
        // Pre-rush charge animation
        animator.SetBool("isIdle", false);
        animator.SetBool("isRun", true);
        
        // Wait for charge time
        yield return new WaitForSeconds(rushChargeTime);
        
        if (player != null && !isDead)
        {
            // Calculate rush direction toward player's position
            Vector2 rushDirection = (player.position - transform.position).normalized;
            
            // Check for obstacles and adjust distance
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rushDirection, rushDistance, wallLayer);
            float actualDistance = hit.collider != null ? hit.distance * 0.9f : rushDistance;
            
            // Perform rush
            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + (rushDirection * actualDistance);
            
            float duration = actualDistance / rushSpeed;
            float timer = 0;
            
            while (timer < duration && !isDead)
            {
                timer += Time.deltaTime;
                
                rb.MovePosition(Vector2.Lerp(startPos, targetPos, timer / duration));
                yield return null;
            }
        }
        
        // End rush sequence
        rb.linearVelocity = Vector2.zero;
        isRushing = false;
        
        // Add a small delay before allowing next rush
        yield return new WaitForSeconds(0.2f);
        
        // Return to regular movement
        animator.SetBool("isRun", false);
        animator.SetBool("isIdle", true);
    }
    
    public void takeDamage()
    {
        if (isDead) return;
        
        health--;
        
        // Play hit animation
        animator.SetTrigger("isHit");
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        
        // Disable collisions
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // Play death animation
        animator.SetTrigger("isDeath");
        
        // Disable all other animation states
        animator.SetBool("isRun", false);
        animator.SetBool("isIdle", false);
        
        // Destroy after animation plays
        Destroy(gameObject, 1.0f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        
        if (other.CompareTag("Player"))
        {
            // Apply contact damage to player
            if (Time.time - lastDamageTime >= damageInterval)
            {
                lastDamageTime = Time.time;
                
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(contactDamage);
                }
            }
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Handle continuous collision damage with interval
        if (other.CompareTag("Player") && Time.time - lastDamageTime >= damageInterval)
        {
            lastDamageTime = Time.time;
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }
}
