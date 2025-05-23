// filepath: c:\Users\leowik23\Documents\GitHub\Unityprojekt\Skol-projekt\Anura\Assets\Scripts\Enemy\rattleEnemy.cs
using UnityEngine;
using System.Collections;

public class rattleEnemy : Enemy // Inherit from Enemy
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

    [Header("Damage")]
    public int contactDamage = 1;
    public float damageInterval = 1.0f;
    private float lastDamageTime;

    [Header("Damage Visuals")] // New Header
    public float flashDuration = 0.1f;
    public Color damageFlashColor = Color.red;
    private Color originalColor;
    private bool isFlashing = false;

    [Header("References")]
    public LayerMask wallLayer;
    public UserData userData;

    // Components
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;

    // State tracking
    private bool isDead = false;
    private Vector2 moveDirection;
    // private bool canRush = true; // canRush was not used, consider removing if still unused

    void Start()
    {
        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) // Store original color
        {
            originalColor = spriteRenderer.color;
        }
        enemyCollider = GetComponent<Collider2D>();
        enemyCollider = GetComponent<Collider2D>();

        // Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player not found in scene for rattleEnemy: " + gameObject.name);
        }

        // Initialize timers
        lastRushTime = -rushCooldown; // Allow immediate rush
        lastDamageTime = -damageInterval;

        // Ensure GameState is assigned (it's inherited from Enemy, but needs to be linked in Inspector)
        if (gameState == null)
        {
            Debug.LogError("GameState not assigned to rattleEnemy: " + gameObject.name + ". Assign it in the Inspector.");
        }
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
        if (player == null || isRushing) return; // Don't move if no player or currently rushing

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            moveDirection = (player.position - transform.position).normalized;

            if (animator != null)
            {
                animator.SetBool("isIdle", false);
                animator.SetBool("isRun", true);
            }
            if (rb != null) rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (animator != null)
            {
                animator.SetBool("isRun", false);
                animator.SetBool("isIdle", true);
            }
        }
    }

    void UpdateFacingDirection()
    {
        if (player == null || spriteRenderer == null) return;
        spriteRenderer.flipX = transform.position.x > player.position.x;
    }

    IEnumerator PerformRushAttack()
    {
        if (isDead || player == null) yield break;

        isRushing = true;
        // canRush = false; // Not used elsewhere
        lastRushTime = Time.time;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isRun", true); // Or a specific charge animation
        }

        yield return new WaitForSeconds(rushChargeTime);

        if (player != null && !isDead) // Re-check player and isDead status
        {
            Vector2 rushDirection = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rushDirection, rushDistance, wallLayer);
            float actualDistance = hit.collider != null ? hit.distance * 0.9f : rushDistance; // Reduce slightly to avoid getting stuck

            Vector2 startPos = transform.position;
            Vector2 targetPos = startPos + (rushDirection * actualDistance);
            float duration = actualDistance / rushSpeed;
            if (duration <= 0) duration = 0.1f; // Prevent division by zero or instant rush
            float timer = 0;

            while (timer < duration && !isDead)
            {
                timer += Time.deltaTime;
                if (rb != null) rb.MovePosition(Vector2.Lerp(startPos, targetPos, timer / duration));
                yield return null;
            }
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;
        isRushing = false;

        yield return new WaitForSeconds(0.2f); // Small cooldown before resuming normal behavior

        if (animator != null && !isDead)
        {
            animator.SetBool("isRun", false);
            animator.SetBool("isIdle", true);
        }
    }

    // Override takeDamage from Enemy class
    public override void takeDamage()
    {
        if (isDead) return;
        AudioManager.Instance.PlaySound("RattleHurt");

        if (spriteRenderer != null && !isFlashing) // Add flash effect
        {
            StartCoroutine(DamageFlashEffect());
        }

        base.takeDamage(); // Calls the Enemy base class logic for health reduction and Die()
    }

    private IEnumerator DamageFlashEffect()
    {
        isFlashing = true;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
    // Override Die from Enemy class
    protected override void Die()
    {
        if (isDead) return; // Prevent Die from being called multiple times
        isDead = true;
        AudioManager.Instance.PlaySound("RattleDeath");
        userData.stats.totalKills++;
        userData.Save();

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("isDeath");
            animator.SetBool("isRun", false);
            animator.SetBool("isIdle", false);
        }

        // Destroy after animation plays (base.Die() would destroy immediately)
        Destroy(gameObject, 1.0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                lastDamageTime = Time.time;

                PlayerController playerScript = other.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(contactDamage);
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                lastDamageTime = Time.time;

                PlayerController playerScript = other.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(contactDamage);
                }
            }
        }
    }
}