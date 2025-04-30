using UnityEngine;
using System.IO;
public class PauseManager : MonoBehaviour
{

    bool isPaused = false;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] GameManager gameManager;

    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
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
        Time.timeScale = 0f;
        isPaused = true;
        pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void OnSaveGameButtonClicked()
    {
        gameManager.SaveGameData();
    }
}
