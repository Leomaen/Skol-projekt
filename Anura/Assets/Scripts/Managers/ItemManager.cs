using System.Collections.Generic;
using UnityEngine;
using System;

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
    
    // Event that fires when items change
    public event Action OnItemsChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddItem(Item item)
    {
        // Don't add null items
        if (item == null) return;
        
        activeItems.Add(item);
        item.ApplyEffect();
        OnItemsChanged?.Invoke();
        
        Debug.Log($"Added item: {item.itemName}");

        // Show notification for the picked-up item
        if (NotificationTitles.Instance != null)
        {
            // Assuming your Item class has a 'itemName' or similar public string property
            NotificationTitles.Instance.ShowNotification(item.itemName); 
        }
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
}