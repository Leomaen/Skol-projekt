using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameState gameState;
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel;
    private bool gameOverTriggered = false;

    void Update()
    {
        if (!gameOverTriggered && gameState.stats.PlayerHealth <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        Time.timeScale = 0f;
        gameState.DeleteSave();
        pauseMenuPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        Debug.Log("Game over :(");
    }
}