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
    public bool feedbacks = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = this;

        if (instance != null)
            this.currentLevelId = instance.currentLevelId;

        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;    
        }
        currentLevelId = SceneManager.GetActiveScene().buildIndex;
    }

    public void LoadScene(int levelId)
    {
        SceneManager.LoadScene(levelId);
        //currentLevelId = SceneManager.GetActiveScene().buildIndex;
        currentLevelId = levelId;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(currentLevelId);
    }

    public void NextLevelScene()
    {
        currentLevelId = SceneManager.GetActiveScene().buildIndex;
        if (currentLevelId < 5)
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
    }

    public void GameUnPaused()
    {
        pauseCanvas.SetActive(false);
    }

    public int getLevelId()
    {
        return currentLevelId;
    }

    public void setFeedbacks(bool enabled)
    {
        feedbacks = enabled;
        Debug.Log("switch");
    }

    public bool getFeedbacks()
    {
        return feedbacks;
    }
}
