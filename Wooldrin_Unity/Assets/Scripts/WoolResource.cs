using UnityEngine;

public class WoolResource : MonoBehaviour
{
    public float health = 100f;
    private float maxHealth;
    private Vector3 originalScale;

    void Start()
    {
        maxHealth = health;
        originalScale = transform.localScale;

        // 1. TAG CHECK: The brain relies entirely on the 'Wool' tag
        if (!gameObject.CompareTag("Wool"))
        {
            Debug.LogWarning("WoolResource: Tagging this object as 'Wool' automatically.");
            gameObject.tag = "Wool";
        }

        // 2. SENSORY CHECK: Enemies use OverlapCircleAll, which REQUIRES a collider.
        // If you forgot to add one to the prefab, we'll add a trigger now.
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    /// <summary>
    /// Called by the AgentUtilityBrain when the enemy is in range.
    /// </summary>
    public void TakeBite(float damage)
    {
        health -= damage;

        // Visual feedback: Wool shrinks as it is eaten
        float t = Mathf.Clamp01(health / maxHealth);
        transform.localScale = originalScale * t;

        if (health <= 0)
        {
            Debug.Log("WoolResource: Resource depleted.");
            Destroy(gameObject);
        }
    }
}