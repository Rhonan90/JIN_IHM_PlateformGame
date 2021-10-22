using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
            Destroy(gameObject);   

        instance = this;
    }

    public void playFeedBackScene()
    {
        SceneManager.LoadScene(1);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
