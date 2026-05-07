using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    [Tooltip("How many seconds the fire stays on the ground before vanishing.")]
    public float lifetime = 5f;

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
            // Gradually lower alpha as it nears death
            Color c = spriteRenderer.color;
            // Fades out linearly over the total lifetime
            c.a = Mathf.Clamp01(timer / lifetime);
            spriteRenderer.color = c;
        }

        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}