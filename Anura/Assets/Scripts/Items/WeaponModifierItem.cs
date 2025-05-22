using UnityEngine;

public enum WeaponModifierType
{
    DoubleShot,
    Spread,
    Homing,
    Explosive
}

[CreateAssetMenu(fileName = "New Weapon Modifier", menuName = "Inventory/Weapon Modifier Item")]
public class WeaponModifierItem : Item
{
    public WeaponModifierType modifierType;
    private const int EXPLOSIVE_MODIFIER_DAMAGE_BONUS = 5; // Damage bonus for each explosive item
    
    public WeaponModifierItem()
    {
        itemType = ItemType.WeaponModifier;
    }
    
    public override void ApplyEffect()
    {
        // Add this weapon modifier to the global list
        ItemManager.Instance.AddWeaponModifier(new WeaponModifier { modifierType = modifierType });
        
        if (modifierType == WeaponModifierType.Explosive)
        {
            if (ItemManager.Instance != null && ItemManager.Instance.gameState != null && ItemManager.Instance.gameState.stats != null)
            {
                ItemManager.Instance.gameState.stats.damage += EXPLOSIVE_MODIFIER_DAMAGE_BONUS;
                Debug.Log($"Explosive modifier applied. Damage increased by {EXPLOSIVE_MODIFIER_DAMAGE_BONUS} to {ItemManager.Instance.gameState.stats.damage}");
            }
        }
        
        Debug.Log($"Applied weapon modifier: {modifierType}");
    }
    
    public override void RemoveEffect()
    {
        // Remove this weapon modifier from the global list
        ItemManager.Instance.RemoveWeaponModifier(modifierType);

        if (modifierType == WeaponModifierType.Explosive)
        {
            if (ItemManager.Instance != null && ItemManager.Instance.gameState != null && ItemManager.Instance.gameState.stats != null)
            {
                ItemManager.Instance.gameState.stats.damage -= EXPLOSIVE_MODIFIER_DAMAGE_BONUS;
                Debug.Log($"Explosive modifier removed. Damage decreased by {EXPLOSIVE_MODIFIER_DAMAGE_BONUS} to {ItemManager.Instance.gameState.stats.damage}");
            }
        }
        
        Debug.Log($"Removed weapon modifier: {modifierType}");
    }
}