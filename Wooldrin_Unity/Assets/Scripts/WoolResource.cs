using UnityEngine;

public class WoolResource : MonoBehaviour
{
    [Header("Health")]
    public float health = 100f;
    private float maxHealth;

    [Header("Kick Indicator")]
    public GameObject indicator;
    public float indicatorRange = 1.2f;
    public Vector3 indicatorOffset = new Vector3(0, 1.2f, 0);
    [Range(0f, 1f)]
    public float indicatorOpacity = 1.0f;

    private Vector3 initialScale;
    private PlayerController player;
    private Transform playerTransform;
    private SpriteRenderer indicatorSR;
    private Rigidbody2D rb;
    private int eatersCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Store the starting health so scaling is always relative to the max
        maxHealth = health;

        // CAPTURE whatever scale you set in the Transform component
        initialScale = transform.localScale;

        if (indicator != null)
        {
            indicatorSR = indicator.GetComponent<SpriteRenderer>();
            indicator.SetActive(false);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerController>();
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // Lock position if being eaten to prevent being pushed
        if (rb != null)
        {
            if (eatersCount > 0) rb.constraints = RigidbodyConstraints2D.FreezeAll;
            else rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (indicator != null && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
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

    public void TakeBite(float amount)
    {
        health -= amount;

        // Use maxHealth instead of hardcoded 100f to prevent "explosion" bugs
        float healthRatio = Mathf.Clamp01(health / maxHealth);
        transform.localScale = initialScale * healthRatio;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetEatingState(bool isEating)
    {
        if (isEating) eatersCount++;
        else eatersCount = Mathf.Max(0, eatersCount - 1);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.NotifyWoolDestroyed();
        }
    }
}