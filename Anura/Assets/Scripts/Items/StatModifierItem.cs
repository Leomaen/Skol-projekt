using UnityEngine;

public enum StatType
{
    MovementSpeed,
    atkSpeed,
    Health,
    Damage
}

[CreateAssetMenu(fileName = "New Stat Item", menuName = "Inventory/Stat Modifier Item")]
public class StatModifierItem : Item
{
    public GameState gameState; // Ensure this is assigned in the Inspector for each StatModifierItem asset
    public StatType statType;
    public float modifierValue; // Changed from int to float

    private const float MIN_ATTACK_SPEED = 0.05f; // Define a minimum attack speed

    public StatModifierItem()
    {
        itemType = ItemType.StatModifier;
    }

    // OnEnable for ScriptableObjects is called when the asset is loaded.
    // The gameState field needs to be assigned via the Inspector.
    // public void OnEnable()
    // {
    //     // This log might be misleading as gameState is assigned, not "found" here.
    //     // Debug.Log($"StatModifierItem {this.name}: GameState assigned in Inspector: {gameState != null}");
    // }

    public override void ApplyEffect()
    {
        if (gameState == null || gameState.stats == null)
        {
            Debug.LogError($"StatModifierItem ({name}): GameState or GameState.stats is not assigned. Cannot apply effect.");
            return;
        }

        switch (statType)
        {
            case StatType.MovementSpeed:
                gameState.stats.movementSpeed += modifierValue;
                Debug.Log($"Movement speed modified by {modifierValue}. New speed: {gameState.stats.movementSpeed}");
                break;
            case StatType.atkSpeed:
                gameState.stats.atkSpeed += modifierValue;
                // Ensure attack speed doesn't go below a minimum threshold
                if (gameState.stats.atkSpeed < MIN_ATTACK_SPEED)
                {
                    gameState.stats.atkSpeed = MIN_ATTACK_SPEED;
                }
                Debug.Log($"Attack speed modified by {modifierValue}. New attack speed: {gameState.stats.atkSpeed}");
                break;
            case StatType.Health:
                int healthChange = Mathf.RoundToInt(modifierValue);
                gameState.stats.maxHealth += healthChange;
                gameState.stats.PlayerHealth += healthChange; // Also increase current health
                if (gameState.stats.PlayerHealth > gameState.stats.maxHealth) // Cap current health at max health
                {
                    gameState.stats.PlayerHealth = gameState.stats.maxHealth;
                }
                if (gameState.stats.maxHealth < 1) gameState.stats.maxHealth = 1; // Ensure max health is at least 1
                if (gameState.stats.PlayerHealth < 1 && gameState.stats.maxHealth >=1) gameState.stats.PlayerHealth = 1; // Ensure current health is at least 1 if max health allows

                Debug.Log($"Max health modified by {healthChange}. New max health: {gameState.stats.maxHealth}");
                break;
            case StatType.Damage:
                int damageChange = Mathf.RoundToInt(modifierValue);
                gameState.stats.damage += damageChange;
                if (gameState.stats.damage < 0) gameState.stats.damage = 0; // Prevent negative damage
                Debug.Log($"Damage modified by {damageChange}. New damage: {gameState.stats.damage}");
                break;
        }
    }

    public override void RemoveEffect()
    {
        if (gameState == null || gameState.stats == null)
        {
            Debug.LogError($"StatModifierItem ({name}): GameState or GameState.stats is not assigned. Cannot remove effect.");
            return;
        }

        // Reverse the stat changes if the item is removed
        switch (statType)
        {
            case StatType.MovementSpeed:
                gameState.stats.movementSpeed -= modifierValue;
                break;
            case StatType.atkSpeed:
                gameState.stats.atkSpeed -= modifierValue;
                // Ensure attack speed doesn't go below a minimum threshold even when removing
                if (gameState.stats.atkSpeed < MIN_ATTACK_SPEED)
                {
                    gameState.stats.atkSpeed = MIN_ATTACK_SPEED;
                }
                break;
            case StatType.Health:
                int healthChange = Mathf.RoundToInt(modifierValue);
                gameState.stats.maxHealth -= healthChange;
                if (gameState.stats.maxHealth < 1) gameState.stats.maxHealth = 1; // Ensure max health is at least 1
                                                                                // Adjust current health, ensuring it doesn't exceed new maxHealth or go below 1
                gameState.stats.PlayerHealth = Mathf.Clamp(gameState.stats.PlayerHealth - healthChange, 1, gameState.stats.maxHealth);
                break;
            case StatType.Damage:
                int damageChange = Mathf.RoundToInt(modifierValue);
                gameState.stats.damage -= damageChange;
                if (gameState.stats.damage < 0) gameState.stats.damage = 0; // Prevent negative damage
                break;
        }
    }
}