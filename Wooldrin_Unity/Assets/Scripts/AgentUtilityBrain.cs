using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Eating & Damage Logic")]
    public float biteStrength = 20f;   // The variable the error was missing!
    public float damageCooldown = 1.0f; // Prevent instant death

    [Header("Simulation Settings")]
    public float detectionRadius = 5f;
    public float moveSpeed = 2f;

    [Header("Utility Weights")]
    public float attractionWeight = 10f; // Wool
    public float aversionWeight = 15f;   // Fire
    public float playerWeight = 8f;      // Wooldrin

    [Header("Roaming Settings")]
    public float wanderRadius = 3f;
    public float wanderInterval = 2f;
    private Vector2 wanderTarget;
    private float wanderTimer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isEating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.drag = 5f; // Helps stop moving when eating
        PickNewWanderTarget();
    }

    void FixedUpdate()
    {
        SimulateDecision();
    }

    void SimulateDecision()
    {
        Collider2D[] sensedObjects = Physics2D.OverlapCircleAll(rb.position, detectionRadius);

        Vector2 aversionForce = Vector2.zero;
        Vector2 woolForce = Vector2.zero;
        Vector2 playerForce = Vector2.zero;

        bool fireFound = false;
        bool woolFound = false;
        bool playerFound = false;

        foreach (var obj in sensedObjects)
        {
            float dist = Vector2.Distance(rb.position, obj.transform.position);
            if (dist < 0.2f) continue;
            Vector2 dir = ((Vector2)obj.transform.position - rb.position).normalized;

            if (obj.CompareTag("Fire")) { aversionForce -= dir * (aversionWeight / dist); fireFound = true; }
            else if (obj.CompareTag("Wool")) { woolForce += dir * (attractionWeight / dist); woolFound = true; }
            else if (obj.CompareTag("Player")) { playerForce += dir * (playerWeight / dist); playerFound = true; }
        }

        // --- HIERARCHY OF NEEDS ---
        if (fireFound)
        {
            isEating = false;
            spriteRenderer.color = Color.red; // Panic
            rb.velocity = aversionForce.normalized * (moveSpeed * 1.5f);
        }
        else if (isEating)
        {
            spriteRenderer.color = Color.green;
            rb.velocity = Vector2.zero; // Stop to eat
        }
        else if (woolFound)
        {
            spriteRenderer.color = Color.green;
            rb.velocity = woolForce.normalized * moveSpeed;
        }
        else if (playerFound)
        {
            spriteRenderer.color = new Color(1, 0.5f, 0); // Orange: Pursuit
            rb.velocity = playerForce.normalized * (moveSpeed * 1.2f);
        }
        else
        {
            spriteRenderer.color = Color.white;
            Roam();
        }
    }

    void Roam()
    {
        wanderTimer += Time.fixedDeltaTime;
        if (wanderTimer >= wanderInterval || Vector2.Distance(rb.position, wanderTarget) < 0.5f) PickNewWanderTarget();
        rb.velocity = (wanderTarget - rb.position).normalized * (moveSpeed * 0.5f);
    }

    void PickNewWanderTarget() { wanderTarget = rb.position + Random.insideUnitCircle * wanderRadius; wanderTimer = 0; }

    // --- COLLISION LOGIC (WHERE DAMAGE HAPPENS) ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool"))
        {
            WoolResource wool = collision.gameObject.GetComponent<WoolResource>();
            if (wool != null)
            {
                isEating = true;
                wool.TakeBite(biteStrength * Time.deltaTime);
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            WooldrinHealth health = collision.gameObject.GetComponent<WooldrinHealth>();
            if (health != null)
            {
                health.TakeDamage(); // This triggers the shrinking

                // Push back so it doesn't stay stuck on the player
                Vector2 pushDir = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushDir * 5f, ForceMode2D.Impulse);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool")) isEating = false;
    }
}