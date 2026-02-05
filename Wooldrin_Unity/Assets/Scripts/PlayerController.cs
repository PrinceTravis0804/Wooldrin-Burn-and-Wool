using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Ability References")]
    public GameObject woolPrefab;
    public float dropDistance = 0.5f;

    // NEW: Reference to the Spirit's script
    public FireSpiritController spirit;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;

    void Update()
    {
        // 1. Manual Movement Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Cancel auto-move if a key is pressed
        if (movement.sqrMagnitude > 0)
        {
            isAutoMoving = false;
        }

        // 2. Left Click: Auto-move to drop WOOL
        if (Input.GetMouseButtonDown(0))
        {
            targetLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isAutoMoving = true;
        }

        // 3. Right Click: Command SPIRIT to drop FIRE
        if (Input.GetMouseButtonDown(1))
        {
            if (spirit != null)
            {
                Vector3 fireTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                fireTarget.z = 0;

                // Calling the method from the FireSpiritController script
                spirit.StartFireAction(fireTarget);
            }
        }
    }

    void FixedUpdate()
    {
        if (isAutoMoving)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetLocation, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, targetLocation) < dropDistance)
            {
                Instantiate(woolPrefab, transform.position, Quaternion.identity);
                isAutoMoving = false;
            }
        }
        else
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
