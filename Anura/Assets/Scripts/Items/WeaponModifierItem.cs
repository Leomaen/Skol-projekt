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
    
    public WeaponModifierItem()
    {
        itemType = ItemType.WeaponModifier;
    }
    
    public override void ApplyEffect()
    {
        // Add this weapon modifier to the global list
        ItemManager.Instance.AddWeaponModifier(new WeaponModifier { modifierType = modifierType });
        
        Debug.Log($"Applied weapon modifier: {modifierType}");
    }
    
    public override void RemoveEffect()
    {
        // Remove this weapon modifier from the global list
        ItemManager.Instance.RemoveWeaponModifier(modifierType);
        
        Debug.Log($"Removed weapon modifier: {modifierType}");
    }
}