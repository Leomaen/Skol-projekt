using UnityEngine;

public class Laser : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;
    public Vector2 direction;
    public float duration = 1.0f;
    public float width = 0.5f;
    public float maxDistance = 30f;
    
    private LineRenderer lineRenderer;
    private float timer = 0f;
    
    void Start()
    {
        // Set up line renderer for the laser visual
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.yellow;
        
        // Set initial positions
        lineRenderer.SetPosition(0, transform.position);
        
        // Cast ray to find where laser should end (wall collision or max distance)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, LayerMask.GetMask("Wall"));
        Vector2 endPoint = hit.collider != null ? hit.point : (Vector2)transform.position + direction * maxDistance;
        lineRenderer.SetPosition(1, endPoint);
        
        // Add a collider for the laser
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        Vector2 midPoint = ((Vector2)transform.position + endPoint) / 2f;
        float distance = Vector2.Distance(transform.position, endPoint);
        
        collider.isTrigger = true;
        collider.offset = new Vector2(distance / 2f, 0);
        collider.size = new Vector2(distance, width);
        
        // Rotate the collider to match the laser direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void Update()
    {
        // Track lifetime and destroy when duration is reached
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider is the player
        if (other.CompareTag("Player"))
        {
            // Try to get player health component and damage it
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}