using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Knockback Settings")]
    public float knockbackForce = 15f;
    public float knockbackDuration = 0.2f;

    [Header("Abilities")]
    public GameObject woolPrefab;
    public float dropArrivalDistance = 0.6f;
    public float kickForce = 25f;
    public FireSpiritController spirit;

    [Header("Ability Cooldowns & Charges")]
    [Tooltip("Maximum decoy wool charges Wooldrin can hold.")]
    public int maxWoolCharges = 2;
    [Tooltip("How long in seconds it takes to recover a single wool charge (Regeneration Speed). Lower is faster!")]
    public float woolRechargeDuration = 5f;
    [Space(5)]
    [Tooltip("Maximum charges the Fire Spirit can hold.")]
    public int maxFireCharges = 3;
    [Tooltip("How long in seconds it takes to regenerate a single Fire Spirit charge (Regeneration Speed). Lower is faster!")]
    public float fireRechargeDuration = 5f;

    [Header("State")]
    public bool canDropWool = true;
    public bool hasActiveWool => activeWools.Count > 0;

    // Cooldown, Charge, and Lockout Trackers
    private int currentWoolCharges = 2;
    private float woolRechargeTimer = 0f;
    private bool isWoolLockedOut = false;

    private int currentFireCharges = 3;
    private float fireRechargeTimer = 0f;
    private bool isFireLockedOut = false;

    // Track spawned decoys currently active in the scene
    private List<GameObject> activeWools = new List<GameObject>();

    // Public properties for the HUD and other scripts to access
    public int CurrentWoolCharges => currentWoolCharges;
    public int MaxWoolCharges => maxWoolCharges;
    public float WoolRechargeTimer => woolRechargeTimer;
    public float WoolRechargeDuration => woolRechargeDuration;
    public bool IsWoolLockedOut => isWoolLockedOut;

    public int CurrentFireCharges => currentFireCharges;
    public int MaxFireCharges => maxFireCharges;
    public float FireRechargeTimer => fireRechargeTimer;
    public float FireRechargeDuration => fireRechargeDuration;
    public bool IsFireLockedOut => isFireLockedOut;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;
    private Vector2 lastFacingDir = Vector2.down;
    private Animator animator;
    private WooldrinHealth health;
    private PlaySound soundController;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<WooldrinHealth>();
        soundController = GetComponentInChildren<PlaySound>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.freezeRotation = true;

        // Initialize at full charges
        currentWoolCharges = maxWoolCharges;
        currentFireCharges = maxFireCharges;
        isWoolLockedOut = false;
        isFireLockedOut = false;
    }

    void Update()
    {
        // Clean up any destroyed/eaten wool references from our active list
        activeWools.RemoveAll(item => item == null);

        // --- DIALOGUE FREEZE CHECK ---
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            movement = Vector2.zero;
            isAutoMoving = false;
            UpdateAnims();
            return;
        }

        // --- UPDATE COOLDOWNS & RECHARGES ---
        HandleCooldownRecharges();

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 0.01f) isAutoMoving = false;

        // Left Click: Drop Decoy Wool (Guarded by Charges + Lockout checks)
        if (Input.GetMouseButtonDown(0) && canDropWool && currentWoolCharges > 0 && !isWoolLockedOut)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetLocation = new Vector2(mousePos.x, mousePos.y);
            isAutoMoving = true;
        }

        // Right Click: Command Fire Spirit (Guarded by Charges + Lockout checks + Readiness Check)
        if (Input.GetMouseButtonDown(1))
        {
            if (spirit == null) spirit = FindObjectOfType<FireSpiritController>();
            if (spirit != null && spirit.isReady && currentFireCharges > 0 && !isFireLockedOut)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spirit.StartFireAction(new Vector3(mousePos.x, mousePos.y, 0));

                // Spend 1 Fire Charge
                currentFireCharges--;

                // Lockout triggers ONLY when you run out of charges (0)
                if (currentFireCharges == 0)
                {
                    isFireLockedOut = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F)) TryKickWool();

        UpdateAnims();
    }

    void FixedUpdate()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (health != null && health.isBeingKnockedBack)
        {
            isAutoMoving = false;
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
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleCooldownRecharges()
    {
        // 1. Process Wool charge recovery
        if (currentWoolCharges < maxWoolCharges)
        {
            woolRechargeTimer += Time.deltaTime;
            if (woolRechargeTimer >= woolRechargeDuration)
            {
                currentWoolCharges++;
                woolRechargeTimer = 0f; // Reset timer for the next charge
            }
        }
        else
        {
            woolRechargeTimer = 0f;
        }

        // Check if Wool is fully replenished to release the lockout
        if (currentWoolCharges >= maxWoolCharges)
        {
            isWoolLockedOut = false;
        }
        else if (currentWoolCharges == 0)
        {
            isWoolLockedOut = true;
        }

        // 2. Process Fire Spirit charge recovery
        if (currentFireCharges < maxFireCharges)
        {
            fireRechargeTimer += Time.deltaTime;
            if (fireRechargeTimer >= fireRechargeDuration)
            {
                currentFireCharges++;
                fireRechargeTimer = 0f; // Reset timer for the next charge
            }
        }
        else
        {
            fireRechargeTimer = 0f;
        }

        // Check if Fire is fully replenished to release the lockout
        if (currentFireCharges >= maxFireCharges)
        {
            isFireLockedOut = false;
        }
        else if (currentFireCharges == 0)
        {
            isFireLockedOut = true;
        }
    }

    void SpawnWool()
    {
        if (woolPrefab != null && currentWoolCharges > 0 && !isWoolLockedOut)
        {
            GameObject newWool = Instantiate(woolPrefab, transform.position, Quaternion.identity);
            activeWools.Add(newWool);

            // Consume 1 wool charge
            currentWoolCharges--;

            // Lockout triggers ONLY when you run out of charges (0)
            if (currentWoolCharges == 0)
            {
                isWoolLockedOut = true;
            }

            if (soundController != null)
            {
                soundController.PlayWoolPlacementSound();
            }
            else
            {
                soundController = GetComponentInChildren<PlaySound>();
                if (soundController != null) soundController.PlayWoolPlacementSound();
            }
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

    public void NotifyWoolDestroyed(GameObject woolObj)
    {
        if (activeWools.Contains(woolObj))
        {
            activeWools.Remove(woolObj);
        }
    }

    void UpdateAnims()
    {
        if (animator == null) return;
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
            animator.SetFloat("moveX", lastFacingDir.x);
            animator.SetFloat("moveY", lastFacingDir.y);
        }
        animator.SetFloat("speed", currentSpeed);
    }
}