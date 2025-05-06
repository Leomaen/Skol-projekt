using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene";
    [SerializeField] Button loadGameButton;
    public GameState gameState;
    public UserData userData;

    void Start()
    {
        Time.timeScale = 1f;

        if (gameState.HasSave())
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
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnLoadGameButtonClicked()
    {
        gameState.Load();
        SceneManager.LoadScene(gameSceneName);
    }
}
