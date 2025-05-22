using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public GameState gameState;

    private Rigidbody2D rb;
    private Vector2 movementDirection;
    public Transform firePoint;
    private float lastShotTime = 0f;
    private bool isWalking = false;
        private float lastFootstepTime = 0f;
    [SerializeField] private float footstepDelay = 0.3f;

    [SerializeField] private float firePointDistance = 0.5f;
    [SerializeField] private Weapon weapon;

    public static event Action OnPlayerDamaged;
    public bool isTransitioning = false;

    public HitEffect playerHitEffect; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

        // Update walking state
        isWalking = isMovingNow;
        
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