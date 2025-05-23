using UnityEngine;
using System.Collections.Generic; // Required for using Lists

// Define a class to hold item data including its spawn chance (weight)
[System.Serializable] // This makes it show up in the Inspector
public class ItemToSpawn
{
    public GameObject itemPrefab; // The actual item prefab to spawn
    public float spawnWeight = 1.0f; // Higher weight means higher chance of spawning
    [Tooltip("Optional: A specific offset from the spawner's position for this item.")]
    public Vector3 spawnOffset = Vector3.zero;
}

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Pool")]
    [Tooltip("List of items that can be spawned by this spawner.")]
    public List<ItemToSpawn> itemPool = new List<ItemToSpawn>();

    [Header("Spawn Settings")]
    [Tooltip("If true, an item will be spawned automatically when this spawner starts.")]
    public bool spawnOnStart = true;
    [Tooltip("If true, only one item will ever be spawned by this spawner.")]
    public bool spawnOnlyOnce = true;

    private bool hasSpawned = false;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnItem();
        }
    }

    // Public method to allow external scripts to trigger spawning
    public GameObject SpawnItem()
    {
        if (spawnOnlyOnce && hasSpawned)
        {
            Debug.Log($"ItemSpawner ({gameObject.name}): Already spawned an item and spawnOnlyOnce is true. Skipping.");
            return null; // Already spawned and only allowed once
        }

        if (itemPool == null || itemPool.Count == 0)
        {
            Debug.LogWarning($"ItemSpawner ({gameObject.name}): Item pool is empty. Cannot spawn item.");
            return null;
        }

        ItemToSpawn selectedItem = SelectItemFromPool();

        if (selectedItem != null && selectedItem.itemPrefab != null)
        {
            Vector3 spawnPosition = transform.position + selectedItem.spawnOffset;
            GameObject spawnedItemInstance = Instantiate(selectedItem.itemPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"ItemSpawner ({gameObject.name}): Spawned item '{selectedItem.itemPrefab.name}' at {spawnPosition}.");
            hasSpawned = true;
            return spawnedItemInstance;
        }
        else
        {
            Debug.LogWarning($"ItemSpawner ({gameObject.name}): Selected item or its prefab was null. Cannot spawn item.");
            return null;
        }
    }

    private ItemToSpawn SelectItemFromPool()
    {
        float totalWeight = 0f;
        foreach (ItemToSpawn item in itemPool)
        {
            if (item.itemPrefab != null) // Only consider items with prefabs assigned
            {
                totalWeight += item.spawnWeight;
            }
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning($"ItemSpawner ({gameObject.name}): Total spawn weight is 0 or less. Cannot select an item.");
            // Optionally, pick the first valid item if weights are all zero
            foreach (ItemToSpawn item in itemPool) { if (item.itemPrefab != null) return item; }
            return null;
        }

        float randomNumber = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (ItemToSpawn item in itemPool)
        {
            if (item.itemPrefab == null) continue; // Skip items without prefabs

            cumulativeWeight += item.spawnWeight;
            if (randomNumber <= cumulativeWeight)
            {
                return item;
            }
        }

        // Fallback, should not be reached if totalWeight > 0 and items exist
        // but as a safety, return the last valid item or null
        for (int i = itemPool.Count - 1; i >= 0; i--)
        {
            if (itemPool[i].itemPrefab != null) return itemPool[i];
        }
        return null;
    }

    // Optional: Gizmo to show the spawn point
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, "Item Spawner");
        if (itemPool != null && itemPool.Count > 0)
        {
            float yOffset = -0.5f;
            foreach(var item in itemPool)
            {
                if (item.itemPrefab != null)
                {
                    UnityEditor.Handles.Label(transform.position + new Vector3(0.6f, yOffset, 0), $"{item.itemPrefab.name} (W: {item.spawnWeight})");
                    yOffset -= 0.3f;
                }
            }
        }
        #endif
    }
}