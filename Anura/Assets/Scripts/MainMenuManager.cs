using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene";

    void Start()
    {
        Time.timeScale = 1f;
        Debug.Log("Main Menu Initialized.");
        OnNewGameButtonClicked();
    }

    void Update()
    {
        
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

        SceneManager.LoadSceneAsync(gameSceneName).completed += (asyncOperation) =>
        {
            RoomManager roomManager = FindFirstObjectByType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.useRandomSeed = false;
                roomManager.seed = 123456789; // mst hitta seeden men aja
                Debug.Log("Successfully loaded game with saved seed.");
            }
        };
    }
}
