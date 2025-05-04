using UnityEngine;
using System;
using System.Collections;

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
                ShootWithModifiers();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            PositionAndRotateFirePoint(Vector2.down, 270f);
            if(canShoot) {
                ShootWithModifiers();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            PositionAndRotateFirePoint(Vector2.left, 180f);
            if(canShoot) {
                ShootWithModifiers();
                lastShotTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PositionAndRotateFirePoint(Vector2.right, 0f);
            if(canShoot) {
                ShootWithModifiers();
                lastShotTime = Time.time;
            }
        }
    }
    
    void ShootWithModifiers()
    {
        // Default behavior - shoot a single bullet
        weapon.Shoot();
        
        // Check for weapon modifiers if ItemManager exists
        if (ItemManager.Instance != null)
        {
            // Double shot modifier
            if (HasWeaponModifier(WeaponModifierType.DoubleShot))
            {
                // Shoot a second bullet with a small delay
                StartCoroutine(DelayedShot(0.1f));
            }
            
            // Spread shot modifier
            if (HasWeaponModifier(WeaponModifierType.Spread))
            {
                // Shoot two additional bullets at an angle
                ShootAtAngle(15);
                ShootAtAngle(-15);
            }
        }
    }
    
    // Helper method to check if a weapon modifier is active
    bool HasWeaponModifier(WeaponModifierType modifierType)
    {
        if (ItemManager.Instance == null) return false;
        
        foreach (var modifier in ItemManager.Instance.activeWeaponModifiers)
        {
            if (modifier.modifierType == modifierType)
                return true;
        }
        return false;
    }
    
    IEnumerator DelayedShot(float delay)
    {
        yield return new WaitForSeconds(delay);
        weapon.Shoot();
    }
    
    void ShootAtAngle(float angleOffset)
    {
        if (firePoint != null)
        {
            // Save original rotation
            Quaternion originalRotation = firePoint.rotation;
            
            // Apply angle offset
            firePoint.Rotate(0, 0, angleOffset);
            
            // Shoot
            weapon.Shoot();
            
            // Restore original rotation
            firePoint.rotation = originalRotation;
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
}