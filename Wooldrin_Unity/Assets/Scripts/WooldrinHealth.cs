using UnityEngine;
using System.Collections;
using Cinemachine; // Required for screen shake

public class WooldrinHealth : MonoBehaviour
{
    [Header("Health")]
    public int woolLayers = 3;
    public float invincibilityTime = 1.5f;

    [Header("Knockback & Shake")]
    [Tooltip("How long the player is 'stunned' during knockback.")]
    public float knockbackDuration = 0.2f;

    private float invinceTimer;
    private Vector3 originalScale;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private CinemachineImpulseSource impulseSource; // The screen shake component
    private bool isDead = false;

    public bool isBeingKnockedBack { get; private set; }
    public bool IsInvulnerable => Time.time < invinceTimer;

    void Awake()
    {
        originalScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        if (rb != null) rb.freezeRotation = true;
    }

    // UPDATED: Now accepts a custom force value from the attacker
    public void TakeDamage(Vector2 attackerPos, float knockbackForce)
    {
        if (IsInvulnerable || woolLayers <= 0 || isDead) return;

        woolLayers--;
        invinceTimer = Time.time + invincibilityTime;

        // 1. SCREEN SHAKE
        if (impulseSource != null)
        {
            // Scale the shake based on how hard the hit was
            float shakeIntensity = Mathf.Clamp(knockbackForce / 10f, 0.5f, 2f);
            impulseSource.GenerateImpulse(Vector3.one * shakeIntensity);
        }

        // 2. KNOCKBACK
        if (rb != null)
        {
            StartCoroutine(KnockbackRoutine(attackerPos, knockbackForce));
        }

        UpdateVisualScale();
        StartCoroutine(HurtFlash());

        // 3. DEATH CHECK
        if (woolLayers <= 0)
        {
            isDead = true;
            if (GameManager.Instance != null) Invoke("TriggerGameOver", 0.5f);
        }
    }

    private void TriggerGameOver() { GameManager.Instance.GameOver(); }

    private IEnumerator KnockbackRoutine(Vector2 attackerPos, float force)
    {
        isBeingKnockedBack = true;

        Vector2 pushDirection = ((Vector2)transform.position - attackerPos).normalized;

        rb.velocity = Vector2.zero; // Clear momentum for a clean shove
        rb.AddForce(pushDirection * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isBeingKnockedBack = false;
    }

    public void ResetHealth()
    {
        woolLayers = 3;
        isDead = false;
        transform.localScale = originalScale;
        invinceTimer = 0;
        if (sr != null) sr.color = Color.white;
    }

    private void UpdateVisualScale()
    {
        int clampedLayers = Mathf.Clamp(woolLayers, 0, 3);
        float targetScaleMultiplier = Mathf.Clamp((float)clampedLayers / 3f, 0.3f, 1f);
        transform.localScale = originalScale * targetScaleMultiplier;
    }

    IEnumerator HurtFlash()
    {
        float elapsed = 0;
        while (elapsed < invincibilityTime)
        {
            if (sr != null) sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }
        if (sr != null) sr.color = Color.white;
    }
}