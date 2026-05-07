using UnityEngine;
using UnityEngine.SceneManagement;

public class WooldrinHealth : MonoBehaviour
{
    [Header("Wool Layer Settings")]
    public int maxLayers = 3;
    public int currentLayers;
    public float minScalePercent = 0.4f;

    [Header("Invulnerability Settings")]
    public float damageCooldown = 1.5f;
    public Color damageColor = Color.red;

    private float lastDamageTime = -100f;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // This property allows PlayerController to check if we are in hit-stun
    public bool IsInvulnerable => Time.time < lastDamageTime + damageCooldown;

    void Start()
    {
        currentLayers = maxLayers;
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        // Visual feedback for being hit (Flickering)
        if (IsInvulnerable)
        {
            float flicker = Mathf.Sin(Time.time * 25f);
            spriteRenderer.color = flicker > 0 ? damageColor : originalColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    public bool TakeDamage()
    {
        // Only take damage if the cooldown has passed
        if (!IsInvulnerable)
        {
            currentLayers--;
            lastDamageTime = Time.time;

            Debug.Log("Wooldrin: Layer lost! Remaining: " + currentLayers);

            UpdateScale();

            if (currentLayers <= 0)
            {
                GameOver();
            }

            return true; // Damage dealt successfully
        }

        return false; // Damage ignored
    }

    private void UpdateScale()
    {
        float t = (float)currentLayers / maxLayers;
        float scaleFactor = Mathf.Max(t, minScalePercent);
        transform.localScale = originalScale * scaleFactor;
    }

    void GameOver()
    {
        Debug.Log("Game Over! Wooldrin is out of wool.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}