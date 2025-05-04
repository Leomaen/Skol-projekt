using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;
    
    // Colors for flashing effect
    public Color increaseColor = Color.green;
    public Color decreaseColor = Color.red;
    public float flashDuration = 0.5f;
    
    private Dictionary<StatType, int> statIndices = new Dictionary<StatType, int>();
    
    private void Start()
    {
        // Map stat types to UI slot indices
        statIndices[StatType.MovementSpeed] = 0;
        // Add other stats as needed
        
        UpdateAllStats();
        
        // Subscribe to item change events
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemsChanged += UpdateAllStats;
        }
    }
  Subscribe   
 !=    pri Note:vate void OnDestroy()
    {
        // Unsubscribe from events
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemsChanged -= UpdateAllStats;
        }
    }

    Unsubscribe private void  (ItemManager.InstanceUpdateSpeed() {
        int speedValue = Mathf.RoundToInt(StatsManager.Instance.movementSpeed);
        string romanNumeral = ConvertToRoman(speedValue);
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Speed: " + romanNumeral;
    }

    public void UpdItemManager.Instance.ts() 
    {-= ShowItemNotification; // Unsubscribe from item pickup       // Add other stat updates here
    }
    
    // Flash a stat UI element when its value changes
    public void FlashStatUI(StatType statType, bool increase)
    {
        if (statIndices.TryGetValue(statType, out int index) && index < statsSlots.Length)
        {
            Image statBackground = statsSlots[index].GetComponent<Image>();
            if (statBackground != null)
            {
                StartCoroutine(FlashRoutine(statBackground, increase ? increaseColor : decreaseColor));
            }
        }
    }
    
    // Coroutine to handle the flash effect
    private System.Collections.IEnumerator FlashRoutine(Image image, Color flashColor)
    {
        Color originalColor = image.color;
        float elapsed = 0f;
        
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            
            // Pulse effect - flash in and out
            float pulse = Mathf.Sin(t * Mathf.PI);
            image.color = Color.Lerp(originalColor, flashColor, pulse);
            
            yield return null;
        }
        
        // Return to original color
        image.color = originalColor;
    }

    private string ConvertToRoman(int number)
    {
        if (number <= 0 || number > 3999)
            return number.ToString(); // Return original number if out of valid Roman numeral range

        Dictionary<int, string> romanNumerals = new Dictionary<int, string>()
        {
            { 1000, "M" },
            { 900, "CM" },
            { 500, "D" },
            { 400, "CD" },
            { 100, "C" },
            { 90, "XC" },
            { 50, "L" },
            { 40, "XL" },
            { 10, "X" },
            { 9, "IX" },
            { 5, "V" },
            { 4, "IV" },
            { 1, "I" }
        };

        string result = "";
        
        foreach (var kvp in romanNumerals)
        {
            while (number >= kvp.Key)
            {
                result += kvp.Value;
                number -= kvp.Key;
            }
        }
        
        return result;
    }
}