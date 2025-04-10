using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Movement")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float waypointReachedDistance = 0.1f;
    public float idleTimeAtWaypoint = 1.5f;

    [Header("Attack Properties")]
    public float attackCooldown = 3f;
    public float attackChance = 0.7f; // Chance to attack when cooldown is ready
    public float rushSpeed = 8f;
    public float rushCooldown = 5f;
    public float rushDistance = 10f;
    public float laserChargeDuration = 1.5f;
    public float laserActiveDuration = 1.0f;
    public float laserCooldown = 4f;
    public GameObject laserPrefab;
    
    [Header("References")]
    public Transform player;
    public LayerMask wallLayer;
    
    // Animation parameters
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    
    // State tracking
    private int currentWaypointIndex = 0;
    private float lastAttackTime;
    private float lastRushTime;
    private float lastLaserTime;
    private bool isAttacking = false;
    private bool isMovingToWaypoint = true;
    private float idleTimer = 0f;
    
    // Cached
    private static readonly int IdleAnim = Animator.StringToHash("idle");
    private static readonly int WalkAnim = Animator.StringToHash("walk");
    private static readonly int RushAnim = Animator.StringToHash("rush");
    private static readonly int ChargeUpLaserAnim = Animator.StringToHash("chargeuplaser");
    private static readonly int ChargeDownLaserAnim = Animator.StringToHash("chargedownlaser");

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Initialize attack timers
        lastAttackTime = -attackCooldown;
        lastRushTime = -rushCooldown;
        lastLaserTime = -laserCooldown;
    }

    void Update()
    {
        if (player == null) return;

        // Face the player
        spriteRenderer.flipX = player.position.x < transform.position.x;
        
        // Don't process movement or attacks if currently attacking
        if (isAttacking)
            return;
        
        // Check if we can attack
        if (Time.time - lastAttackTime >= attackCooldown && Random.value <= attackChance)
        {
            // Choose an attack
            if (Time.time - lastRushTime >= rushCooldown && 
                Time.time - lastLaserTime >= laserCooldown)
            {
                // Both attacks available, random choice
                if (Random.value > 0.5f)
                    StartCoroutine(RushAttack());
                else
                    StartCoroutine(LaserAttack());
            }
            else if (Time.time - lastRushTime >= rushCooldown)
            {
                StartCoroutine(RushAttack());
            }
            else if (Time.time - lastLaserTime >= laserCooldown)
            {
                StartCoroutine(LaserAttack());
            }
        }
        
        // Handle movement between waypoints when not attacking
        HandleMovement();
    }
    
    void HandleMovement()
    {
        if (waypoints.Length == 0) return;
        
        if (isMovingToWaypoint)
        {
            // Move toward current waypoint
            Vector2 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            
            // Play walk animation
            animator.Play(WalkAnim);
            
            // Check if we reached the waypoint
            if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < waypointReachedDistance)
            {
                // Stop and idle at waypoint
                rb.linearVelocity = Vector2.zero;
                isMovingToWaypoint = false;
                idleTimer = idleTimeAtWaypoint;
                
                // Play idle animation
                animator.Play(IdleAnim);
            }
        }
        else
        {
            // Wait at waypoint
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                // Move to next waypoint
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                isMovingToWaypoint = true;
            }
        }
    }
    
    IEnumerator RushAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        lastRushTime = Time.time;
        
        // Stop movement
        rb.linearVelocity = Vector2.zero;
        
        // Play rush animation
        animator.Play(RushAnim);
        
        // Small delay before rushing
        yield return new WaitForSeconds(0.5f);
        
        // Calculate rush direction toward player
        Vector2 rushDirection = (player.position - transform.position).normalized;
        
        // Check for wall collisions in rush direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rushDirection, rushDistance, wallLayer);
        float actualRushDistance = hit.collider != null ? hit.distance * 0.9f : rushDistance;
        
        // Perform the rush
        float rushDuration = actualRushDistance / rushSpeed;
        float rushTimer = 0;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (rushDirection * actualRushDistance);
        
        while (rushTimer < rushDuration)
        {
            rushTimer += Time.deltaTime;
            float t = rushTimer / rushDuration;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));
            yield return null;
        }
        
        // Reset after attack
        rb.linearVelocity = Vector2.zero;
        animator.Play(IdleAnim);
        
        // Cooldown after attack
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    IEnumerator LaserAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        lastLaserTime = Time.time;
        
        // Stop movement
        rb.linearVelocity = Vector2.zero;
        
        // Get direction to player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // Charge up laser animation
        animator.Play(ChargeUpLaserAnim);
        yield return new WaitForSeconds(laserChargeDuration);
        
        // Spawn and shoot laser
        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            laser.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            Laser laserScript = laser.GetComponent<Laser>();
            if (laserScript != null)
            {
                laserScript.direction = directionToPlayer;
                laserScript.duration = laserActiveDuration;
            }
            else
            {
                Destroy(laser, laserActiveDuration);
            }
        }
        
        // Charge down laser animation
        animator.Play(ChargeDownLaserAnim);
        yield return new WaitForSeconds(laserActiveDuration);
        
        // Reset after attack
        animator.Play(IdleAnim);
        
        // Cooldown after attack
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
    }
    
    void OnDrawGizmos()
    {
        // Visualize waypoints
        if (waypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                    
                    // Draw lines connecting waypoints
                    if (i < waypoints.Length - 1 && waypoints[i+1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    }
                    else if (i == waypoints.Length - 1 && waypoints[0] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }
    }
}
