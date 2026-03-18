using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Ability References")]
    public GameObject woolPrefab;
    public float dropDistance = 0.2f; // Snappier arrival

    [Header("Spirit Connection")]
    public FireSpiritController spirit;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;

    [Header("State")]
    public bool canDropWool = false;

    void Update()
    {
        // 1. Manual WASD Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. Cancel auto-move if player actually presses a key
        if (movement.magnitude > 0.1f)
        {
            isAutoMoving = false;
        }

        // 3. LEFT CLICK: Drop Wool
        if (Input.GetMouseButtonDown(0) && canDropWool)
        {
            // Fix the Z-offset for 2D screens
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePos);
            targetLocation = new Vector2(worldPoint.x, worldPoint.y);

            isAutoMoving = true;

            // Visual guide in Scene View
            Debug.DrawLine(transform.position, targetLocation, Color.white, 1f);
        }

        // 4. RIGHT CLICK: Command Spirit
        if (Input.GetMouseButtonDown(1))
        {
            if (spirit != null)
            {
                Vector3 fireTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                fireTarget.z = 0;
                spirit.StartFireAction(fireTarget);
            }
        }
    }

    void FixedUpdate()
    {
        if (isAutoMoving)
        {
            float distance = Vector2.Distance(rb.position, targetLocation);

            if (distance > dropDistance)
            {
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetLocation, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }
            else
            {
                // FIX: Spawn the wool at the TARGET, not at Wooldrin's feet
                // This prevents the physics engine from "shoving" Wooldrin away
                Instantiate(woolPrefab, targetLocation, Quaternion.identity);

                isAutoMoving = false;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // Manual Movement
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}