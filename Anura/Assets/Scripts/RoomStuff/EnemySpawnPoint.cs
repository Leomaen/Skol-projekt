using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public GameObject enemyPrefab;
    
    // You can add spawn properties like:
    [SerializeField] private float spawnDelay = 0f;
    [SerializeField] private bool randomizePosition = false;
    [SerializeField] private float randomRadius = 1f;
    
    // Visual representation in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (randomizePosition)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, randomRadius);
        }
        
        // Draw a line to show the enemy type if one is assigned
        if (enemyPrefab != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, enemyPrefab.name);
            #endif
        }
    }
}
