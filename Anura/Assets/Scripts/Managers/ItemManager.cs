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
    public static ItemManager Instance { get; private set; }
    
    // Lists to track active items and modifiers
    [SerializeField] private List<Item> activeItems = new List<Item>();
    [SerializeField] private List<WeaponModifier> activeWeaponModifiers = new List<WeaponModifier>();
    
    // List to track which items have been permanently collected (by unique ID)
    private HashSet<string> collectedItemIds = new HashSet<string>();
    
    // Event that fires when items change
    public event Action OnItemsChanged;
    
    // Event that fires when scene is starting - for item pickups to check if they should appear
    public static event Action<string> OnCheckItemsInScene;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
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
        InvokeItemChangeEvent();
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
        activeItems.Add(item);
        
        try
        {
            item.ApplyEffect();
            Debug.Log($"Applied effect of item: {item.itemName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error applying item effect: {e.Message}");
        }
        
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
        if (activeItems.Contains(item))
        {
            activeItems.Remove(item);
            item.RemoveEffect();
            OnItemsChanged?.Invoke();
            
            Debug.Log($"Removed item: {item.itemName}");
        }
    }
    
    public void AddWeaponModifier(WeaponModifier modifier)
    {
        activeWeaponModifiers.Add(modifier);
        OnItemsChanged?.Invoke();
    }
    
    public void RemoveWeaponModifier(WeaponModifierType modifierType)
    {
        WeaponModifier modifierToRemove = activeWeaponModifiers.Find(m => m.modifierType == modifierType);
        if (modifierToRemove != null)
        {
            activeWeaponModifiers.Remove(modifierToRemove);
            OnItemsChanged?.Invoke();
        }
    }
    
    public bool HasWeaponModifier(WeaponModifierType modifierType)
    {
        return activeWeaponModifiers.Exists(m => m.modifierType == modifierType);
    }
    
    // Get all active items for UI display
    public List<Item> GetActiveItems()
    {
        return activeItems;
    }
    
    // Method to register an item as permanently collected
    public void RegisterItemPickup(string uniqueItemId)
    {
        if (!string.IsNullOrEmpty(uniqueItemId))
        {
            collectedItemIds.Add(uniqueItemId);
            Debug.Log($"Item registered as picked up: {uniqueItemId}");
        }
    }
    
    // Method to check if an item has been collected already
    public bool HasItemBeenCollected(string uniqueItemId)
    {
        return !string.IsNullOrEmpty(uniqueItemId) && collectedItemIds.Contains(uniqueItemId);
    }
    
    // For debugging or save/load functionality
    public string[] GetAllCollectedItemIds()
    {
        string[] result = new string[collectedItemIds.Count];
        collectedItemIds.CopyTo(result);
        return result;
    }
}