using System;
using System.Collections;
using System.Collections.Generic;
/*using System.IO;*/
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum JumpscareType
{
    Chaser,
    Angel,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

public bool IsPaused { get; private set; }

    [Header("Soulplayback")]
    [SerializeField]
    private GameObject soulPrefab;

    [SerializeField]
    private float spawnDelay = 5f;
    private SoulRecorder playerSoulRecorder;
    /*private string soulSaveFilePath;*/

    [Header("Fade ref")]
    [SerializeField]
    private Canvas fadeCanvas;

    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private float fadeSpeed = 2f;

    [Header("UI refs")]
    [SerializeField]
    private GameObject gameOverMenu;

    [SerializeField]
    private GameObject gameWinMenu;

    [SerializeField]
    private GameObject gamePauseMenu;

    [Header("Player Data")]
    public bool hasKey = false;
    public int totalPoints = 0;
    public bool hasRed = false;
    public bool hasBlue = false;
    public bool hasGreen = false;

    [Header("Jumpscare references")]
    [SerializeField]
    private GameObject angelJumpscareModel;

    [SerializeField]
    private GameObject chaserJumpscareModel;

    [SerializeField]
    private AudioSource jumpscareAudioSource;

    [SerializeField]
    private AudioSource lessScaryJumpscareAudioSource;

    [SerializeField]
    private AudioClip lessScaryJumpScareSound;

    [SerializeField]
    private AudioClip jumpscareSound;

    [SerializeField]
    private float jumpscareShakeIntensity = 2.0f;

    private bool isGameOver = false;

    private const string SOULS_SAVE_KEY = "SoulsData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            /*soulSaveFilePath = Application.persistentDataPath + "/souls.json";*/

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        IsPaused = false;
        isGameOver = false;
        bool isMenuScene = scene.name == "MainMenu";
        Cursor.lockState = isMenuScene ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isMenuScene;

        if (fadeCanvas != null && fadeCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            fadeCanvas.worldCamera = Camera.main;
        }
        Time.timeScale = 1f;
        StartCoroutine(Fade(false));

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerSoulRecorder = player.GetComponent<SoulRecorder>();
            if (playerSoulRecorder != null)
            {
                playerSoulRecorder.StartRecording();
            }
        }

        PlayBackSouls();

        GameObject gameOverObject = GameObject.FindGameObjectWithTag("GameOverUI");
        if (gameOverObject != null)
        {
            gameOverMenu = gameOverObject;
            gameOverMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GAMEOVERUI object not set");
        }

        GameObject gameWinObject = GameObject.FindGameObjectWithTag("GameWinUI");
        if (gameWinObject != null)
        {
            gameWinMenu = gameWinObject;
            gameWinMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GAMEWINUI object not set");
        }

        GameObject gamePauseObject = GameObject.FindGameObjectWithTag("GamePauseUI");
        if (gamePauseObject != null)
        {
            gamePauseMenu = gamePauseObject;
            gamePauseMenu.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GAMEPAUSEUI object not set");
        }

        GameObject angelModel = GameObject.FindGameObjectWithTag("AngelJumpscare");
        if (angelModel != null)
        {
            angelJumpscareModel = angelModel;
            angelJumpscareModel.SetActive(false);
        }

        GameObject chaserModel = GameObject.FindGameObjectWithTag("ChaserJumpscare");
        if (chaserModel != null)
        {
            chaserJumpscareModel = chaserModel;
            chaserJumpscareModel.SetActive(false);
        }

        GameObject audioSourceObject = GameObject.FindGameObjectWithTag("JumpscareAudio");
        if (audioSourceObject != null)
        {
            jumpscareAudioSource = audioSourceObject.GetComponent<AudioSource>();
        }

         GameObject lessScaryAudioSourceObject = GameObject.FindGameObjectWithTag("LessScaryJumpscareAudio");
        if (lessScaryAudioSourceObject != null)
        {
            lessScaryJumpscareAudioSource = audioSourceObject.GetComponent<AudioSource>();
        }

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        /* SpawnDeathMarkers(); */
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isGameOver)
        {
            if (gamePauseMenu != null && gamePauseMenu.activeSelf)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    private void SaveSoulData(SoulData newData)
    {
        AllSoulsData allSouls = LoadAllSoulsData();
        allSouls.soulList.Add(newData);
        while (allSouls.soulList.Count > 15)
        {
            allSouls.soulList.RemoveAt(0);
        }

        string json = JsonUtility.ToJson(allSouls);
        PlayerPrefs.SetString(SOULS_SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private AllSoulsData LoadAllSoulsData()
    {
        if (PlayerPrefs.HasKey(SOULS_SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SOULS_SAVE_KEY);
            AllSoulsData data = JsonUtility.FromJson<AllSoulsData>(json);
            if (data != null)
            {
                return data;
            }
        }
        return new AllSoulsData();
    }

    private void PlayBackSouls()
    {
        AllSoulsData allSouls = LoadAllSoulsData();

        if (allSouls == null || soulPrefab == null)
            return;

        foreach (SoulData soulData in allSouls.soulList)
        {
            if (soulData.snapshots.Count > 0)
            {
                GameObject soulInstance = Instantiate(
                    soulPrefab,
                    soulData.snapshots[0].position,
                    Quaternion.identity
                );
                SoulPlayback playback = soulInstance.GetComponent<SoulPlayback>();
                if (playback != null)
                {
                    playback.StartPlayback(soulData);
                }
            }
        }
    }

    public void AddRed()
    {
        hasRed = true;
    }

    public void AddBlue()
    {
        hasBlue = true;
    }

    public void AddGreen()
    {
        hasGreen = true;
    }

    public void RemoveRed()
    {
        hasRed = false;
    }

    public void RemoveBlue()
    {
        hasBlue = false;
    }

    public void RemoveGreen()
    {
        hasGreen = false;
    }

    public void AddKey() { }

    public void AddPoints(int amount)
    {
        totalPoints += amount;
        Debug.Log($"Spheres collected: {totalPoints}");
    }

    public void TriggerJumpscare(JumpscareType type, Vector3 playerPos)
    {
        if (playerSoulRecorder != null)
        {
            playerSoulRecorder.StopRecording();
            SoulData currentSoulData = playerSoulRecorder.GetSoulData();
            if (currentSoulData.snapshots.Count > 0)
            {
                SaveSoulData(currentSoulData);
            }
        }
        StartCoroutine(JumpscareSequence(type));
    }

    private IEnumerator JumpscareSequence(JumpscareType type)
    {
        if (type == JumpscareType.Chaser && chaserJumpscareModel != null)
        {
            chaserJumpscareModel.SetActive(true);
        }
        if (type == JumpscareType.Angel && angelJumpscareModel != null)
        {
            angelJumpscareModel.SetActive(true);
        }

        bool useLessScary = SettingsManager.Instance != null &&
        !SettingsManager.Instance.CurrentSettings.jumpscareChanger;

        AudioSource audioSource = useLessScary ? lessScaryJumpscareAudioSource : jumpscareAudioSource;

        AudioClip audioClip = useLessScary ? lessScaryJumpScareSound : jumpscareSound;

        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }

yield return new WaitForSecondsRealtime(1f);

    EndGame();
    }

     public void OpenPauseMenu()
    {
        IsPaused = true;
        if (gamePauseMenu != null)
        {
            gamePauseMenu.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePauseMenu()
    {
        IsPaused = false;
        if (gamePauseMenu != null)
        {
            gamePauseMenu.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void EndGame()
    {
        Debug.Log("Game over");
        IsPaused = true;
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        isGameOver = true;

        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
    }

    public void WinGame()
    {
        Debug.Log("Game won");
        IsPaused = true;
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        isGameOver = true;

        if (gameWinMenu != null)
        {
            gameWinMenu.SetActive(true);
        }
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1f;
    }

    public void ReturnToMain()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeLoadScene("MainMenu"));
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        hasKey = false;

        StartCoroutine(FadeLoadScene(SceneManager.GetActiveScene().name));
    }

    public void LoadNextLevel(string sceneName)
    {
        StartCoroutine(FadeLoadScene(sceneName));
    }

    public void PlayGame()
    {
        StartCoroutine(FadeLoadScene("SampleScene"));
    }

    public void LoadCredits()
    {
        StartCoroutine(FadeLoadScene("CreditsScene"));
    }

    public void QuitGame()
    {
        StartCoroutine(FadeQuit());
    }

    public void OpenOptions() { }

    private IEnumerator Fade(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 1.0f : 0.0f;
        float startAlpha = fadeImage.color.a;

        float time = 0;
        while (time < fadeSpeed)
        {
            time += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeSpeed);
            fadeImage.color = new Color(0, 0, 0, newAlpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }

    private IEnumerator FadeLoadScene(string sceneName)
    {
        yield return StartCoroutine(Fade(true));

        Debug.Log($"FADE COMPLETE. Attempting to load scene: {sceneName}");
        SceneManager.LoadScene(sceneName);

        Debug.Log($"Scene not loaded successfully");
    }

    private IEnumerator FadeQuit()
    {
        yield return null;
        yield return StartCoroutine(Fade(true));

#if UNITY_WEBGL
    SceneManager.LoadScene("MainMenu");
#else
        Application.Quit();
#endif
    }
}

[System.Serializable]
public class AllSoulsData
{
    public List<SoulData> soulList = new List<SoulData>();
}
