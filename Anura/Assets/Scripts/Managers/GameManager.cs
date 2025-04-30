using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public GameData gameData;
    private string saveFilePath;

  void Awake()
  {
    saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
  }

  public void NewGame()
    {
        Time.timeScale = 1f;

        gameData = new GameData();
        SaveGameData();
    }
    public void LoadGame()
    {
        Time.timeScale = 1f;

        gameData = GetGameData();
    }

    public GameData GetGameData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            return gameData;
        }
        else
        {
            Debug.LogError("Save file not found!");
            return null;
        }
    }
    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved to: " + saveFilePath);
    }
}
