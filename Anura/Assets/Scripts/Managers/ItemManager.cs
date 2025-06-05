using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[Serializable]
public class ItemPool
{
    public string poolName;
    public List<ItemWeight> items = new List<ItemWeight>();
}

[Serializable]
public class ItemWeight
{
    public Item item;
    public float weight = 1f;
}

[Serializable]
public class WeaponModifier
{
    public WeaponModifierType modifierType;
}

public class ItemManager : MonoBehaviour
{
    public GameState gameState;
    public UserData userData;
    public static ItemManager Instance { get; private set; }

    // Item pools with weights
    [SerializeField] private List<ItemPool> itemPools = new List<ItemPool>();
    [SerializeField] private ItemPool defaultPool;
    
    // For deterministic item generation
    private System.Random seedRandom;

    // Event that fires when items change
    public event Action OnItemsChanged;

    // Event that fires when scene is starting - for item pickups to check if they should appear
    public static event Action<string> OnCheckItemsInScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Subscribe to scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeSeedRandom();
    }

    private void InitializeSeedRandom()
    {
        if (gameState == null || gameState.world == null)
        {
            Debug.LogError("GameState or GameState.world is null, cannot initialize seed!");
            seedRandom = new System.Random(UnityEngine.Random.Range(1, 1000000));
            return;
        }

        // Use the same seed as the room generation but with a different offset
        // This ensures different but deterministic sequences
        int itemSeed = gameState.world.seed + ((gameState.world.floor - 1) * 7919) + 1337;
        seedRandom = new System.Random(itemSeed);
        Debug.Log($"Initialized item seed: {itemSeed}");
    }

    // Get random item from a specific pool
    public Item GetRandomItem(string poolName = "")
    {
        // Re-initialize the random generator if needed
        if (seedRandom == null)
        {
            InitializeSeedRandom();
        }

        ItemPool pool = string.IsNullOrEmpty(poolName) ? 
            defaultPool : 
            itemPools.Find(p => p.poolName == poolName);

        if (pool == null || pool.items.Count == 0)
        {
            Debug.LogWarning($"Item pool '{poolName}' not found or empty, using default pool");
            pool = defaultPool;
        }

        if (pool == null || pool.items.Count == 0)
        {
            Debug.LogError("No valid item pool available!");
            return null;
        }

        // Calculate total weight
        float totalWeight = 0;
        foreach (var itemWeight in pool.items)
        {
            totalWeight += itemWeight.weight;
        }

        // Get a random value between 0 and the total weight
        float randomValue = (float)(seedRandom.NextDouble() * totalWeight);
        
        // Find the item that corresponds to the random value
        float currentWeight = 0;
        foreach (var itemWeight in pool.items)
        {
            currentWeight += itemWeight.weight;
            if (randomValue <= currentWeight)
            {
                // Create a new instance of the item to avoid shared references
                return Instantiate(itemWeight.item);
            }
        }

        // Fallback in case of rounding errors
        return Instantiate(pool.items[pool.items.Count - 1].item);
    }

    // Helper method to instantiate an item while keeping its scriptable object type
    private Item Instantiate(Item original)
    {
        if (original == null) return null;
        return UnityEngine.Object.Instantiate(original);
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene events when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}, checking for previously collected items");

        // Reinitialize seed random when loading a new scene
        InitializeSeedRandom();

        // Allow item pickups to check if they should appear
        if (OnCheckItemsInScene != null)
        {
            OnCheckItemsInScene.Invoke(scene.name);
        }

        // Force item refreshes when a new scene loads
        if (scene.name == "SampleScene")
        {
            InvokeItemChangeEvent();
        }
    }

    // Helper method to safely invoke the OnItemsChanged event
    private void InvokeItemChangeEvent()
    {
        if (OnItemsChanged != null)
        {
            try
            {
                OnItemsChanged.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in OnItemsChanged event: {e.Message}");
            }
        }
    }

    // Existing methods remain unchanged below...
    

    public void AddItem(Item item)
    {
        // Don't add null items
        if (item == null) return;

        // Don't check for duplicates - each item should be instantiated uniquely
        gameState.activeItems.Add(item);

        try
        {
            item.ApplyEffect();
            Debug.Log($"Applied effect of item: {item.itemName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error applying item effect: {e.Message}");
        }

        userData.stats.totalItemsCollected++;
        userData.Save();
        AudioManager.Instance.PlaySound("ItemPickup");
        Debug.Log($"Added item: {item.itemName}");

        // Show notification for the picked-up item
        if (NotificationTitles.Instance != null)
        {
            NotificationTitles.Instance.ShowNotification(item.itemName);
        }

        // Notify listeners after item is fully added
        InvokeItemChangeEvent();
    }

    public void RemoveItem(Item item)
    {
        if (gameState.activeItems.Contains(item))
        {
            gameState.activeItems.Remove(item);
            item.RemoveEffect();
            OnItemsChanged?.Invoke();

            Debug.Log($"Removed item: {item.itemName}");
        }
    }

    public void AddWeaponModifier(WeaponModifier modifier)
    {
        userData.stats.totalItemsCollected++;
        userData.Save();
        gameState.activeWeaponModifiers.Add(modifier);
        AudioManager.Instance.PlaySound("ItemPickup");
        OnItemsChanged?.Invoke();
    }

    public void RemoveWeaponModifier(WeaponModifierType modifierType)
    {
        WeaponModifier modifierToRemove = gameState.activeWeaponModifiers.Find(m => m.modifierType == modifierType);
        if (modifierToRemove != null)
        {
            gameState.activeWeaponModifiers.Remove(modifierToRemove);
            OnItemsChanged?.Invoke();
        }
    }

    public bool HasWeaponModifier(WeaponModifierType modifierType)
    {
        return gameState.activeWeaponModifiers.Exists(m => m.modifierType == modifierType);
    }

    // Get all active items for UI display
    public List<Item> GetActiveItems()
    {
        return gameState.activeItems;
    }

    // Method to register an item as permanently collected
    public void RegisterItemPickup(string uniqueItemId)
    {
        if (!string.IsNullOrEmpty(uniqueItemId))
        {
            gameState.collectedItemIds.Add(uniqueItemId);
            Debug.Log($"Item registered as picked up: {uniqueItemId}");
        }
    }

    // Method to check if an item has been collected already
    public bool HasItemBeenCollected(string uniqueItemId)
    {
        return !string.IsNullOrEmpty(uniqueItemId) && gameState.collectedItemIds.Contains(uniqueItemId);
    }

    // For debugging or save/load functionality
    public string[] GetAllCollectedItemIds()
    {
        string[] result = new string[gameState.collectedItemIds.Count];
        gameState.collectedItemIds.CopyTo(result);
        return result;
    }
}