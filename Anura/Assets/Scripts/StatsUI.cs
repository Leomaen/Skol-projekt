using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;

    private void Awake()
    {
        // Subscribe to the item change event
        StartCoroutine(SubscribeAfterItemManagerInitialized());
    }

    private IEnumerator SubscribeAfterItemManagerInitialized()
    {
        // Wait until ItemManager is available
        while (ItemManager.Instance == null)
        {
            yield return null;
        }
        
        // Now it's safe to subscribe
        ItemManager.Instance.OnItemsChanged += OnItemsChanged;
        Debug.Log("Successfully subscribed to OnItemsChanged event");
    }

    private void Start()
    {
        UpdateAllStats();
    }

    private void OnItemsChanged()
    {
        UpdateAllStats();
    }

    private void UpdateSpeed() {
        int speedValue = Mathf.RoundToInt(StatsManager.Instance.movementSpeed);
        string romanNumeral = ConvertToRoman(speedValue);
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Speed: " + romanNumeral;
    }

    private void UpdateDamage() {
        int damageValue = Mathf.RoundToInt(StatsManager.Instance.damage);
        string romanNumeral = ConvertToRoman(damageValue);
        statsSlots[1].GetComponentInChildren<TMP_Text>().text = "Damage: " + romanNumeral;
    }

    private void UpdateatkSpeed() {
        int atkSpeedValue = Mathf.RoundToInt(StatsManager.Instance.atkSpeed);
        string romanNumeral = ConvertToRoman(atkSpeedValue);
        statsSlots[2].GetComponentInChildren<TMP_Text>().text = "ATKSpeed: " + romanNumeral;
    }

    private void UpdateBulletSpeed() {
        int bulletSpeedValue = Mathf.RoundToInt(StatsManager.Instance.bulletSpeed);
        string romanNumeral = ConvertToRoman(bulletSpeedValue);
        statsSlots[3].GetComponentInChildren<TMP_Text>().text = "BulletSpeed: " + romanNumeral;
    }

    public void UpdateAllStats() {
        UpdateSpeed();
        UpdateDamage();
        UpdateatkSpeed();
        UpdateBulletSpeed();
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