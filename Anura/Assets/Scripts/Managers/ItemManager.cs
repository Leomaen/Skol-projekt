using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponModifier
{
    public WeaponModifierType modifierType;
}

public class ItemManager : MonoBehaviour
{
    public GameState gameState;
    public static ItemManager Instance { get; private set; }
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

        gameState.activeItems.Add(item);
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
        gameState.activeWeaponModifiers.Add(modifier);
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
}