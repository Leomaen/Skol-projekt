using UnityEngine;

public enum StatType
{
    MovementSpeed,
    atksPeed,
    Health,
    Damage
}

[CreateAssetMenu(fileName = "New Stat Item", menuName = "Inventory/Stat Modifier Item")]
public class StatModifierItem : Item
{
    public StatType statType;
    public int modifierValue;
    
    public StatModifierItem()
    {
        itemType = ItemType.StatModifier;
    }
    
    public override void ApplyEffect()
    {
        switch (statType)
        {
            case StatType.MovementSpeed:
                StatsManager.Instance.movementSpeed += modifierValue;
                Debug.Log($"Movement speed increased by {modifierValue}. New speed: {StatsManager.Instance.movementSpeed}");
                break;
            case StatType.atksPeed:
                StatsManager.Instance.atkSpeed += modifierValue;
                Debug.Log($"Attack speed modified by {modifierValue}");
                break;
            case StatType.Health:
                StatsManager.Instance.maxHealth += Mathf.RoundToInt(modifierValue);
                StatsManager.Instance.PlayerHealth += Mathf.RoundToInt(modifierValue);
                Debug.Log($"Max health increased by {modifierValue}");
                break;
            case StatType.Damage:
                StatsManager.Instance.damage += modifierValue;
                Debug.Log($"Damage increased by {modifierValue}");
                break;
        }
    }
    
    public override void RemoveEffect()
    {
        // Reverse the stat changes if the item is removed
        switch (statType)
        {
            case StatType.MovementSpeed:
                StatsManager.Instance.movementSpeed -= modifierValue;
                break;
            case StatType.atksPeed:
                StatsManager.Instance.atkSpeed -= modifierValue;
                break;
            case StatType.Health:
                StatsManager.Instance.maxHealth -= Mathf.RoundToInt(modifierValue);
                // Don't reduce current health below 1
                StatsManager.Instance.PlayerHealth = Mathf.Max(1, StatsManager.Instance.PlayerHealth - Mathf.RoundToInt(modifierValue));
                break;
            case StatType.Damage:
                StatsManager.Instance.damage -= modifierValue;
                break;
        }
    }
}