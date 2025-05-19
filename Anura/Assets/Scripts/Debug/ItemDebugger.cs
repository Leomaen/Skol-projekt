using UnityEngine;
using System.Text;

public class ItemDebugger : MonoBehaviour
{
    [SerializeField] private bool showCollectedItemsInLog = false;
    [SerializeField] private KeyCode debugKey = KeyCode.F2;
    
    void Update()
    {
        if (showCollectedItemsInLog && Input.GetKeyDown(debugKey))
        {
            if (ItemManager.Instance != null)
            {
                string[] collectedItems = ItemManager.Instance.GetAllCollectedItemIds();
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== COLLECTED ITEMS ({collectedItems.Length}) ===");
                
                foreach (string itemId in collectedItems)
                {
                    sb.AppendLine(itemId);
                }
                
                Debug.Log(sb.ToString());
            }
            else
            {
                Debug.Log("ItemManager not available");
            }
        }
    }
}
