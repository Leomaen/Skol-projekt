using UnityEngine;

public enum ItemType
{
    StatModifier,
    WeaponModifier
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public abstract class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;

    // These methods will be implemented by specific item types
    public abstract void ApplyEffect();
    public abstract void RemoveEffect();
}