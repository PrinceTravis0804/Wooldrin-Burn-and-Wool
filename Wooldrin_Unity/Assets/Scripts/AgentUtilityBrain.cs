using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float detectionRadius = 5f;
    public float moveSpeed = 2f;

    [Header("Utility Weights")]
    public float attractionWeight = 10f; // Scent of Wool
    public float aversionWeight = 15f;   // Heat/Fire

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the gravity is zero for top-down physics
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        SimulateDecision();
    }

    void SimulateDecision()
    {
        // 1. Sense the environment
        Collider2D[] sensedObjects = Physics2D.OverlapCircleAll(rb.position, detectionRadius);

        Vector2 aversionForce = Vector2.zero;
        Vector2 attractionForce = Vector2.zero;
        int fireCount = 0;
        int woolCount = 0;

        foreach (var obj in sensedObjects)
        {
            float distance = Vector2.Distance(rb.position, obj.transform.position);

            // Prevent division by zero or jittering when too close
            if (distance <= 0.2f) continue;

            Vector2 directionToObj = ((Vector2)obj.transform.position - rb.position).normalized;

            if (obj.CompareTag("Fire"))
            {
                // Inverse Square Law: Force = Weight / Distance
                aversionForce -= directionToObj * (aversionWeight / distance);
                fireCount++;
            }
            else if (obj.CompareTag("Wool"))
            {
                attractionForce += directionToObj * (attractionWeight / distance);
                woolCount++;
            }
        }

        // 2. State Logic & Visual Feedback
        // We calculate the state AFTER the loop so the Priority (Fire) always wins
        if (fireCount > 0)
        {
            spriteRenderer.color = Color.red; // Panic State
            MoveAgent(aversionForce, moveSpeed * 1.5f); // Escape is faster than grazing
        }
        else if (woolCount > 0)
        {
            spriteRenderer.color = Color.green; // Lure State
            MoveAgent(attractionForce, moveSpeed);
        }
        else
        {
            spriteRenderer.color = Color.white; // Neutral State
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f); // Smooth stop
        }
    }

    void MoveAgent(Vector2 force, float speed)
    {
        // Calculate the next position using physics-friendly MovePosition
        Vector2 newPos = rb.position + force.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    // This is for your class presentation—it shows the "Senses" in the Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f); // Transparent Yellow
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}