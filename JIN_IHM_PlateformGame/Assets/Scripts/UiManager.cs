using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;

    public static UiManager Instance { get; private set; }

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
