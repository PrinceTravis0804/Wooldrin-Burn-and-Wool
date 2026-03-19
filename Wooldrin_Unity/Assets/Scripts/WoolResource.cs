using UnityEngine;

public class WoolResource : MonoBehaviour
{
    public float health = 100f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        // Check if the tag is correct
        if (!gameObject.CompareTag("Wool"))
        {
            Debug.LogError("CRITICAL: This wool object is NOT tagged 'Wool'! The Agent won't see it.");
        }
    }

    public void TakeBite(float damage)
    {
        Debug.Log("MUNCH! Wool is being eaten. Current Health: " + health);
        health -= damage;

        float t = health / 100f;
        transform.localScale = originalScale * t;

        if (health <= 0)
        {
            Debug.Log("Wool is gone!");
            Destroy(gameObject);
        }
    }
}