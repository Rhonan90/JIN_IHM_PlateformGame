using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get; private set; }

    private int currentLevelId=1;
    public GameObject endLevelCanvas;
    public GameObject pauseCanvas;
    public GameObject eventSystem;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
            Destroy(gameObject);   

        instance = this;
    }

    public void LoadScene(int levelId)
    {
        currentLevelId = levelId;
        SceneManager.LoadScene(levelId);
        currentLevelId = SceneManager.GetActiveScene().buildIndex;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(currentLevelId);
    }

    public void NextLevelScene()
    {
        if (currentLevelId < 4)
        {
            currentLevelId++;
        }
        else
            LoadScene(0);
        SceneManager.LoadScene(currentLevelId);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void EndLevelMenu()
    {
        endLevelCanvas.SetActive(true);
    }

    public void GamePausedMenu()
    {
        pauseCanvas.SetActive(true);
        //Time.timeScale = 0.1f;
    }

    public int getLevelId()
    {
        return currentLevelId;
    }
}
