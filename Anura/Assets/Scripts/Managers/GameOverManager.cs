using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameState gameState;
    public UserData userData;
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel;
    private bool gameOverTriggered = false;

    public bool IsGameOver => gameOverTriggered;

    void Awake()
    {
        PlayerController.OnPlayerDamaged += CheckIfGameOver;
        // Ensure IsGameOver is false when the manager awakes (or scene loads)
        gameOverTriggered = false; 
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerDamaged -= CheckIfGameOver;
    }

    private void CheckIfGameOver()
    {
        // Check gameState and stats nullity for safety
        if (gameState == null || gameState.stats == null)
        {
            Debug.LogError("GameOverManager: GameState or GameState.stats is null, cannot check for game over.");
            return;
        }

        if (!gameOverTriggered && gameState.stats.PlayerHealth <= 0)
        {
            GameOver();
        }
    }
    public void GameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        AudioManager.Instance.PlaySound("PlayerDeath");
        Time.timeScale = 0f; 

        // Activate Game Over panel first
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("GameOverManager: gameOverPanel is null.");
        }

        // Deactivate pause menu if it's active and exists
        if (pauseMenuPanel != null) 
        { 
            pauseMenuPanel.SetActive(false); 
        }
        
        AudioManager.Instance.PlayMenuOpen();

        if (gameState != null)
        {
            gameState.DeleteSave();
        }
        
        if (userData != null && userData.stats != null)
        {
            userData.stats.totalDeaths++;
            userData.Save();
        }

        Debug.Log("Game over :(");
    }
}