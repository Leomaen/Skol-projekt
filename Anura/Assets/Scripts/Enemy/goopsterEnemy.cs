using UnityEngine;
using System.Collections;

public class goopsterEnemy : Enemy // Inherit from Enemy
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float directionChangeInterval = 3f; // How often to pick a new random direction
    private float lastDirectionChangeTime;
    private Vector2 moveDirection;
    private bool isMoving = false;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootInterval = 2.5f;
    public float shootDetectionRange = 7f; // How close the player needs to be to start shooting
    private float lastShootTime;
    private Vector3 originalFirePointLocalPosition;
    private Quaternion originalFirePointLocalRotation;


    [Header("Damage Visuals")]
    public float flashDuration = 0.1f;
    public Color damageFlashColor = Color.yellow;
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null)
        {
            Debug.LogError("GoopsterEnemy requires a Collider2D for wall detection.", this);
        }

        if (firePoint != null)
        {
            originalFirePointLocalPosition = firePoint.localPosition;
            originalFirePointLocalRotation = firePoint.localRotation;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player not found in scene for goopsterEnemy: " + gameObject.name);
        }

        lastShootTime = Time.time;
        if (gameState == null)
        {
            Debug.LogError("GameState not assigned to goopsterEnemy: " + gameObject.name + ". Assign it in the Inspector.");
        }

        // Ensure initial direction is set up for the first HandleMovement call
        lastDirectionChangeTime = -directionChangeInterval - 1f; // Force direction change on first update
        isMoving = false; // Start idle
        moveDirection = Vector2.zero; // Will be set by AttemptToSetNewMoveDirection
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleShooting();
        UpdateFacingDirection();
    }

    // Renamed to reflect its purpose better
    void AttemptToSetNewMoveDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
        lastDirectionChangeTime = Time.time;
    }

    void HandleMovement()
    {
        bool decidedToMoveThisFrame = false;

        if (Time.time - lastDirectionChangeTime > directionChangeInterval || moveDirection == Vector2.zero)
        {
            AttemptToSetNewMoveDirection();
        }

        if (rb != null && enemyCollider != null)
        {
            float colliderRadius = Mathf.Max(enemyCollider.bounds.extents.x, enemyCollider.bounds.extents.y);
            // Check slightly further than the collider's edge to anticipate collision
            float checkDistance = colliderRadius + moveSpeed * Time.deltaTime;
            int attempts = 0;
            const int maxAttempts = 8; // Try a few times to find a clear path

            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, checkDistance, wallLayer);

            while (hit.collider != null && attempts < maxAttempts)
            {
                AttemptToSetNewMoveDirection(); // Sets a new moveDirection and resets lastDirectionChangeTime
                hit = Physics2D.Raycast(transform.position, moveDirection, checkDistance, wallLayer);
                attempts++;
            }

            if (hit.collider == null) // Found a clear path
            {
                rb.linearVelocity = moveDirection * moveSpeed;
                decidedToMoveThisFrame = true;
            }
            else // Still couldn't find a clear path after attempts
            {
                rb.linearVelocity = Vector2.zero;
                decidedToMoveThisFrame = false;
                // Ensure lastDirectionChangeTime is reset so it tries again soon if stuck
                lastDirectionChangeTime = Time.time - directionChangeInterval + 0.5f; // Try again fairly soon
            }
        }
        else if (rb != null) // Collider is null, or rb is null (though rb should exist)
        {
            rb.linearVelocity = Vector2.zero;
            decidedToMoveThisFrame = false;
        }

        isMoving = decidedToMoveThisFrame;

        if (animator != null && !isDead)
        {
            animator.SetBool("isWalk", isMoving);
            animator.SetBool("isIdle", !isMoving);
        }
    }

    void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        bool shouldFlip = false;
        if (player != null && Vector2.Distance(transform.position, player.position) <= shootDetectionRange)
        {
            shouldFlip = transform.position.x > player.position.x;
        }
        else if (isMoving && rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f) // Check actual velocity
        {
            shouldFlip = rb.linearVelocity.x < 0;
        }

        spriteRenderer.flipX = shouldFlip;

        if (firePoint != null)
        {
            if (shouldFlip)
            {
                firePoint.localPosition = new Vector3(-originalFirePointLocalPosition.x, originalFirePointLocalPosition.y, originalFirePointLocalPosition.z);
                firePoint.localRotation = originalFirePointLocalRotation * Quaternion.Euler(0, 180, 0);
            }
            else
            {
                firePoint.localPosition = originalFirePointLocalPosition;
                firePoint.localRotation = originalFirePointLocalRotation;
            }
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            // Forcefully stop and attempt to find a new direction in the next HandleMovement update
            rb.linearVelocity = Vector2.zero;
            isMoving = false; // Update state
            AttemptToSetNewMoveDirection(); // Get a new direction candidate
            // The next HandleMovement call will validate this new direction with raycasts.
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                // playerScript.TakeDamage(contactDamage); // Add contactDamage field if needed
            }
        }
    }

    // ... (HandleShooting, Shoot, takeDamage, DamageFlashEffect, Die methods remain the same) ...

    void HandleShooting()
    {
        if (player == null || projectilePrefab == null || firePoint == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= shootDetectionRange && Time.time - lastShootTime >= shootInterval)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        if (player == null || firePoint == null || projectilePrefab == null) return;
        AudioManager.Instance.PlaySound("GoopsterShoot");

        Vector2 directionToPlayer = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion projectileRotation = Quaternion.Euler(0f, 0f, angle);

        Instantiate(projectilePrefab, firePoint.position, projectileRotation);
    }

    public override void takeDamage()
    {
        if (isDead) return;
        AudioManager.Instance.PlaySound("GoopsterHurt");
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(DamageFlashEffect());
        }

        base.takeDamage();
    }

    private IEnumerator DamageFlashEffect()
    {
        isFlashing = true;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        isFlashing = false;
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        AudioManager.Instance.PlaySound("GoopsterDeath");
        userData.stats.totalKills++;
        userData.Save();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isIdle", false);
            animator.SetTrigger("isDeath");
        }

        float deathAnimationLength = 1f;
        if (animator != null)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            foreach (AnimationClip clip in ac.animationClips)
            {
                if (clip.name.ToLower().Contains("death"))
                {
                    deathAnimationLength = clip.length;
                    break;
                }
            }
        }
        Destroy(gameObject, deathAnimationLength);
    }
}