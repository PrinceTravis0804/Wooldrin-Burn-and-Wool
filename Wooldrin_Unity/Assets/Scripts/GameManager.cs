using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Management")]
    public GameObject wooldrinPrefab;

    [Header("UI Management")]
    public GameObject gameOverPrefab;
    public GameObject gameWinPrefab;

    [Header("Fire Spirit Management")]
    public GameObject fireSpiritPrefab;
    public bool isFireSpiritRescued = false;
    public Vector3 spiritFollowOffset = new Vector3(-0.7f, 0.7f, 0);

    [Header("Audio Management")]
    public AudioSource bgmSource;
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    [Header("Progression Data")]
    public List<string> rescuedFamilyMembers = new List<string>();

    [Header("Level Info")]
    public string firstLevelName = "Level_01_Throat";
    public string mainMenuName = "LandingPage";
    public string currentStageName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null) bgmSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        // Safety reset to ensure game runs at normal speed in every new scene
        Time.timeScale = 1f;
        currentStageName = scene.name;

        // --- HANDLE BACKGROUND MUSIC ---
        if (scene.name == mainMenuName)
        {
            UpdateBGM(menuMusic);
            ResetGameState();
            DestroyPersistentObjects();
            return;
        }
        else
        {
            UpdateBGM(levelMusic);
        }

        CleanupDuplicates();
    }

    private void UpdateBGM(AudioClip newClip)
    {
        if (bgmSource == null || newClip == null) return;

        if (bgmSource.clip != newClip)
        {
            bgmSource.clip = newClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void RequestSpawn(Vector3 spawnPosition)
    {
        StartCoroutine(ExecuteSpawn(spawnPosition));
    }

    private IEnumerator ExecuteSpawn(Vector3 targetPos)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && player.scene.name != "DontDestroyOnLoad")
        {
            DontDestroyOnLoad(player);
        }

        if (player == null && wooldrinPrefab != null)
        {
            player = Instantiate(wooldrinPrefab);
            player.name = "Wooldrin";
            player.tag = "Player";
            DontDestroyOnLoad(player);
        }

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            player.transform.position = new Vector3(targetPos.x, targetPos.y, 0f);
            HandleFireSpiritSpawn(player.transform);
            UpdateCameraTarget(player.transform);
        }
    }

    private void HandleFireSpiritSpawn(Transform playerTransform)
    {
        if (!isFireSpiritRescued) return;

        FireSpiritController spiritScript = FindObjectOfType<FireSpiritController>();
        GameObject spirit = spiritScript != null ? spiritScript.gameObject : null;

        if (spirit == null && fireSpiritPrefab != null)
        {
            Vector3 spawnPos = playerTransform.position + spiritFollowOffset;
            spirit = Instantiate(fireSpiritPrefab, spawnPos, Quaternion.identity);
            spirit.name = "FireSpirit";
            spiritScript = spirit.GetComponent<FireSpiritController>();
        }

        if (spirit != null)
        {
            spirit.transform.SetParent(null);
            DontDestroyOnLoad(spirit);

            if (spiritScript != null)
            {
                spiritScript.player = playerTransform;
                spiritScript.isReady = true;

                if (Vector3.Distance(spirit.transform.position, playerTransform.position) > 10f)
                {
                    spirit.transform.position = playerTransform.position + spiritFollowOffset;
                }
            }
        }
    }

    private void UpdateCameraTarget(Transform playerTransform)
    {
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = playerTransform;
            vcam.OnTargetObjectWarped(playerTransform, playerTransform.position - vcam.transform.position);
        }
    }

    private void CleanupDuplicates()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.scene.name != "DontDestroyOnLoad" && players.Length > 1)
                Destroy(p);
        }

        FireSpiritController[] spirits = FindObjectsOfType<FireSpiritController>();
        foreach (var s in spirits)
        {
            if (s.gameObject.scene.name != "DontDestroyOnLoad" && spirits.Length > 1)
                Destroy(s.gameObject);
        }
    }

    private void ResetGameState()
    {
        rescuedFamilyMembers.Clear();
        isFireSpiritRescued = false;
        Debug.Log("GameManager: State Reset (Fire Spirit is now caged/unrescued).");
    }

    private void DestroyPersistentObjects()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);

        FireSpiritController spirit = FindObjectOfType<FireSpiritController>();
        if (spirit != null) Destroy(spirit.gameObject);
    }

    // --- NAVIGATION & PUBLIC METHODS ---

    public void LoadFirstLevel()
    {
        ResetGameState();
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelName);
    }

    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            WinGame();
    }

    public void RestartLevel()
    {
        if (SceneManager.GetActiveScene().name == firstLevelName)
        {
            ResetGameState();
            FireSpiritController spirit = FindObjectOfType<FireSpiritController>();
            if (spirit != null) Destroy(spirit.gameObject);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WooldrinHealth health = player.GetComponent<WooldrinHealth>();
            if (health != null) health.ResetHealth();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // UPDATED: Faster response by resetting timescale and objects before the load begins
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Unpause immediately so the scene load doesn't feel sluggish
        ResetGameState();    // Prepare data for menu
        DestroyPersistentObjects(); // Clear the world immediately for faster visual transition
        SceneManager.LoadScene(mainMenuName);
    }

    public void GameOver()
    {
        if (gameOverPrefab != null && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
            Instantiate(gameOverPrefab);
        }
    }

    public void WinGame()
    {
        if (gameWinPrefab != null && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
            Instantiate(gameWinPrefab);
        }
        else
        {
            GoToMainMenu();
        }
    }

    public void RescueFamilyMember(string name)
    {
        if (!rescuedFamilyMembers.Contains(name)) rescuedFamilyMembers.Add(name);
    }

    public void SetFireSpiritRescued()
    {
        isFireSpiritRescued = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) HandleFireSpiritSpawn(player.transform);
    }
}