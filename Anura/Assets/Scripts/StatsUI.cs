using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;

    private void Start()
    {
        UpdateAllStats();
    }

    private void UpdateSpeed() {
        int speedValue = Mathf.RoundToInt(StatsManager.Instance.movementSpeed);
        string romanNumeral = ConvertToRoman(speedValue);
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Speed: " + romanNumeral;
    }

    public void UpdateAllStats() {
        UpdateSpeed();
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