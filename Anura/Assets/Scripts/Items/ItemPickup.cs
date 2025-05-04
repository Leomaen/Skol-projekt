using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemPickupType
    {
        StatBoost,
        Ability,
        WeaponMod
    }
    
    public ItemPickupType pickupType;
    
    // For stat boosts
    public StatType statType;
    public float modifierValue = 1f;
    public bool isPercentage = false;
    
    // For abilities
    public AbilityType abilityType;
    
    // For weapon modifiers
    public WeaponModifierType weaponModType;
    
    // Visual properties
    public Sprite icon;
    public string itemName;
    public string itemDescription;
    
    // Visual effects
    public GameObject pickupEffectPrefab;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Item item = CreateItemFromPickup();
            
            if (item != null)
            {
                if (ItemManager.Instance == null)
                {
                    Debug.LogError("ItemManager.Instance is null! Cannot add item.");
                    return;
                }
                
                ItemManager.Instance.AddItem(item);
                
                // Debug log for ability pickup
                if (item is AbilityItem abilityItem)
                {
                    Debug.Log($"Picked up ability: {abilityItem.abilityType}");
                }
                
                // Play pickup effect
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnStatChanged -= FlashStatUI;
        }
    }
    
    // This will trigger the UI flash effect when a stat is changed
    private void FlashStatUI(StatType changedStatType, bool isIncrease)
    {
        // Only flash if this pickup's stat type matches the changed stat
        if (pickupType == ItemPickupType.StatBoost && changedStatType == statType)
        {
            // Find StatsUI in scene
            StatsUI statsUI = FindFirstObjectByType<StatsUI>();
            if (statsUI != null)
            {
                statsUI.FlashStatUI(statType, isIncrease);
            }
        }
    }
    
    private Item CreateItemFromPickup()
    {
        switch (pickupType)
        {
            case ItemPickupType.StatBoost:
                StatModifier statMod = new StatModifier
                {
                    itemName = itemName,
                    description = itemDescription,
                    icon = icon,
                    statType = statType,
                    value = modifierValue,
                    isPercentage = isPercentage
                };
                return statMod;
                
            case ItemPickupType.Ability:
                AbilityItem ability = new AbilityItem
                {
                    itemName = itemName,
                    description = itemDescription,
                    icon = icon,
                    abilityType = abilityType
                };
                return ability;
                
            case ItemPickupType.WeaponMod:
                WeaponModifier weaponMod = new WeaponModifier
                {
                    itemName = itemName,
                    description = itemDescription,
                    icon = icon,
                    modifierType = weaponModType
                };
                return weaponMod;
        }
        
        return null;
    }
}
