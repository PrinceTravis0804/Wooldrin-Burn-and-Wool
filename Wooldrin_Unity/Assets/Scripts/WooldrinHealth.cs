using UnityEngine;
using UnityEngine.SceneManagement;

public class WooldrinHealth : MonoBehaviour
{
    public int maxLayers = 3;
    private int currentLayers;
    private Vector3 originalScale;

    [Header("Invincibility Settings")]
    public float damageCooldown = 1.5f; // Seconds of safety after getting hit
    private float lastDamageTime;

    void Start()
    {
        currentLayers = maxLayers;
        originalScale = transform.localScale;
    }

    public void TakeDamage()
    {
        // Only take damage if the cooldown has passed
        if (Time.time > lastDamageTime + damageCooldown)
        {
            currentLayers--;
            lastDamageTime = Time.time;

            Debug.Log("Ouch! Wool layers left: " + currentLayers);

            // Visual feedback: Shrink Wooldrin
            float t = (float)currentLayers / maxLayers;
            transform.localScale = originalScale * Mathf.Max(t, 0.4f);

            // Optional: Make Wooldrin flash red when hit
            StartCoroutine(FlashRed());

            if (currentLayers <= 0)
            {
                GameOver();
            }
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}