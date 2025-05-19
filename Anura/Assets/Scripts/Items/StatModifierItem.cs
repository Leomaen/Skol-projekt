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
    public GameState gameState;
    public StatType statType;
    public int modifierValue;

    public StatModifierItem()
    {
        itemType = ItemType.StatModifier;
    }

    public void OnEnable()
    {
        Debug.Log($"GameState found: {gameState}");
    }

    public override void ApplyEffect()
    {
        switch (statType)
        {
            case StatType.MovementSpeed:
                gameState.stats.movementSpeed += modifierValue;
                Debug.Log($"Movement speed increased by {modifierValue}. New speed: {gameState.stats.movementSpeed}");
                break;
            case StatType.atksPeed:
                gameState.stats.atkSpeed += modifierValue;
                Debug.Log($"Attack speed modified by {modifierValue}");
                break;
            case StatType.Health:
                gameState.stats.maxHealth += Mathf.RoundToInt(modifierValue);
                gameState.stats.PlayerHealth += Mathf.RoundToInt(modifierValue);
                Debug.Log($"Max health increased by {modifierValue}");
                break;
            case StatType.Damage:
                gameState.stats.damage += modifierValue;
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
                gameState.stats.movementSpeed -= modifierValue;
                break;
            case StatType.atksPeed:
                gameState.stats.atkSpeed -= modifierValue;
                break;
            case StatType.Health:
                gameState.stats.maxHealth -= Mathf.RoundToInt(modifierValue);
                // Don't reduce current health below 1
                gameState.stats.PlayerHealth = Mathf.Max(1, gameState.stats.PlayerHealth - Mathf.RoundToInt(modifierValue));
                break;
            case StatType.Damage:
                gameState.stats.damage -= modifierValue;
                break;
        }
    }
}