using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton instance to access from any script via GameManager.Instance
    public static GameManager Instance { get; private set; }

    [Header("Progression Data")]
    public List<string> rescuedFamilyMembers = new List<string>();

    [Header("Current Level State")]
    public string currentStageName;

    private void Awake()
    {
        // Ensure only one GameManager exists across all levels
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
        // Listen for scene changes to handle spawning automatically
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    // Called automatically by Unity whenever a new scene finishes loading
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentStageName = scene.name;
        PositionPlayerAtSpawn();
    }

    public void RescueMember(string name)
    {
        if (!rescuedFamilyMembers.Contains(name))
        {
            rescuedFamilyMembers.Add(name);
            Debug.Log($"GameManager: {name} was added to the rescued list!");
        }
    }

    public void LoadNextLevel()
    {
        // Logic: Move to the next index in the Build Settings (File > Build Settings)
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("GameManager: No more levels found. You've reached the end of the Dragon!");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void PositionPlayerAtSpawn()
    {
        // This looks for the empty object you created in the hierarchy
        GameObject spawnPoint = GameObject.Find("Level_SpawnPoint");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;
            Debug.Log($"GameManager: Successfully spawned Wooldrin at {spawnPoint.name}");
        }
        else if (player != null)
        {
            Debug.LogWarning("GameManager: No 'Level_SpawnPoint' found in this scene! Wooldrin stayed at his default position.");
        }
    }
}