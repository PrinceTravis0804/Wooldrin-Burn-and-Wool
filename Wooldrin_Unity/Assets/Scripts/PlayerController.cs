using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

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

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement.magnitude > 0.1f) isAutoMoving = false;

        // LEFT CLICK: Move and Drop
        if (Input.GetMouseButtonDown(0) && canDropWool && !hasActiveWool)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetLocation = new Vector2(mousePos.x, mousePos.y);
            isAutoMoving = true;
        }

        // RIGHT CLICK: Fire Spirit Action
        if (Input.GetMouseButtonDown(1) && spirit != null && spirit.isReady)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spirit.StartFireAction(new Vector3(mousePos.x, mousePos.y, 0));
        }

        if (Input.GetKeyDown(KeyCode.F)) TryKickWool();

        UpdateAnims();
    }

    void FixedUpdate()
    {
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
        Instantiate(woolPrefab, transform.position, Quaternion.identity);
        hasActiveWool = true;
        isAutoMoving = false;
        rb.velocity = Vector2.zero;
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

    // Safer reset logic
    public void NotifyWoolDestroyed()
    {
        hasActiveWool = false;
        Debug.Log("Wooldrin: Wool slot is now FREE.");
    }

    void UpdateAnims()
    {
        if (animator == null) return;
        Vector2 currentVel = isAutoMoving ? (targetLocation - rb.position).normalized : movement;
        float speed = isAutoMoving ? 1f : movement.magnitude;
        if (currentVel.magnitude > 0.1f)
        {
            lastFacingDir = currentVel.normalized;
            animator.SetFloat("moveX", lastFacingDir.x);
            animator.SetFloat("moveY", lastFacingDir.y);
        }
        animator.SetFloat("speed", speed);
    }
}