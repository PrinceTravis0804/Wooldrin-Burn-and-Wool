using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    [Tooltip("How many seconds the object stays before vanishing.")]
    public float lifetime = 15f;

    [Header("Optional Fade Out")]
    public bool fadeOut = true;
    private SpriteRenderer spriteRenderer;
    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = lifetime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (fadeOut && spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            // Fades out linearly based on the remaining lifetime
            float alpha = Mathf.Clamp01(timer / lifetime);
            c.a = alpha;
            spriteRenderer.color = c;
        }

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}