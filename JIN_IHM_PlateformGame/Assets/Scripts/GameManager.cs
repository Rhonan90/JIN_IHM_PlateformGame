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
        {
            instance = this;
            feedbacks = true;
        }

        if (instance != null)
        {
            this.currentLevelId = instance.currentLevelId;
            this.feedbacks = instance.feedbacks;
        }

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
    }

    public void GameUnPaused()
    {
        pauseCanvas.SetActive(false);
    }

    public int getLevelId()
    {
        return currentLevelId;
    }

    public void setFeedbacks()
    {
        feedbacks = !feedbacks;
        Debug.Log("switch, feedbacks set to "+feedbacks);
    }

    public bool getFeedbacks()
    {
        return feedbacks;
    }

    public void feedBacksOnText(GameObject text)
    {
        if (feedbacks)
        {
            text.SetActive(true);
        }
        else
            text.SetActive(false);
    }

    public void feedBacksOffText(GameObject text)
    {
        if (feedbacks)
        {
            text.SetActive(false);
        }
        else
            text.SetActive(true);
    }

    public void optionsMenuOn(GameObject textOn)
    {
        if (feedbacks)
        {
            textOn.SetActive(true);
        }
    }

    public void optionsMenuOff(GameObject textOff)
    {
        if (!feedbacks)
        {
            textOff.SetActive(true);
        }

    }
}
