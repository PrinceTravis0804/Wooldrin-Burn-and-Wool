using UnityEngine;

public class WoolPressurePlate : MonoBehaviour
{
    [Header("Linked Obstacles")]
    [Tooltip("Drag the obstacle GameObjects (like an Acid Wall or Door) here. They will disappear when pressed.")]
    public GameObject[] obstaclesToDisable;

    [Header("Visuals & Audio")]
    public Sprite unpressedSprite;
    public Sprite pressedSprite;
    public AudioClip pressSound;

    [Header("Settings")]
    [Tooltip("If true, the door stays open forever once pressed. If false, removing the wool closes the door.")]
    public bool keepOpenWhenRemoved = false;

    private SpriteRenderer sr;
    private int woolsOnPlate = 0;
    private bool isPressed = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && unpressedSprite != null) sr.sprite = unpressedSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect if a dropped or kicked wool lands on the plate
        if (other.CompareTag("Wool"))
        {
            woolsOnPlate++;
            if (!isPressed && woolsOnPlate > 0)
            {
                PressPlate();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wool"))
        {
            woolsOnPlate--;
            if (woolsOnPlate <= 0)
            {
                woolsOnPlate = 0;

                // Only close the door if we haven't checked "Keep Open"
                if (!keepOpenWhenRemoved)
                {
                    ReleasePlate();
                }
            }
        }
    }

    private void PressPlate()
    {
        isPressed = true;

        // Update visuals and audio
        if (sr != null && pressedSprite != null) sr.sprite = pressedSprite;
        if (pressSound != null) AudioSource.PlayClipAtPoint(pressSound, transform.position);

        // Turn off all linked obstacles
        foreach (var obs in obstaclesToDisable)
        {
            if (obs != null) obs.SetActive(false);
        }
    }

    private void ReleasePlate()
    {
        isPressed = false;

        if (sr != null && unpressedSprite != null) sr.sprite = unpressedSprite;

        // Turn the obstacles back on
        foreach (var obs in obstaclesToDisable)
        {
            if (obs != null) obs.SetActive(true);
        }
    }
}