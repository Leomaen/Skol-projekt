using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private GameObject pickupEffect;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    
    private void Start()
    {
        // Get or add a sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Use the item's icon as the sprite
        if (item != null)
        {
            spriteRenderer.sprite = item.icon;
        }
        
        // Store initial position for bobbing effect
        startPosition = transform.position;
    }
    
    private void Update()
    {
        // Spinning effect
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
        
        // Bobbing effect
        Vector3 position = startPosition;
        position.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = position;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }
    
    private void PickUp()
    {
        Debug.Log($"Picking up item: {item.itemName}");
        
        // Add the item to the player's inventory
        ItemManager.Instance.AddItem(item);
        
        // Play pickup effects
        PlayPickupEffect();
        
        // Destroy the pickup game object
        Destroy(gameObject);
    }
    
    private void PlayPickupEffect()
    {
        // Spawn visual effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        // Play sound effect (if you have an audio manager)
        // AudioManager.Instance.PlaySound("ItemPickup");
    }
}