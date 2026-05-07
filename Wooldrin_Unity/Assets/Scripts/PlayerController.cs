using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [Tooltip("How much control you have while staggering from a hit (0 = none, 1 = full)")]
    public float controlDuringHit = 0.2f;

    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private WooldrinHealth health; // Reference to our health script

    [Header("Spirit and Abilities")]
    public FireSpiritController spirit;
    public GameObject woolPrefab;
    public bool canDropWool = true;
    public float dropDistance = 0.10f;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;
    private bool lastFlipState = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        health = GetComponent<WooldrinHealth>();

        // IMPORTANT for Physics: Set high drag so he doesn't slide forever like on ice
        if (rb != null)
        {
            rb.drag = 5f;
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 1. Get Manual Keyboard Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Cancel auto-move if player touches WASD
        if (movement.magnitude > 0.1f && isAutoMoving)
        {
            isAutoMoving = false;
            if (rb != null) rb.velocity = Vector2.zero;
            Debug.Log("Lure placement cancelled.");
        }

        // LEFT CLICK: Drop Wool
        if (Input.GetMouseButtonDown(0) && canDropWool)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            targetLocation = Camera.main.ScreenToWorldPoint(mousePos);
            isAutoMoving = true;
        }

        // RIGHT CLICK: Command Spirit
        if (Input.GetMouseButtonDown(1))
        {
            if (spirit != null)
            {
                Vector3 fireTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                fireTarget.z = 0;
                spirit.StartFireAction(fireTarget);
            }
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Use manual movement for animation parameters
        animator.SetFloat("MoveX", Mathf.Abs(movement.x));
        animator.SetFloat("MoveY", movement.y);
        animator.SetFloat("speed", isAutoMoving ? 1f : movement.sqrMagnitude);

        // Sticky Flip logic
        if (Mathf.Abs(movement.x) > 0.1f)
        {
            bool shouldFlip = (movement.x < -0.1f);
            spriteRenderer.flipX = shouldFlip;
            lastFlipState = shouldFlip;
        }
        else if (Mathf.Abs(movement.y) > 0.1f)
        {
            spriteRenderer.flipX = lastFlipState;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // PHYSICS FIX: Determine the speed based on health state
        float currentSpeed = moveSpeed;

        // Check if Wooldrin is currently staggering from a hit
        if (health != null && health.IsInvulnerable)
        {
            currentSpeed *= controlDuringHit;
        }

        if (isAutoMoving)
        {
            Vector2 directionToTarget = (targetLocation - rb.position).normalized;
            Vector2 stopPoint = targetLocation - (directionToTarget * dropDistance);
            float distanceToStop = Vector2.Distance(rb.position, stopPoint);

            if (distanceToStop > 0.1f)
            {
                rb.velocity = directionToTarget * currentSpeed;
            }
            else
            {
                Instantiate(woolPrefab, targetLocation, Quaternion.identity);
                isAutoMoving = false;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // PHYSICAL MOVEMENT: Instead of MovePosition, we set velocity.
            // This allows AddForce (Knockback) from the enemy script to actually work.
            Vector2 targetVelocity = movement.normalized * currentSpeed;

            if (movement.magnitude > 0.1f)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.2f);
            }
            // If no input, let 'Drag' handle the stopping smoothly
        }
    }
}