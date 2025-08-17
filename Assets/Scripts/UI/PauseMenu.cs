using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gamePaused = false;

    public GameObject pauseMenuUi;
    public GameObject gameplayUI;
    public GameObject settingsUI;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUi.SetActive(false);
        settingsUI.SetActive(false);
        gameplayUI.SetActive(true);
        Time.timeScale = 1f;
        gamePaused = false;
    }

    void Pause()
    {
        pauseMenuUi.SetActive(true);
        gameplayUI.SetActive(false);
        Time.timeScale = 0f;
        gamePaused = true;
    }

    public void NewGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void QuitToMenu()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
