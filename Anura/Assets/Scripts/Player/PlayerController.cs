using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public GameState gameState;

    private Rigidbody2D rb;
    private Vector2 movementDirection;
    public Transform firePoint;
    private float lastShotTime = 0f;
    private float lastFootstepTime = 0f;
    [SerializeField] private float footstepDelay = 0.3f;

    [SerializeField] private float firePointDistance = 0.5f;
    [SerializeField] private Weapon weapon;

    public static event Action OnPlayerDamaged;
    public bool isTransitioning = false;

    public HitEffect playerHitEffect; 

    private Animator animator; // Add this line

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Add this line
    }

    void Update()
    {
        HandleMovement();

        HandleFirePointRotation();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementDirection * gameState.stats.movementSpeed;
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (!isTransitioning)
        {
            if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
            if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
            if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
        }
        bool isMovingNow = (verticalInput != 0f || horizontalInput != 0f);

  if (isMovingNow && Time.time > lastFootstepTime + footstepDelay)
        {
            PlayFootstepSound();
            lastFootstepTime = Time.time;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", isMovingNow);

            if (isMovingNow)
            {
                if (verticalInput > 0f) // Moving predominantly upwards
                {
                    animator.SetBool("isWalkingUp", true);
                    animator.SetBool("isFacingUp", true); // Set facing direction for future idle/horizontal walk
                }
                else if (verticalInput < 0f) // Moving predominantly downwards
                {
                    animator.SetBool("isWalkingUp", false);
                    animator.SetBool("isFacingUp", false); // Set facing direction
                }
                else // Purely horizontal movement (verticalInput == 0f && horizontalInput != 0f)
                {
                    // Continue using the walk animation (WalkUp or Walk) based on the last facing direction
                    animator.SetBool("isWalkingUp", animator.GetBool("isFacingUp"));
                }
            }
            // When isMovingNow is false, isWalking is set to false.
            // isWalkingUp will retain its last state from movement.
            // isFacingUp also retains its last state, which will be used by the Animator
            // to transition to the correct idle state (isLookUp or isLookDown).
        }
        
        movementDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }

    private void PlayFootstepSound()
    {
        int randomSound = UnityEngine.Random.Range(0, 4);
        AudioManager.Instance.PlaySound($"PlayerWalk{randomSound}");
    }

    void HandleFirePointRotation()
    {

        bool canShoot = Time.time > lastShotTime + gameState.stats.atkSpeed;

        // Check arrow key input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            PositionAndRotateFirePoint(Vector2.up, 90f);
            if (canShoot)
            {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            PositionAndRotateFirePoint(Vector2.down, 270f);
            if (canShoot)
            {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            PositionAndRotateFirePoint(Vector2.left, 180f);
            if (canShoot)
            {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PositionAndRotateFirePoint(Vector2.right, 0f);
            if (canShoot)
            {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
    }

    void PositionAndRotateFirePoint(Vector2 direction, float angle)
    {
        if (firePoint != null)
        {
            // Set the position of the fire point
            firePoint.transform.position = transform.position + (Vector3)(direction * firePointDistance);

            // Set the rotation of the fire point
            firePoint.transform.rotation = Quaternion.Euler(0, 0, angle);

        }
    }

    public void TakeDamage(int amount)
    {
        gameState.stats.PlayerHealth -= amount;
        OnPlayerDamaged?.Invoke();
        if (playerHitEffect != null)
    {
        playerHitEffect.Play();
    }
    }
}