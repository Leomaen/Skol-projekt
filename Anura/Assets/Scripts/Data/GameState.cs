using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Scriptable Objects/GameState")]
public class GameState : ScriptableObject
{

  [SerializeField] private string saveName = "game-state.json";
  private string savePath;
  public WorldState world = new();

  public void OnEnable()
  {
    savePath = Path.Combine(Application.persistentDataPath, saveName);

    RoomManager.OnGenerationComplete += Save;
  }

  public void OnDisable()
  {
    RoomManager.OnGenerationComplete -= Save;
  }

  public bool HasSave()
  {
    return File.Exists(savePath);
  }

  public void Save()
  {
    try
    {
      string json = JsonUtility.ToJson(this, true);
      File.WriteAllText(savePath, json);
      Debug.Log($"Game saved to: {savePath}");
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to save game: {e.Message}");
    }
  }

  public void Load()
  {
    if (!HasSave())
    {
      Debug.LogWarning("No save file found.");
      return;
    }
    try
    {
      string json = File.ReadAllText(savePath);
      JsonUtility.FromJsonOverwrite(json, this);
    }
    catch (Exception e)
    {
      Debug.LogError($"Failed to load game: {e.Message}");
    }
  }
}

[Serializable]
public class WorldState
{
  public int seed = 0;
  public bool isGenerated = false;
  // public Dictionary<Vector2Int, RoomInfo> rooms = new Dictionary<Vector2Int, RoomInfo>();
}
