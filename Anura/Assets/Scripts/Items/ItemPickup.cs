using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private float spinSpeed = 100f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private string customItemId = ""; // Optional custom ID if needed

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool isPickedUp = false;
    private string uniqueItemId;

    // Keep track of this item instance in a static list
    private static List<GameObject> activePickups = new List<GameObject>();

    private void Awake()
    {
        // Generate a unique ID for this item pickup based on position and scene
        uniqueItemId = GenerateUniqueId();
    }

    private void OnEnable()
    {
        // Register this pickup
        if (!activePickups.Contains(gameObject))
            activePickups.Add(gameObject);

        // Subscribe to the scene check event
        ItemManager.OnCheckItemsInScene += CheckIfShouldExist;

        // Immediately check if this item should exist
        if (ItemManager.Instance != null)
        {
            CheckIfShouldExist(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDisable()
    {
        // Remove from registry when disabled
        activePickups.Remove(gameObject);

        // Unsubscribe from events
        ItemManager.OnCheckItemsInScene -= CheckIfShouldExist;
    }

    // Check if this item has already been picked up and should be destroyed
    private void CheckIfShouldExist(string currentSceneName)
    {
        if (ItemManager.Instance != null && ItemManager.Instance.HasItemBeenCollected(uniqueItemId))
        {
            Debug.Log($"Item {uniqueItemId} has already been collected - destroying");
            Destroy(gameObject);
        }
    }

    // Generate a unique ID for this item based on scene, position, and item name
    private string GenerateUniqueId()
    {
        // Use custom ID if provided
        if (!string.IsNullOrEmpty(customItemId))
            return customItemId;

        // Otherwise generate an ID based on scene, position and item name
        string sceneName = SceneManager.GetActiveScene().name;
        Vector3 posRounded = new Vector3(
            Mathf.Round(transform.position.x * 10f) / 10f,
            Mathf.Round(transform.position.y * 10f) / 10f,
            Mathf.Round(transform.position.z * 10f) / 10f
        );

        // Maybe we shouldn't use the floor here? idk let's see if we wanna change it later
        int floor = ItemManager.Instance.gameState.world.floor;
        string itemName = item != null ? item.itemName : "unknown";
        return $"{sceneName}_{floor}_{posRounded.x}_{posRounded.y}_{posRounded.z}_{itemName}";
    }

    // Static method to clear all active pickups - call on scene transitions
    public static void CleanupAllPickups()
    {
        foreach (var pickup in new List<GameObject>(activePickups))
        {
            if (pickup != null)
            {
                // Force immediate destruction
                DestroyImmediate(pickup);
            }
        }
        activePickups.Clear();
    }

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
        // Don't update if already picked up
        if (isPickedUp)
            return;

        // Spinning effect
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

        // Bobbing effect
        Vector3 position = startPosition;
        position.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp || item == null)
            return;

        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (isPickedUp)
            return;

        isPickedUp = true;
        Debug.Log($"Picking up item: {item.itemName} with ID: {uniqueItemId}");

        // Add the item to the player's inventory
        if (ItemManager.Instance != null)
        {
            // Register this item as picked up permanently
            ItemManager.Instance.RegisterItemPickup(uniqueItemId);

            // Create a new instance of the scriptable object to avoid sharing references
            Item itemInstance = Instantiate(item);
            ItemManager.Instance.AddItem(itemInstance);

            // Play pickup effects
            PlayPickupEffect();

            // Begin thorough destruction process
            StartCoroutine(DestroyCompletely());
        }
    }

    private IEnumerator DestroyCompletely()
    {
        // Disable all components that could affect gameplay
        DisableAllComponents();

        // Hide visually
        gameObject.SetActive(false);

        // Remove from scene hierarchy immediately
        transform.parent = null;

        // Wait one frame to ensure everything is processed
        yield return null;

        // Final destruction
        Destroy(gameObject);

        // Double-check destruction after a delay
        yield return new WaitForSeconds(0.5f);

        // Make sure we're removed from the tracking list
        if (activePickups.Contains(gameObject))
            activePickups.Remove(gameObject);
    }

    private void DisableAllComponents()
    {
        // Disable all colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Disable other components that could cause persistence
        var behaviours = GetComponents<MonoBehaviour>();
        foreach (var behaviour in behaviours)
        {
            if (behaviour != this) // Don't disable this script yet
                behaviour.enabled = false;
        }
    }

    private void PlayPickupEffect()
    {
        // Spawn visual effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            // Destroy effect after 2 seconds to prevent clutter
            Destroy(effect, 2f);
        }
    }
}