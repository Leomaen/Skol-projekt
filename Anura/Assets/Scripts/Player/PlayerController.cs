using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementDirection;
    public Transform firePoint;
    private float lastShotTime = 0f;

    [SerializeField] private float firePointDistance = 0.5f;
    [SerializeField] private Weapon weapon;

    public static event Action OnPlayerDamaged;
    

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
        rb.linearVelocity = movementDirection * StatsManager.Instance.movementSpeed;
    }
    
    void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
        
        movementDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }
    
    void HandleFirePointRotation()
    {

        bool canShoot = Time.time > lastShotTime + StatsManager.Instance.atkSpeed;

        // Check arrow key input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            PositionAndRotateFirePoint(Vector2.up, 90f);
                if(canShoot) {
                    weapon.Shoot();
                    lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            PositionAndRotateFirePoint(Vector2.down, 270f);
            if(canShoot) {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            PositionAndRotateFirePoint(Vector2.left, 180f);
            if(canShoot) {
                weapon.Shoot();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PositionAndRotateFirePoint(Vector2.right, 0f);
            if(canShoot) {
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
        StatsManager.Instance.PlayerHealth -= amount;
        OnPlayerDamaged?.Invoke();
    }

    // Add this new public method to expose the movement direction
    public Vector2 GetMovementDirection()
    {
        return movementDirection;
    }
}