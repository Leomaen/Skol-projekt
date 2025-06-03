using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneEnemySpawner : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of your test scene.")]
    public string testSceneName = "YourTestSceneName"; // IMPORTANT: Change this to your actual test scene name

    [Header("Enemy Prefabs")]
    public GameObject goopsterPrefab;
    public GameObject rattleEnemyPrefab;
    public GameObject duckBossPrefab; // Added Duck Boss prefab

    [Header("Spawn Settings")]
    [Tooltip("Optional: Assign a transform to specify where enemies spawn. If null, they spawn at this GameObject's position.")]
    public Transform spawnPointOverride;

    [Header("References")]
    [Tooltip("Assign your GameState ScriptableObject here.")]
    public GameState gameState; // Needed for enemy initialization

    void Update()
    {
        // Only allow spawning in the specified test scene
        if (SceneManager.GetActiveScene().name != testSceneName)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;
        if (spawnPointOverride != null)
        {
            spawnPosition = spawnPointOverride.position;
        }

        // Check for Numpad 1 to spawn Goopster
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (goopsterPrefab != null)
            {
                SpawnEnemy(goopsterPrefab, spawnPosition);
                Debug.Log("Attempted to spawn Goopster via Numpad 1.");
            }
            else
            {
                Debug.LogWarning("Goopster Prefab not assigned in TestSceneEnemySpawner.");
            }
        }

        // Check for Numpad 2 to spawn Rattle Enemy
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (rattleEnemyPrefab != null)
            {
                SpawnEnemy(rattleEnemyPrefab, spawnPosition);
                Debug.Log("Attempted to spawn Rattle Enemy via Numpad 2.");
            }
            else
            {
                Debug.LogWarning("Rattle Enemy Prefab not assigned in TestSceneEnemySpawner.");
            }
        }

        // Check for Numpad 3 to spawn Duck Boss
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (duckBossPrefab != null)
            {
                SpawnEnemy(duckBossPrefab, spawnPosition);
                Debug.Log("Attempted to spawn Duck Boss via Numpad 3.");
            }
            else
            {
                Debug.LogWarning("Duck Boss Prefab not assigned in TestSceneEnemySpawner.");
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        if (enemyPrefab == null) return;
    
        GameObject spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);

        // Attempt to assign GameState to the spawned enemy
        // GoopsterEnemy and RattleEnemy scripts might require it
        goopsterEnemy goopsterScript = spawnedEnemy.GetComponent<goopsterEnemy>();
        if (goopsterScript != null)
        {
            if (gameState != null)
            {
                goopsterScript.gameState = this.gameState;
            }
            else
            {
                Debug.LogWarning($"GameState not assigned in TestSceneEnemySpawner. Spawned Goopster ({spawnedEnemy.name}) might not initialize correctly.");
            }
        }

        rattleEnemy rattleScript = spawnedEnemy.GetComponent<rattleEnemy>();
        if (rattleScript != null)
        {
            if (gameState != null)
            {
                rattleScript.gameState = this.gameState;
            }
            else
            {
                Debug.LogWarning($"GameState not assigned in TestSceneEnemySpawner. Spawned Rattle Enemy ({spawnedEnemy.name}) might not initialize correctly.");
            }
        }

        // Attempt to assign GameState to the spawned Duck Boss
        BossEnemy bossScript = spawnedEnemy.GetComponent<BossEnemy>(); // Assuming your boss script is named BossEnemy
        if (bossScript != null)
        {
            if (gameState != null)
            {
                bossScript.gameState = this.gameState;
                // If the boss needs the player reference immediately, you might need to find and assign it here too.
                // bossScript.player = GameObject.FindGameObjectWithTag("Player")?.transform;
            }
            else
            {
                Debug.LogWarning($"GameState not assigned in TestSceneEnemySpawner. Spawned Duck Boss ({spawnedEnemy.name}) might not initialize correctly.");
            }
        }
    }
}