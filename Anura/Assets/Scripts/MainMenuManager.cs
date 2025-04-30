using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] Button loadGameButton;
    private string saveFilePath; 
    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    void Start()
    {
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

    public void OnNewGameButtonClicked()
    {
        SceneManager.LoadSceneAsync(gameSceneName).completed += (asyncOperation) =>
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.NewGame();
            }
        };
    }

    public void OnLoadGameButtonClicked()
    {
        SceneManager.LoadSceneAsync(gameSceneName).completed += (asyncOperation) =>
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.LoadGame();
            }
        };
    }
}
