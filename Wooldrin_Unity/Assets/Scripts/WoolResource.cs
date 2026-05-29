using UnityEngine;

public class WoolResource : MonoBehaviour
{
    [Header("Health & Scaling")]
    public float health = 100f;
    private float maxHealth;
    private Vector3 initialScale;

    [Header("Indicator UI")]
    public GameObject indicator;
    public float indicatorRange = 1.2f;
    public Vector3 indicatorOffset = new Vector3(0, 1.2f, 0);
    [Range(0f, 1f)] public float indicatorOpacity = 1.0f;

    private Rigidbody2D rb;
    private SpriteRenderer indicatorSR;
    private Transform player;
    private int eatersCount = 0;

    void Start()
    {
        maxHealth = health;
        initialScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();

        if (indicator != null)
        {
            indicatorSR = indicator.GetComponent<SpriteRenderer>();
            indicator.SetActive(false);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Safe Unity null check (evaluates correctly for destroyed objects)
        if (indicator != null && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            bool inRange = dist <= indicatorRange;

            indicator.transform.localPosition = indicatorOffset;
            if (indicatorSR != null)
            {
                Color c = indicatorSR.color;
                c.a = indicatorOpacity;
                indicatorSR.color = c;
            }

            if (indicator.activeSelf != inRange) indicator.SetActive(inRange);
        }
    }

    public void SetEatingState(bool isEating)
    {
        if (isEating) eatersCount++;
        else eatersCount = Mathf.Max(0, eatersCount - 1);

        if (rb != null)
        {
            // Freeze position if being eaten so slimes can't push it
            rb.constraints = eatersCount > 0 ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void TakeBite(float amount)
    {
        health -= amount;
        float ratio = Mathf.Clamp01(health / maxHealth);
        transform.localScale = initialScale * ratio;

        if (health <= 0) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // CRITICAL FIX: The '?.' operator on Unity GameObjects/Transforms bypasses Unity's custom null check
        // because the C# wrapper object is technically not null even if the underlying engine object is destroyed.
        // We use standard Unity '!= null' checks to prevent MissingReferenceException during scene loads/reloads!
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.NotifyWoolDestroyed(gameObject);
            }
        }
    }
}