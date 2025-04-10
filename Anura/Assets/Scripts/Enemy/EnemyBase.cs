using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] protected float pathUpdateTime = 1.5f;
    [SerializeField] protected float detectionRange = 8.0f;
    [SerializeField] protected float minDistanceToPlayer = 1.0f;
    
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int currentHealth;
    
    protected Transform playerTransform;
    protected Rigidbody2D rb;
    protected Vector2 currentMoveDirection;
    protected bool isChasing = false;
    protected float lastPathUpdateTime = 0f;
    
    // Animation support
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // Find player at start
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        
        // Start the path updating process
        StartCoroutine(UpdatePathRoutine());
    }

    protected virtual void Update()
    {
        // Check if player is found and within range
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            isChasing = distanceToPlayer <= detectionRange;
            
            // Handle animations if we have an animator
            if (animator != null)
            {
                animator.SetBool("IsMoving", rb.linearVelocity.sqrMagnitude > 0.1f);
                if (rb.linearVelocity.x != 0)
                {
                    spriteRenderer.flipX = rb.linearVelocity.x < 0;
                }
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isChasing)
        {
            // Move in the current direction
            rb.linearVelocity = currentMoveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected IEnumerator UpdatePathRoutine()
    {
        while (true)
        {
            // Wait for the specified time
            yield return new WaitForSeconds(pathUpdateTime);
            
            if (isChasing && playerTransform != null)
            {
                ChooseNewPath();
            }
        }
    }

    protected virtual void ChooseNewPath()
    {
        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Add some randomness for more natural movement
        float randomAngle = Random.Range(-30f, 30f);
        currentMoveDirection = RotateVector(directionToPlayer, randomAngle);
        
        // If we're too close to the player, back off a bit
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < minDistanceToPlayer)
        {
            currentMoveDirection = -directionToPlayer; // Move away from player
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Play hit animation/effect
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }

    protected virtual void Die()
    {
        // Play death animation/effect
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Add particle effects, sound, etc. here
        
        // Disable enemy components
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // Destroy after animation would complete
        Destroy(gameObject, 1.0f);
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

    // Draw gizmos to visualize the detection range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);
    }
}
