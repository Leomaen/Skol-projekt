using UnityEngine;
using System.IO;
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

    public void OnSaveGameButtonClicked()
    {
        SavedGameData gameData = new SavedGameData();
        gameData.seed = FindFirstObjectByType<RoomManager>().seed;
        string json = JsonUtility.ToJson(gameData, true);
        string saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        System.IO.File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved to: " + saveFilePath);
    }
}
