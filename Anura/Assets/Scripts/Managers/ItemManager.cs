using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    
    // Lists to track different active items by type
    public List<Item> activeItems = new List<Item>();
    public List<StatModifier> activeStatModifiers = new List<StatModifier>();
    public List<AbilityItem> activeAbilities = new List<AbilityItem>();
    public List<WeaponModifier> activeWeaponModifiers = new List<WeaponModifier>();
    
    // Event that gets triggered when items change
    public delegate void ItemChangeHandler();
    public event ItemChangeHandler OnItemsChanged;
    
    // Event for stat changes with information about which stat changed
    public delegate void StatChangeHandler(StatType statType, bool isIncrease);
    public event StatChangeHandler OnStatChanged;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // Add an item to the player's collection
    public void AddItem(Item item)
    {
        activeItems.Add(item);
        
        // Add to type-specific list
        if (item is StatModifier statMod)
        {
            // Store original value before applying
            float originalValue = GetStatValue(statMod.statType);
            
            activeStatModifiers.Add(statMod);
            statMod.Apply();
            
            // Check if the stat value increased or decreased
            float newValue = GetStatValue(statMod.statType);
            bool isIncrease = newValue > originalValue;
            
            // Trigger stat change event
            OnStatChanged?.Invoke(statMod.statType, isIncrease);
        }
        else if (item is AbilityItem ability)
        {
            activeAbilities.Add(ability);
            ability.Apply();
        }
        else if (item is WeaponModifier weaponMod)
        {
            activeWeaponModifiers.Add(weaponMod);
            weaponMod.Apply();
        }
        
        // Notify listeners about item change
        OnItemsChanged?.Invoke();
    }
    
    // Helper method to get the current value of a stat from StatsManager
    private float GetStatValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.Damage:
                return StatsManager.Instance.damage;
            case StatType.BulletSpeed:
                return StatsManager.Instance.bulletSpeed;
            case StatType.AttackSpeed:
                return StatsManager.Instance.atkSpeed;
            case StatType.MaxHealth:
                return StatsManager.Instance.maxHealth;
            case StatType.MovementSpeed:
                return StatsManager.Instance.movementSpeed;
            default:
                return 0;
        }
    }
    
    // Process active items that need updating
    private void Update()
    {
        foreach (var item in activeItems)
        {
            if (item.needsUpdate)
                item.UpdateEffect();
        }
    }
    
    // Calculate total stat bonuses from all active items
    public float GetStatModifier(StatType statType)
    {
        float totalModifier = 0f;
        
        foreach (var statMod in activeStatModifiers)
        {
            if (statMod.statType == statType)
                totalModifier += statMod.GetModifierValue();
        }
        
        return totalModifier;
    }
    
    // Check if a specific ability is active
    public bool HasAbility(AbilityType abilityType)
    {
        foreach (var ability in activeAbilities)
        {
            if (ability.abilityType == abilityType)
                return true;
        }
        
        return false;
    }
}

// Base class for all items
public abstract class Item
{
    public string itemName;
    public string description;
    public Sprite icon;
    public bool needsUpdate = false;
    
    public abstract void Apply();
    public virtual void UpdateEffect() { }
}

// Item types
public enum StatType
{
    Damage,
    BulletSpeed,
    AttackSpeed,
    MaxHealth,
    MovementSpeed
}

public enum AbilityType
{
    // Dash removed
    Shield,
    DoubleJump,
    Teleport
}

public enum WeaponModifierType
{
    DoubleShot,
    Spread,
    Homing,
    Piercing,
    Explosive
}

// Item that modifies player stats
public class StatModifier : Item
{
    public StatType statType;
    public float value;
    public bool isPercentage = false;
    
    public override void Apply()
    {
        // Apply stat change to StatsManager
        switch (statType)
        {
            case StatType.Damage:
                if (isPercentage)
                    StatsManager.Instance.damage = Mathf.RoundToInt(StatsManager.Instance.damage * (1 + value));
                else
                    StatsManager.Instance.damage += Mathf.RoundToInt(value);
                break;
                
            case StatType.BulletSpeed:
                if (isPercentage)
                    StatsManager.Instance.bulletSpeed = Mathf.RoundToInt(StatsManager.Instance.bulletSpeed * (1 + value));
                else
                    StatsManager.Instance.bulletSpeed += Mathf.RoundToInt(value);
                break;
                
            case StatType.AttackSpeed:
                if (isPercentage)
                    StatsManager.Instance.atkSpeed *= (1 - value); // Lower is faster
                else
                    StatsManager.Instance.atkSpeed -= value;
                break;
                
            case StatType.MaxHealth:
                if (isPercentage)
                    StatsManager.Instance.maxHealth = Mathf.RoundToInt(StatsManager.Instance.maxHealth * (1 + value));
                else
                    StatsManager.Instance.maxHealth += Mathf.RoundToInt(value);
                StatsManager.Instance.PlayerHealth += Mathf.RoundToInt(value); // Also heal the player
                break;
                
            case StatType.MovementSpeed:
                if (isPercentage)
                    StatsManager.Instance.movementSpeed *= (1 + value);
                else
                    StatsManager.Instance.movementSpeed += value;
                break;
        }
    }
    
    public float GetModifierValue()
    {
        return value;
    }
}

// Item that grants new abilities
public class AbilityItem : Item
{
    public AbilityType abilityType;
    
    public override void Apply()
    {
        // Each ability would be implemented in the player controller
        // This just signals that the ability is available
    }
}

// Item that modifies weapon behavior
public class WeaponModifier : Item
{
    public WeaponModifierType modifierType;
    
    public override void Apply()
    {
        // Apply weapon modification
        // The actual implementation would depend on your weapon system
    }
}
