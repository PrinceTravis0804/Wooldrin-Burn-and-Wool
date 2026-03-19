using UnityEngine;

public class KickableWool : MonoBehaviour
{
    public GameObject indicator;    // Drag your "Indicator" child here
    public float kickForce = 10f;    // How hard it flies
    private Rigidbody2D rb;
    private bool canBeKicked = false;
    private Transform playerTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (indicator != null) indicator.SetActive(false);
    }

    void Update()
    {
        // If the player is near and presses 'F' (or whatever key you prefer)
        if (canBeKicked && Input.GetKeyDown(KeyCode.F))
        {
            Kick();
        }
    }

    void Kick()
    {
        // Calculate direction: From Wooldrin toward the Wool
        Vector2 kickDirection = (transform.position - playerTransform.position).normalized;

        // Apply the force as a sudden "Impulse"
        rb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);

        // Optional: Hide indicator immediately after kick
        if (indicator != null) indicator.SetActive(false);
    }

    // Detect when Wooldrin is near
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canBeKicked = true;
            playerTransform = other.transform;
            if (indicator != null) indicator.SetActive(true);
        }
    }

    // Detect when Wooldrin leaves
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canBeKicked = false;
            if (indicator != null) indicator.SetActive(false);
        }
    }
}