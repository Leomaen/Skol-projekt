using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections;
using System.Threading.Tasks;
public class PauseManager : MonoBehaviour
{

    bool isPaused = false;
    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] string mainMenuSceneName = "MainMenu";

    public UserData userData;

    void Start()
    {
        StartCoroutine(PeriodicSave());
    }

    void Update()
    {
        if (!isPaused)
        {
            float deltaTimeMS = Time.unscaledDeltaTime * 1000;
            userData.stats.playTime += Mathf.FloorToInt(deltaTimeMS);
        }

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
        userData.Save();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quiting Game...");
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator PeriodicSave()
    {
        while (true)
        {
            userData.Save();
            yield return new WaitForSeconds(60f);
        }
    }

    private void OnApplicationQuit()
    {
        userData.Save();
    }
}
