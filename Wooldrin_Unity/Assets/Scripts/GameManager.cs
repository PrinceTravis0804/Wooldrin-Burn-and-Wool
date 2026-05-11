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

    [Header("Fire Spirit Management")]
    public GameObject fireSpiritPrefab;
    public bool isFireSpiritRescued = false;

    [Header("Progression Data")]
    public List<string> rescuedFamilyMembers = new List<string>();

    [Header("Level Info")]
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

        CleanupDuplicates();
    }

    public void RequestSpawn(Vector3 spawnPosition)
    {
        StartCoroutine(ExecuteSpawn(spawnPosition));
    }

    private IEnumerator ExecuteSpawn(Vector3 targetPos)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 1. FIND OR SPAWN WOOLDRIN
        GameObject player = GameObject.FindGameObjectWithTag("Player");
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

            // 2. HANDLE FIRE SPIRIT
            HandleFireSpiritSpawn(player.transform);

            // 3. UPDATE CAMERA
            UpdateCameraTarget(player.transform);

            Debug.Log($"GameManager: Wooldrin initialized at {targetPos}. Camera linked.");
        }
    }

    private void HandleFireSpiritSpawn(Transform playerTransform)
    {
        if (!isFireSpiritRescued) return;

        GameObject spirit = null;
        GameObject[] allObjs = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var obj in allObjs)
        {
            if (obj.CompareTag("FireSpirit"))
            {
                spirit = obj;
                break;
            }
        }

        if (spirit == null && fireSpiritPrefab != null)
        {
            spirit = Instantiate(fireSpiritPrefab, playerTransform.position, Quaternion.identity);
            spirit.name = "FireSpirit";
            try { spirit.tag = "FireSpirit"; }
            catch { Debug.LogError("PLEASE ADD THE 'FireSpirit' TAG IN PROJECT SETTINGS!"); }
            DontDestroyOnLoad(spirit);
        }

        if (spirit != null)
        {
            FireSpiritController spiritScript = spirit.GetComponent<FireSpiritController>();
            if (spiritScript != null) spiritScript.player = playerTransform;
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
        // Cleanup Players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.scene.name != "DontDestroyOnLoad" && p.scene.isLoaded && players.Length > 1)
                Destroy(p);
        }

        // --- NEW: Cleanup Fire Spirits ---
        GameObject[] spirits = GameObject.FindGameObjectsWithTag("FireSpirit");
        foreach (GameObject s in spirits)
        {
            // If we have a persistent spirit and a scene spirit, destroy the scene spirit
            if (s.scene.name != "DontDestroyOnLoad" && s.scene.isLoaded && spirits.Length > 1)
            {
                Debug.Log("GameManager: Removing duplicate Fire Spirit from scene.");
                Destroy(s);
            }
        }
    }

    public void SetFireSpiritRescued()
    {
        isFireSpiritRescued = true;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) HandleFireSpiritSpawn(player.transform);
    }

    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LandingPage");
    }

    public void RescueFamilyMember(string name)
    {
        if (!rescuedFamilyMembers.Contains(name)) rescuedFamilyMembers.Add(name);
    }
}