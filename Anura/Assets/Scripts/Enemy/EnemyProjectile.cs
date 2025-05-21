using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifetime = 3f; // Time before the projectile destroys itself if it hits nothing
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("EnemyProjectile requires a Rigidbody2D component!");
            Destroy(gameObject);
            return;
        }
        // Projectiles usually move along their local 'right' or 'up' axis
        // The goopsterEnemy already rotates the projectile to face the player
        rb.linearVelocity = transform.right * speed; 
        Destroy(gameObject, lifetime); // Self-destruct after a certain time
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage); // Assuming PlayerController has a TakeDamage method
            }
            Destroy(gameObject); // Destroy projectile on hitting the player
        }
        else if (collision.gameObject.CompareTag("Wall")) // Or use a layer mask
        {
            // Optionally, play a hit effect/sound for walls
            Destroy(gameObject); // Destroy projectile on hitting a wall
        }
        // Add other collision checks if needed (e.g., other enemies, environment)
    }
}