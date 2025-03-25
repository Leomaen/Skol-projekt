using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementDirection;
    [SerializeField] private GameObject firePoint;
    [SerializeField] private float firePointDistance = 0.5f; // Distance from center of player to firePoint
    private Vector2 lastShootDirection = Vector2.right; // Default direction

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
        
        // WASD for movement
        if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
        
        movementDirection = new Vector2(horizontalInput, verticalInput).normalized;
    }
    
    void HandleFirePointRotation()
    {
        // Check arrow key input
        if (Input.GetKey(KeyCode.UpArrow))
        {
            PositionAndRotateFirePoint(Vector2.up, 90f);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            PositionAndRotateFirePoint(Vector2.down, 270f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            PositionAndRotateFirePoint(Vector2.left, 180f);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PositionAndRotateFirePoint(Vector2.right, 0f);
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
            
            // Store the last shooting direction
            lastShootDirection = direction;
        }
    }
}