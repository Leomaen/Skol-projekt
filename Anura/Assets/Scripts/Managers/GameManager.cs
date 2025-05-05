using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public GameState gameState;
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void NewGame()
    {
        Time.timeScale = 1f;
        gameState.Initialize();
    }
    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No save file found. Starting new game.");
            NewGame();
            return;
        }

        Time.timeScale = 1f;
        gameState.Load();
    }
}
