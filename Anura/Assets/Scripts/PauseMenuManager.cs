using UnityEngine;
public class PauseManager : MonoBehaviour
{

    bool isPaused = false;
    [SerializeField] GameObject pauseMenuUI;

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
}
