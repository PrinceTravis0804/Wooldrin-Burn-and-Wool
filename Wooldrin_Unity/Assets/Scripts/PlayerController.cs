using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // For Turning 'to siya

    //Spirit and Abilities ni Wooldrin
    public FireSpiritController spirit;
    public GameObject woolPrefab;
    public bool canDropWool = true;
    public float dropDistance = 0.2f;

    private Vector2 movement;
    private Vector2 targetLocation;
    private bool isAutoMoving = false;
    private bool lastFlipState = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get Manual Keyboard Input Only
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

    // If the player presses any movement key (WASD/Arrows)
    if (movement.magnitude > 0.1f)
    {
        if (isAutoMoving)
        {
            isAutoMoving = false; // Stops the sheep from sliding to the lure
            rb.velocity = Vector2.zero; // Stops any sliding momentum
            Debug.Log("Lure placement cancelled.");
        }
    }

        //LEFT CLICK: Drop Wool
    if (Input.GetMouseButtonDown(0) && canDropWool)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            targetLocation = Camera.main.ScreenToWorldPoint(mousePos);
            isAutoMoving = true;
        }

        // RIGHT CLICK: Command Spirit to Attack
    if (Input.GetMouseButtonDown(1)) // 1 is Right Click
    {
        if (spirit != null)
        {
            // Convert mouse position to world position
            Vector3 fireTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            fireTarget.z = 0; // Ensure it stays on the 2D plane
            
            // Send the command to the FireSpiritController
            spirit.StartFireAction(fireTarget);
        }
    }

        // Update Animator and Flipping
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        // We use the raw movement vector for direction
        Vector2 currentDir = isAutoMoving ? (targetLocation - (Vector2)transform.position).normalized : movement.normalized;

        // 1. Send values to Animator (using Abs for X to stay on "Side" view)
        animator.SetFloat("MoveX", Mathf.Abs(movement.x)); 
        animator.SetFloat("MoveY", movement.y);

        // 2. STICKY FLIP: Only change flipX if moving left or right
        if (Mathf.Abs(movement.x) > 0.1f)
        {
            bool shouldFlip = (movement.x < -0.1f);
            spriteRenderer.flipX = shouldFlip;
            lastFlipState = shouldFlip;
        }
        else if (Mathf.Abs(movement.y) > 0.1f)
        {
            // Keep looking the same way horizontally even when walking Up/Down
            spriteRenderer.flipX = lastFlipState;
        }
    }

    void FixedUpdate()
    {

        if (isAutoMoving)
        {
            // 1. Calculate the direction from Wooldrin to the target
            Vector2 directionToTarget = (targetLocation - rb.position).normalized;

            // 2. Define a "Stop Point" that is 0.5 units away from the actual click
            // Change 0.5f to a higher number if you want him to stay further away
            Vector2 stopPoint = targetLocation - (directionToTarget * 0.10f);

            float distanceToStop = Vector2.Distance(rb.position, stopPoint);

            if (distanceToStop > 0.2f)
            {
                // Move toward the STOP point, not the wool itself
                rb.MovePosition(Vector2.MoveTowards(rb.position, stopPoint, moveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                // ARRIVED at the safe distance: Now drop the wool at the original targetLocation
                Instantiate(woolPrefab, targetLocation, Quaternion.identity);
                
                isAutoMoving = false;
                rb.velocity = Vector2.zero; 
            }
        }
        else
        {
            // Normal Manual Movement
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}