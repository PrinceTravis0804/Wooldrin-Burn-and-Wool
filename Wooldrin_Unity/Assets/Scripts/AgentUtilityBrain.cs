using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 2.5f;
    public float fearSpeedMultiplier = 2.5f; // Slimes run much faster when afraid
    public float detectionRadius = 6f;
    public float biteStrength = 25f;

    [Header("Utility Weights")]
    public float attractionWeight = 10f;
    public float aversionWeight = 30f;   // Increased weight for stronger fear response
    public float playerWeight = 5f;

    [Header("Roaming Settings")]
    public float wanderRadius = 4f;
    public float wanderTimer = 3f;
    public float waitProbability = 0.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private Vector2 wanderTarget;
    private float currentWanderTime;
    private float currentWaitTime;
    private bool isWaiting = false;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isEating = false;
    private Vector2 lastMoveDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        PickNewWanderPoint();
    }

    void FixedUpdate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Vector2 totalForce = Vector2.zero;
        bool fireNearby = false;
        bool playerVisible = false;
        bool woolNearby = false;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < 0.1f) continue;
            Vector2 dir = (hit.transform.position - transform.position).normalized;

            if (hit.CompareTag("Fire"))
            {
                // Summing fear: Move AWAY from all fire sources
                totalForce -= dir * (aversionWeight / dist);
                fireNearby = true;
            }
            else if (hit.CompareTag("Wool"))
            {
                totalForce += dir * (attractionWeight / dist);
                woolNearby = true;
            }
            else if (hit.CompareTag("Player"))
            {
                if (HasLineOfSight(hit.transform.position))
                {
                    totalForce += dir * (playerWeight / dist);
                    playerVisible = true;
                }
            }
        }

        // --- HIERARCHY OF NEEDS & COLOR LOGIC ---
        if (fireNearby)
        {
            isEating = false;
            isWaiting = false;
            sr.color = Color.red; // CHANGE TO RED WHEN AFRAID
            // Panic movement: Fast and in the calculated escape direction
            rb.velocity = totalForce.normalized * (moveSpeed * fearSpeedMultiplier);
        }
        else if (isEating || woolNearby)
        {
            sr.color = Color.green; // CHANGE TO GREEN WHEN INTERESTED IN WOOL
            if (isEating)
            {
                rb.velocity = Vector2.zero;
            }
            else
            {
                isWaiting = false;
                rb.velocity = totalForce.normalized * moveSpeed;
            }
        }
        else if (playerVisible)
        {
            sr.color = new Color(1, 0.5f, 0); // Orange tint for hunting
            isWaiting = false;
            rb.velocity = totalForce.normalized * moveSpeed;
        }
        else
        {
            sr.color = Color.white; // RESET COLOR WHEN NEUTRAL
            Roam();
        }

        UpdateAnimationParameters();
    }

    void UpdateAnimationParameters()
    {
        if (animator == null) return;
        float currentSpeed = rb.velocity.magnitude;
        if (currentSpeed > 0.1f)
        {
            lastMoveDir = rb.velocity.normalized;
            animator.SetFloat("moveX", lastMoveDir.x);
            animator.SetFloat("moveY", lastMoveDir.y);
        }
        animator.SetFloat("speed", currentSpeed);
    }

    public void TakeDamage()
    {
        if (animator != null) animator.SetTrigger("isHurt");
    }

    bool HasLineOfSight(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, target);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, LayerMask.GetMask("Walls"));
        return hit.collider == null;
    }

    void Roam()
    {
        if (isWaiting)
        {
            rb.velocity = Vector2.zero;
            currentWaitTime -= Time.fixedDeltaTime;
            if (currentWaitTime <= 0)
            {
                isWaiting = false;
                PickNewWanderPoint();
            }
        }
        else
        {
            currentWanderTime += Time.fixedDeltaTime;
            float distToTarget = Vector2.Distance(transform.position, wanderTarget);
            if (currentWanderTime >= wanderTimer || distToTarget < 0.3f)
            {
                if (Random.value < waitProbability)
                {
                    isWaiting = true;
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                }
                else
                {
                    PickNewWanderPoint();
                }
            }
            rb.velocity = (wanderTarget - (Vector2)transform.position).normalized * (moveSpeed * 0.5f);
        }
    }

    void PickNewWanderPoint()
    {
        wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
        currentWanderTime = 0;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool"))
        {
            isEating = true;
            collision.gameObject.GetComponent<WoolResource>()?.TakeBite(biteStrength * Time.deltaTime);
        }
        // FIXED: Added explicit damage call for the player
        else if (collision.gameObject.CompareTag("Player"))
        {
            WooldrinHealth health = collision.gameObject.GetComponent<WooldrinHealth>();
            if (health != null)
            {
                // Call damage logic
                health.TakeDamage(transform.position);

                // Briefly stop the slime's velocity so it doesn't just "push" the player
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool")) isEating = false;
    }
}