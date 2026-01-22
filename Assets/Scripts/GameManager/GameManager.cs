using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI refs")]
    [SerializeField] private GameObject gameOverMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EndGame()
    {
        Debug.Log("Game over");
        Time.timeScale = 0f;

        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}
