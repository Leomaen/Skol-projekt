using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] Button loadGameButton;

    private string saveFilePath; 
    private int seed;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    void Start()
    {
        Debug.Log(saveFilePath);
        Time.timeScale = 1f;

        if (File.Exists(saveFilePath))
        {
            loadGameButton.interactable = true;
        }
        else
        {
            loadGameButton.interactable = false;
        }
    }

    private int GetSavedSeed()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SavedGameData saveGameData = JsonUtility.FromJson<SavedGameData>(json);
            if (saveGameData != null)
            {
                Debug.Log("Loaded seed: " + saveGameData.seed);
                return saveGameData.seed;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            Debug.LogError("Save file not found!");
            return 0;
        }
    }

    public void OnNewGameButtonClicked()
    {
        Debug.Log("Starting New Game...");

        SceneManager.LoadSceneAsync(gameSceneName).completed += (asyncOperation) =>
        {
            RoomManager roomManager = FindFirstObjectByType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.useRandomSeed = true;
                Debug.Log("Successfully started new game with random seed.");
            }
        };
    }

    public void OnLoadGameButtonClicked()
    {
        Debug.Log("Loading Game...");
        seed = GetSavedSeed();
        SceneManager.LoadSceneAsync(gameSceneName).completed += (asyncOperation) =>
        {
            RoomManager roomManager = FindFirstObjectByType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.useRandomSeed = false;
                roomManager.seed = seed;
                Debug.Log("Successfully loaded game with saved seed.");
            }
        };
    }
}
