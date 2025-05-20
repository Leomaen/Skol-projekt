using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameState gameState;
    public UserData userData;
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel;
    private bool gameOverTriggered = false;

    void Awake()
    {
        PlayerController.OnPlayerDamaged += CheckIfGameOver;
    }


    private void CheckIfGameOver()
    {
        if (!gameOverTriggered && gameState.stats.PlayerHealth <= 0)
        {
            GameOver();
        }
    }

    public async void GameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        Time.timeScale = 0f;
        gameState.DeleteSave();
        if (pauseMenuPanel != null) { pauseMenuPanel.SetActive(false); }
        AudioManager.Instance.PlayMenuOpen();
        gameOverPanel.SetActive(true);
        userData.stats.totalDeaths++;
        userData.Save();
        await userData.PushStats();

        Debug.Log("Game over :(");
    }
}