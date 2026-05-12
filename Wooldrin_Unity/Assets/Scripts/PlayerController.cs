using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Knockback Settings")]
    [Tooltip("How hard Wooldrin is pushed back. Increase this to increase the knockback distance.")]
    public float knockbackForce = 15f;
    [Tooltip("How long (in seconds) Wooldrin loses control after being hit. A shorter time makes the knockback feel 'snappier'.")]
    public float knockbackDuration = 0.2f;

    [Header("Abilities")]
    public GameObject woolPrefab;
    public float dropArrivalDistance = 0.6f;
    public float kickForce = 25f;
    public FireSpiritController spirit;

    [Header("State")]
    public bool canDropWool = true;
    public bool hasActiveWool = false;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;
    private Vector2 lastFacingDir = Vector2.down;
    private Animator animator;
    private WooldrinHealth health; // Reference for knockback check

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<WooldrinHealth>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Ensure physics doesn't rotate the lamb
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. Capture Manual Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Interrupt auto-moving if the player tries to move manually
        if (movement.sqrMagnitude > 0.01f) isAutoMoving = false;

        // 2. LEFT CLICK: Auto-move to location and Drop Wool
        if (Input.GetMouseButtonDown(0) && canDropWool && !hasActiveWool)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetLocation = new Vector2(mousePos.x, mousePos.y);
            isAutoMoving = true;
        }

        // 3. RIGHT CLICK: Fire Spirit Action
        if (Input.GetMouseButtonDown(1) && spirit != null && spirit.isReady)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spirit.StartFireAction(new Vector3(mousePos.x, mousePos.y, 0));
        }

        // 4. KICK WOOL
        if (Input.GetKeyDown(KeyCode.F)) TryKickWool();

        // 5. Update Animations
        UpdateAnims();
    }

    void FixedUpdate()
    {
        // --- CRITICAL CHECK: KNOCKBACK OVERRIDE ---
        // If Wooldrin is being hit, we skip all movement logic so physics takes over.
        if (health != null && health.isBeingKnockedBack)
        {
            isAutoMoving = false; // Cancel auto-movement on hit
            return;
        }

        if (isAutoMoving)
        {
            float dist = Vector2.Distance(rb.position, targetLocation);
            if (dist > dropArrivalDistance)
            {
                Vector2 nextStep = Vector2.MoveTowards(rb.position, targetLocation, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(nextStep);
            }
            else
            {
                SpawnWool();
            }
        }
        else
        {
            // Standard manual movement
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void SpawnWool()
    {
        if (woolPrefab != null)
        {
            Instantiate(woolPrefab, transform.position, Quaternion.identity);
            hasActiveWool = true;
        }
        isAutoMoving = false;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    void TryKickWool()
    {
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var obj in hit)
        {
            if (obj.CompareTag("Wool"))
            {
                Rigidbody2D woolRb = obj.GetComponent<Rigidbody2D>();
                if (woolRb != null)
                {
                    Vector2 dir = (obj.transform.position - transform.position).normalized;
                    woolRb.velocity = Vector2.zero;
                    woolRb.AddForce(dir * kickForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    public void NotifyWoolDestroyed()
    {
        hasActiveWool = false;
        Debug.Log("Wooldrin: Wool slot is now FREE.");
    }

    void UpdateAnims()
    {
        if (animator == null) return;

        // Determine which direction we are actually traveling
        Vector2 currentDir = isAutoMoving ? (targetLocation - rb.position).normalized : movement;
        float currentSpeed = isAutoMoving ? 1f : movement.sqrMagnitude;

        if (currentDir.sqrMagnitude > 0.01f)
        {
            lastFacingDir = currentDir.normalized;
            animator.SetFloat("moveX", lastFacingDir.x);
            animator.SetFloat("moveY", lastFacingDir.y);
        }
        else
        {
            // Facing direction while idle
            animator.SetFloat("moveX", lastFacingDir.x);
            animator.SetFloat("moveY", lastFacingDir.y);
        }

        animator.SetFloat("speed", currentSpeed);
    }
}