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
        Time.timeScale = 1f;
        currentStageName = scene.name;

        if (scene.name == mainMenuName)
        {
            DestroyPersistentObjects();
            return;
        }

        // Immediate cleanup of non-persistent duplicates
        CleanupDuplicates();
    }

    public void RequestSpawn(Vector3 spawnPosition)
    {
        StartCoroutine(ExecuteSpawn(spawnPosition));
    }

    private IEnumerator ExecuteSpawn(Vector3 targetPos)
    {
        // Wait 2 frames for scene initialization to be fully complete
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 1. Find or Spawn Wooldrin
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // If player exists but is not persistent (e.g. placed in scene), mark it
        if (player != null && player.scene.name != "DontDestroyOnLoad")
        {
            DontDestroyOnLoad(player);
        }

        if (player == null && wooldrinPrefab != null)
        {
            Debug.Log("GameManager: Player missing in scene, spawning from prefab.");
            player = Instantiate(wooldrinPrefab);
            player.name = "Wooldrin";
            player.tag = "Player";
            DontDestroyOnLoad(player);
        }

        if (player != null)
        {
            // Reset physical state to prevent carrying over velocity from previous level
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            player.transform.position = new Vector3(targetPos.x, targetPos.y, 0f);

            // 2. Handle Fire Spirit 
            HandleFireSpiritSpawn(player.transform);

            // 3. Update Camera
            UpdateCameraTarget(player.transform);

            Debug.Log($"GameManager: Successfully initialized stage: {currentStageName}");
        }
    }

    private void HandleFireSpiritSpawn(Transform playerTransform)
    {
        // Only spawn or manage spirit if it has been rescued
        if (!isFireSpiritRescued) return;

        // Look for existing spirit in the entire game (including DontDestroyOnLoad)
        FireSpiritController spiritScript = FindObjectOfType<FireSpiritController>();
        GameObject spirit = spiritScript != null ? spiritScript.gameObject : null;

        // If no spirit found, spawn the prefab beside the player
        if (spirit == null && fireSpiritPrefab != null)
        {
            Debug.Log("GameManager: Rescued Spirit missing, spawning beside Wooldrin.");
            Vector3 spawnPos = playerTransform.position + spiritFollowOffset;
            spirit = Instantiate(fireSpiritPrefab, spawnPos, Quaternion.identity);
            spirit.name = "FireSpirit";
            spiritScript = spirit.GetComponent<FireSpiritController>();
        }

        if (spirit != null)
        {
            // Ensure the spirit is persistent and unparented from any scene-specific objects
            spirit.transform.SetParent(null);
            DontDestroyOnLoad(spirit);

            if (spiritScript != null)
            {
                // Re-link the spirit to follow the current level's Wooldrin instance
                spiritScript.player = playerTransform;
                spiritScript.isReady = true;

                // If it was already persistent, snap its position to stay close
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
            // Notify Cinemachine that the target has "warped" to avoid a long camera pan
            vcam.OnTargetObjectWarped(playerTransform, playerTransform.position - vcam.transform.position);
            Debug.Log("GameManager: Camera re-linked to Wooldrin.");
        }
    }

    private void CleanupDuplicates()
    {
        // Clean up duplicate players (keep the persistent one)
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1)
        {
            foreach (GameObject p in players)
            {
                if (p.scene.name != "DontDestroyOnLoad")
                {
                    Debug.Log("GameManager: Cleaning up scene-based Wooldrin duplicate.");
                    Destroy(p);
                }
            }
        }

        // Clean up duplicate spirits
        FireSpiritController[] spirits = FindObjectsOfType<FireSpiritController>();
        if (spirits.Length > 1)
        {
            foreach (var s in spirits)
            {
                if (s.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    Debug.Log("GameManager: Cleaning up scene-based FireSpirit duplicate.");
                    Destroy(s.gameObject);
                }
            }
        }
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
        Time.timeScale = 1f;
        rescuedFamilyMembers.Clear();
        isFireSpiritRescued = false;
        SceneManager.LoadScene(firstLevelName);
    }

    public void LoadNextLevel()
    {
        // Advance to the next level in the build sequence
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            WinGame();
        }
    }

    public void RestartLevel()
    {
        // Reset Wooldrin's health/state before reloading the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            WooldrinHealth health = player.GetComponent<WooldrinHealth>();
            if (health != null) health.ResetHealth();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu() { SceneManager.LoadScene(mainMenuName); }

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
            Debug.Log("GameManager: You escaped the Dragon!");
        }
        else
        {
            Debug.Log("GameManager: Ending reached, returning to menu.");
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
        Debug.Log("<color=yellow>GameManager:</color> Fire Spirit marked as RESCUED. Spawning follow-system.");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) HandleFireSpiritSpawn(player.transform);
    }
}