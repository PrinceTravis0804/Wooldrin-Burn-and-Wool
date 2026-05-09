using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// Final version for Wooldrin:
// - Keyboard movement (WASD)
// - Left Click: Move and Drop Wool (limited to 1 active)
// - Right Click: Command Spirit to Repel (Fire) - Only if Spirit is Ready
// - F Key: Kick Wool (only when touching)
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Abilities")]
    public GameObject woolPrefab;
    public float dropArrivalDistance = 0.3f;
    public float kickForce = 12f;
    public FireSpiritController spirit;

    [Header("State")]
    public bool canDropWool = false; // Unlocked by Sir Baa-lot
    public bool hasActiveWool = false;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;
    private GameObject currentWool;

    void Update()
    {
        // 1. Get Keyboard Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Cancel auto-move if player manually overrides with keys
        if (movement.magnitude > 0.1f) isAutoMoving = false;

        // 2. LEFT CLICK: Auto-move to drop Wool (Limit 1)
        if (Input.GetMouseButtonDown(0) && canDropWool && !hasActiveWool)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetLocation = new Vector2(mousePos.x, mousePos.y);
            isAutoMoving = true;
        }

        // 3. RIGHT CLICK: Command Spirit to drop Fire
        if (Input.GetMouseButtonDown(1))
        {
            // Only cast if the spirit is not already busy or returning
            if (spirit != null && spirit.isReady)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 fireTarget = new Vector3(mousePos.x, mousePos.y, 0);

                spirit.StartFireAction(fireTarget);
            }
            else if (spirit != null && !spirit.isReady)
            {
                Debug.Log("Spirit is currently busy or retrieving!");
            }
        }

        // 4. F KEY: Kick Wool
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryKickWool();
        }
    }

    void FixedUpdate()
    {
        if (isAutoMoving)
        {
            // Move Wooldrin toward the mouse-click location
            float dist = Vector2.Distance(rb.position, targetLocation);
            if (dist > dropArrivalDistance)
            {
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetLocation, moveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                // Target reached, drop the resource
                SpawnWool();
            }
        }
        else
        {
            // Standard manual WASD movement
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void SpawnWool()
    {
        if (woolPrefab != null)
        {
            currentWool = Instantiate(woolPrefab, transform.position, Quaternion.identity);
            hasActiveWool = true;
            isAutoMoving = false;
            rb.velocity = Vector2.zero; // Stop all momentum upon dropping
        }
    }

    void TryKickWool()
    {
        // Use a small circle overlap to check if Wooldrin is physically touching wool
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 0.8f);
        foreach (var obj in hit)
        {
            if (obj.CompareTag("Wool"))
            {
                Rigidbody2D woolRb = obj.GetComponent<Rigidbody2D>();
                if (woolRb != null)
                {
                    // Apply force from Wooldrin's position through the wool's center
                    Vector2 dir = (obj.transform.position - transform.position).normalized;
                    woolRb.AddForce(dir * kickForce, ForceMode2D.Impulse);
                    break; // Only kick one pile at a time
                }
            }
        }
    }

    // This is called by the WoolResource script or Fire Spirit when wool is gone
    public void NotifyWoolDestroyed()
    {
        hasActiveWool = false;
        currentWool = null;
        Debug.Log("Wool slot cleared. Ready to drop another!");
    }
}