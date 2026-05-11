using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progression Data")]
    public List<string> rescuedFamilyMembers = new List<string>();

    [Header("Current Level State")]
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

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        // Safety: Always ensure time is running when a new scene loads
        Time.timeScale = 1f; 
        
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
        Time.timeScale = 1f; // Unpause before loading next level
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("GameManager: No more levels found.");
        }
    }

    public void RestartLevel()
    {
        // CRITICAL FIX: Set time back to normal before reloading
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // NEW: Function for your Main Menu button
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("LandingPage"); // Make sure this matches your scene name
    }

    private void PositionPlayerAtSpawn()
    {
        GameObject spawnPoint = GameObject.Find("Level_SpawnPoint");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.transform.position;
            Debug.Log($"GameManager: Successfully spawned Wooldrin at {spawnPoint.name}");
        }
    }
}