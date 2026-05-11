using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class WooldrinHealth : MonoBehaviour
{
    public int woolLayers = 3;
    public float invincibilityTime = 2f;

    [Header("Game Over Settings")]
    // Drag your Game Over Prefab into this slot in the Inspector
    public GameObject gameOverPrefab;

    private float invinceTimer;
    private Vector3 originalScale;
    private CinemachineImpulseSource impulse;
    private bool isDead = false; // Prevents logic from firing multiple times after death

    void Start()
    {
        // Safety: Ensure time is running normally whenever the scene starts
        Time.timeScale = 1f;
        originalScale = transform.localScale;
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(Vector2 attackerPos)
    {
        // Don't take damage if already dead or currently invincible
        if (isDead || Time.time < invinceTimer) return;

        woolLayers--;
        invinceTimer = Time.time + invincibilityTime;

        // Visual Feedback
        transform.localScale = originalScale * (woolLayers / 3f + 0.33f);
        if (impulse != null) impulse.GenerateImpulse();

        // Knockback
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 pushDir = ((Vector2)transform.position - attackerPos).normalized;
            rb.velocity = Vector2.zero; // Clear existing velocity for consistent push
            rb.AddForce(pushDir * 10f, ForceMode2D.Impulse);
        }

        // Check for Death
        if (woolLayers <= 0) 
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        isDead = true;

        // 1. Show the Game Over Prefab
        if (gameOverPrefab != null)
        {
            Instantiate(gameOverPrefab);
        }

        // 2. Pause the game
        // Setting timeScale to 0 freezes physics and most movement
        Time.timeScale = 0f; 

        Debug.Log("Game Over! Wooldrin has lost all his wool.");
    }
}