using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class WooldrinHealth : MonoBehaviour
{
    public int woolLayers = 3;
    public float invincibilityTime = 2f;

    private float invinceTimer;
    private Vector3 originalScale;
    private CinemachineImpulseSource impulse;

    void Start()
    {
        originalScale = transform.localScale;
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(Vector2 attackerPos)
    {
        if (Time.time < invinceTimer) return;

        woolLayers--;
        invinceTimer = Time.time + invincibilityTime;

        // Visual Feedback
        transform.localScale = originalScale * (woolLayers / 3f + 0.33f);
        if (impulse != null) impulse.GenerateImpulse();

        // Knockback
        Vector2 pushDir = ((Vector2)transform.position - attackerPos).normalized;
        GetComponent<Rigidbody2D>().AddForce(pushDir * 10f, ForceMode2D.Impulse);

        if (woolLayers <= 0) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}