using UnityEngine;

public class Bullet : MonoBehaviour
{

    public Rigidbody2D rb;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.linearVelocity = transform.right * StatsManager.Instance.bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.gameObject.CompareTag("Wall"))
    {
        animator.SetTrigger("hitWall");

        Destroy(gameObject, 0.5f);

        rb.linearVelocity = transform.right * 0;
    }

    if (collision.gameObject.CompareTag("Enemy"))
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null) 
        {
            enemy.takeDamage();
            Destroy(gameObject);
        }

    }
}

}
