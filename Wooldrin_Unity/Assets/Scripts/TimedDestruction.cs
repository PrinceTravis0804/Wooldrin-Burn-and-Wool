using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestruction : MonoBehaviour
{
    public float lifetime = 5f;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        timer = lifetime;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Visual Feedback: Fade out as the timer runs out
        if (spriteRenderer != null)
        {
            float alpha = timer / lifetime;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // Remove from simulation when time is up
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
