using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Get input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        
        // Create direction vector and normalize for consistent diagonal speed
        moveDirection = new Vector2(moveX, moveY).normalized;
        
        // Prevent sprite flipping
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
        }
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate for consistent physics
        rb.linearVelocity = moveDirection * moveSpeed;
    }
}