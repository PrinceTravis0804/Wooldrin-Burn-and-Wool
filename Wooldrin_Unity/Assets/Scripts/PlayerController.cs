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

    [Header("State")]
    public bool canDropWool = true;
    public bool hasActiveWool = false;

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
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.sqrMagnitude > 0.01f) isAutoMoving = false;

        if (Input.GetMouseButtonDown(0) && canDropWool && !hasActiveWool)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetLocation = new Vector2(mousePos.x, mousePos.y);
            isAutoMoving = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (spirit == null) spirit = FindObjectOfType<FireSpiritController>();
            if (spirit != null && spirit.isReady)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spirit.StartFireAction(new Vector3(mousePos.x, mousePos.y, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.F)) TryKickWool();

        UpdateAnims();
    }

    void FixedUpdate()
    {
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

    void SpawnWool()
    {
        if (woolPrefab != null)
        {
            Instantiate(woolPrefab, transform.position, Quaternion.identity);
            hasActiveWool = true;

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

    public void NotifyWoolDestroyed() { hasActiveWool = false; }

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