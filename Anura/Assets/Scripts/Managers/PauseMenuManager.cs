using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections;
using System.Threading.Tasks;

public class PauseManager : MonoBehaviour
{
    bool isPaused = false;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] GameOverManager gameOverManager;

    public UserData userData;

    void Start()
    {
        StartCoroutine(PeriodicSave());
        // Ensure pauseMenuUI is initially inactive if it's not already
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPaused)
        {
            // Ensure userData and stats are not null before accessing playTime
            if (userData != null && userData.stats != null)
            {
                float deltaTimeMS = Time.unscaledDeltaTime * 1000;
                userData.stats.playTime += Mathf.FloorToInt(deltaTimeMS);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if game is over before allowing pause/resume
            if (gameOverManager != null && gameOverManager.IsGameOver)
            {
                // If game is over, ensure pause menu is closed and do nothing else
                if (isPaused && pauseMenuUI != null)
                {
                    pauseMenuUI.SetActive(false);
                    isPaused = false; // Correct the state
                }
                return; 
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Additional check: Do not pause if game is already over
        if (gameOverManager != null && gameOverManager.IsGameOver)
        {
            return;
        }

        Time.timeScale = 0f;
        isPaused = true;
        AudioManager.Instance.PlayMenuOpen();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        if (userData != null)
        {
            userData.Save();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        AudioManager.Instance.PlayMenuClose();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        isPaused = false;
        // Optionally save before going to main menu
        if (userData != null)
        {
            userData.Save();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        if (userData != null)
        {
            userData.Save(); // Save before quitting
        }
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator PeriodicSave()
    {
        while (true)
        {
            // Wait for a minute before the first save, then periodically
            yield return new WaitForSeconds(60f); 
            if (userData != null)
            {
                userData.Save();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (userData != null)
        {
            userData.Save();
        }
    }
}