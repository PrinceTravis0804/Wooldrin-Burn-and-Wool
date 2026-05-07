using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Combat & Consumption")]
    public float biteStrength = 25f; // Damage per second to wool or player
    public float damageCooldown = 1.0f;
    public float attackRange = 1.6f;
    public float playerKnockbackForce = 12f;
    private float lastDamageTime;

    [Header("Cinemachine")]
    private CinemachineImpulseSource impulseSource;

    [Header("Senses")]
    public float detectionRadius = 6f; // Slightly increased for better detection
    public float moveSpeed = 2.5f;
    public LayerMask wallLayer;

    [Header("Utility Weights")]
    public float attractionWeight = 15f; // Wool (Slightly higher than player to prioritize food)
    public float aversionWeight = 20f;    // Fire
    public float playerWeight = 8f;      // Wooldrin

    [Header("Roaming")]
    public float wanderRadius = 3f;
    public float wanderInterval = 3f;
    private Vector2 wanderTarget;
    private float wanderTimer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isEating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.drag = 5f;

        PickNewWanderTarget();
    }

    void Update()
    {
        // Sprite Flipping
        if (rb.velocity.x > 0.1f) spriteRenderer.flipX = false;
        else if (rb.velocity.x < -0.1f) spriteRenderer.flipX = true;

        if (anim != null)
        {
            anim.SetBool("isWalking", rb.velocity.magnitude > 0.1f);
        }

        // Color feedback for states
        if (isEating) spriteRenderer.color = Color.green;
        else if (rb.velocity.magnitude > moveSpeed) spriteRenderer.color = Color.red; // Panicking
        else spriteRenderer.color = Color.white;
    }

    void FixedUpdate() => SimulateDecision();

    void SimulateDecision()
    {
        Collider2D[] sensedObjects = Physics2D.OverlapCircleAll(rb.position, detectionRadius);
        Vector2 aversionForce = Vector2.zero;
        Vector2 attractionForce = Vector2.zero;
        bool fireFound = false;
        GameObject closestTarget = null;
        float minTargetDist = Mathf.Infinity;

        foreach (var obj in sensedObjects)
        {
            float dist = Vector2.Distance(rb.position, obj.transform.position);
            if (dist < 0.1f) continue;

            // Line of Sight check
            RaycastHit2D wallCheck = Physics2D.Linecast(rb.position, obj.transform.position, wallLayer);
            if (wallCheck.collider != null && wallCheck.transform != obj.transform) continue;

            Vector2 dir = ((Vector2)obj.transform.position - rb.position).normalized;

            if (obj.CompareTag("Fire"))
            {
                aversionForce -= dir * (aversionWeight / (dist * dist));
                fireFound = true;
            }
            else if (obj.CompareTag("Wool") || obj.CompareTag("Player"))
            {
                float weight = obj.CompareTag("Wool") ? attractionWeight : playerWeight;
                attractionForce += dir * (weight / dist);

                if (dist < minTargetDist)
                {
                    minTargetDist = dist;
                    closestTarget = obj.gameObject;
                }
            }
        }

        // --- BRAIN OUTPUT ---
        if (fireFound)
        {
            isEating = false;
            if (anim != null) anim.SetBool("isAttacking", false);
            rb.velocity = aversionForce.normalized * (moveSpeed * 1.5f);
        }
        else if (closestTarget != null && minTargetDist <= attackRange)
        {
            // STOP TO EAT/ATTACK
            rb.velocity = Vector2.zero;
            if (anim != null) anim.SetBool("isAttacking", true);

            if (closestTarget.CompareTag("Player"))
            {
                isEating = false;
                HandlePlayerDamage(closestTarget);
            }
            else if (closestTarget.CompareTag("Wool"))
            {
                isEating = true;
                HandleWoolEating(closestTarget);
            }
        }
        else
        {
            isEating = false;
            if (anim != null) anim.SetBool("isAttacking", false);

            if (attractionForce != Vector2.zero)
            {
                rb.velocity = attractionForce.normalized * moveSpeed;
            }
            else
            {
                Roam();
            }
        }
    }

    void Roam()
    {
        wanderTimer += Time.fixedDeltaTime;
        if (wanderTimer >= wanderInterval || Vector2.Distance(rb.position, wanderTarget) < 0.5f)
            PickNewWanderTarget();
        rb.velocity = (wanderTarget - rb.position).normalized * (moveSpeed * 0.5f);
    }

    void PickNewWanderTarget()
    {
        wanderTarget = rb.position + Random.insideUnitCircle * wanderRadius;
        wanderTimer = 0;
    }

    private void HandleWoolEating(GameObject woolObj)
    {
        WoolResource wool = woolObj.GetComponent<WoolResource>();
        if (wool != null)
        {
            // Damage the wool over time
            wool.TakeBite(biteStrength * Time.deltaTime);
        }
    }

    private void HandlePlayerDamage(GameObject victim)
    {
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            WooldrinHealth healthScript = victim.GetComponent<WooldrinHealth>();
            Rigidbody2D victimRb = victim.GetComponent<Rigidbody2D>();

            if (healthScript != null && healthScript.TakeDamage())
            {
                lastDamageTime = Time.time;

                // 1. Knockback
                if (victimRb != null)
                {
                    Vector2 knockbackDir = (victim.transform.position - transform.position).normalized;
                    victimRb.AddForce(knockbackDir * playerKnockbackForce, ForceMode2D.Impulse);
                }

                // 2. Camera Shake
                if (CameraShakeManager.instance != null && impulseSource != null)
                {
                    CameraShakeManager.instance.CameraShake(impulseSource);
                }

                // 3. Enemy recoil
                rb.AddForce(-((Vector2)victim.transform.position - rb.position).normalized * 4f, ForceMode2D.Impulse);
            }
        }
    }
}