using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Eating & Damage Logic")]
    public float biteStrength = 20f;   // The variable the error was missing!
    public float damageCooldown = 1.0f; // Prevent instant death
    public float attackRange = 1.8f;    // INCREASED: Buffer zone for pushback
    private float lastDamageTime;       // Timer to track cooldown

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
    private Animator anim; // Para sa animations ng TestMOB
    private bool isEating = false;
    private Transform currentWoolTarget; // Track the specific wool being eaten

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); // Automatically gets the Animator component
        
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.drag = 5f; // Helps stop moving when eating
        PickNewWanderTarget();
    }

    void Update()
    {
        // Handle Sprite Flipping based on movement direction
        if (rb.velocity.x > 0.1f) spriteRenderer.flipX = false;
        else if (rb.velocity.x < -0.1f) spriteRenderer.flipX = true;

        // Update Walk Animation state
        bool isMoving = rb.velocity.magnitude > 0.1f;
        anim.SetBool("isWalking", isMoving);
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

        // Track the closest target for the Attack Animation
        GameObject closestTarget = null;
        float minTargetDist = Mathf.Infinity;

        foreach (var obj in sensedObjects)
        {
            float dist = Vector2.Distance(rb.position, obj.transform.position);
            if (dist < 0.2f) continue;
            Vector2 dir = ((Vector2)obj.transform.position - rb.position).normalized;

            if (obj.CompareTag("Fire")) { aversionForce -= dir * (aversionWeight / dist); fireFound = true; }
            else if (obj.CompareTag("Wool")) 
            { 
                woolForce += dir * (attractionWeight / dist); 
                woolFound = true; 
                if (dist < minTargetDist) { minTargetDist = dist; closestTarget = obj.gameObject; }
            }
            else if (obj.CompareTag("Player")) 
            { 
                playerForce += dir * (playerWeight / dist); 
                playerFound = true; 
                if (dist < minTargetDist) { minTargetDist = dist; closestTarget = obj.gameObject; }
            }
        }

        // --- ANIMATION LOGIC: Check if close enough to attack ---
        if (closestTarget != null && minTargetDist <= attackRange && !fireFound)
        {
            anim.SetBool("isAttacking", true);
            
            // If we are eating wool, apply a tiny "Lean In" force to keep collision active
            if (isEating && currentWoolTarget != null)
            {
                Vector2 biteDir = ((Vector2)currentWoolTarget.position - rb.position).normalized;
                rb.velocity = biteDir * 0.5f; 
            }
            else
            {
                rb.velocity = Vector2.zero; // Stop moving to play attack animation properly
            }
        }
        else
        {
            // If the Trigger hasn't forced it to true, set to false
            anim.SetBool("isAttacking", false);

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
    }

    void Roam()
    {
        wanderTimer += Time.fixedDeltaTime;
        if (wanderTimer >= wanderInterval || Vector2.Distance(rb.position, wanderTarget) < 0.5f) PickNewWanderTarget();
        rb.velocity = (wanderTarget - rb.position).normalized * (moveSpeed * 0.5f);
    }

    void PickNewWanderTarget() { wanderTarget = rb.position + Random.insideUnitCircle * wanderRadius; wanderTimer = 0; }

    // --- IMMEDIATE ATTACK LOGIC (REFLEXES) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        // High-speed detection: If the player or wool enters the 'Bite Zone'
        if (other.CompareTag("Player") || other.CompareTag("Wool"))
        {
            anim.SetBool("isAttacking", true);
            rb.velocity = Vector2.zero; // Stop instantly

            if (other.CompareTag("Player"))
            {
                HandlePlayerDamage(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // If the target leaves the bite zone, stop attacking immediately
        if (other.CompareTag("Player") || other.CompareTag("Wool"))
        {
            anim.SetBool("isAttacking", false);
        }
    }

    // --- COLLISION LOGIC (WHERE DAMAGE HAPPENS) ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool"))
        {
            WoolResource wool = collision.gameObject.GetComponent<WoolResource>();
            if (wool != null)
            {
                isEating = true;
                currentWoolTarget = collision.transform; 
                wool.TakeBite(biteStrength * Time.deltaTime);
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerDamage(collision.gameObject);
        }
    }

    private void HandlePlayerDamage(GameObject victim)
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            WooldrinHealth health = victim.GetComponent<WooldrinHealth>();
            if (health != null)
            {
                health.TakeDamage(); 
                lastDamageTime = Time.time;

                // REDUCED PUSHBACK: Changing from 5f to 2f helps keep them in range for the next bite
                Vector2 pushDir = (transform.position - victim.transform.position).normalized;
                rb.AddForce(pushDir * 2f, ForceMode2D.Impulse);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wool")) 
        {
            isEating = false;
            currentWoolTarget = null;
        }
    }
}