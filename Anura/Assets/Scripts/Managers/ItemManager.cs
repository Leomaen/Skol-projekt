using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class WeaponModifier
{
    public WeaponModifierType modifierType;
}

public class ItemManager : MonoBehaviour
{
    public GameState gameState;
    public UserData userData;
    public static ItemManager Instance { get; private set; }

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

    private void OnDestroy()
    {
        // Unsubscribe from scene events when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}, checking for previously collected items");

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