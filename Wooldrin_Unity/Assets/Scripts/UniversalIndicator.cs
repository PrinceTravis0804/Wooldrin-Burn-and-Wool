using UnityEngine;
using UnityEngine.UI; // Required for handling Image components!
using TMPro;
using System;

public class UniversalIndicator : MonoBehaviour
{
    [Header("UI & Content Settings")]
    [Tooltip("The parent canvas or group containing the bubble graphics. Will be faded in/out.")]
    public CanvasGroup indicatorCanvasGroup;
    [Tooltip("The text component inside the bubble to display hints (e.g. '[F] Kick' or '[Space] Talk').")]
    public TextMeshProUGUI indicatorText;
    [Tooltip("The text content to display. Leave empty if you only want to show a graphic/icon.")]
    public string defaultText = "[F] INTERACT";

    [Header("Optional Sprite Animation Settings")]
    [Tooltip("The Image component that displays the icon we want to animate (e.g. Bubble_Image).")]
    public Image indicatorImage;
    [Tooltip("Assign sliced sprite frames here to cycle through them (e.g., key down, key up).")]
    public Sprite[] animationFrames;
    [Tooltip("Time in seconds to wait before changing to the next frame.")]
    public float frameDuration = 0.2f;
    [Tooltip("Should the sprite animation loop infinitely?")]
    public bool loopAnimation = true;

    [Header("Proximity Settings")]
    [Tooltip("The player must be within this distance for the indicator to appear.")]
    public float detectionRadius = 2.0f;
    [Tooltip("The tag assigned to your player GameObject to detect them.")]
    public string playerTag = "Player";

    [Header("Animation (Bobbing & Fading)")]
    [Tooltip("Speed of the floating vertical motion.")]
    public float bobSpeed = 3f;
    [Tooltip("How far up and down the bubble floats.")]
    public float bobAmount = 0.15f;
    [Tooltip("How fast the bubble fades in and out.")]
    public float fadeSpeed = 5f;

    [Header("Interaction Action (Optional)")]
    [Tooltip("If checked, the player can press an action key to trigger a dialogue or custom event.")]
    public bool enableActionKey = false;
    [Tooltip("The key the player must press to execute the action.")]
    public KeyCode actionKey = KeyCode.F;
    [Tooltip("Optional Dialogue parameters to trigger when the key is pressed.")]
    public string speakerName = "Signpost";
    [TextArea(2, 4)]
    public string dialogueText = "This is a mysterious warning message...";

    private Transform playerTransform;
    private Vector3 originalLocalPosition;
    private float startOffset;
    private bool isPlayerInRange = false;

    // Sprite Animation Trackers
    private int currentFrameIndex = 0;
    private float frameTimer = 0f;

    private void Start()
    {
        // Remember the starting local position so we bob relative to this spot
        originalLocalPosition = transform.localPosition;

        // Randomize start offset so multiple bubbles in a scene don't bob in perfect unison
        startOffset = UnityEngine.Random.Range(0f, 100f);

        // Find the player in the scene
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // Apply starting text
        if (indicatorText != null && !string.IsNullOrEmpty(defaultText))
        {
            indicatorText.text = defaultText;
        }

        // Initialize animation sprite if frames exist
        if (indicatorImage != null && animationFrames != null && animationFrames.Length > 0)
        {
            indicatorImage.sprite = animationFrames[0];
        }

        // Ensure the indicator starts completely invisible
        if (indicatorCanvasGroup != null)
        {
            indicatorCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        // 1. Proximity Check
        CheckPlayerDistance();

        // 2. Fade Transition (In/Out)
        HandleFading();

        // 3. Bobbing Floating Animation
        HandleBobbing();

        // 4. Sprite Frame Animation Cycle
        HandleSpriteAnimation();

        // 5. Action Key Detection
        HandleActionInput();
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null)
        {
            // Fallback search in case player spawned late
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= detectionRadius;
    }

    private void HandleFading()
    {
        if (indicatorCanvasGroup == null) return;

        float targetAlpha = isPlayerInRange ? 1f : 0f;

        // Smoothly interpolate the alpha transparency
        indicatorCanvasGroup.alpha = Mathf.MoveTowards(
            indicatorCanvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );
    }

    private void HandleBobbing()
    {
        // Only bob when visible to conserve performance
        if (indicatorCanvasGroup != null && indicatorCanvasGroup.alpha <= 0.01f) return;

        float newY = originalLocalPosition.y + (Mathf.Sin((Time.time * bobSpeed) + startOffset) * bobAmount);
        transform.localPosition = new Vector3(originalLocalPosition.x, newY, originalLocalPosition.z);
    }

    /// <summary>
    /// Cycles through the assigned sprite frames to create a clean, lightweight retro animation.
    /// </summary>
    private void HandleSpriteAnimation()
    {
        if (indicatorImage == null || animationFrames == null || animationFrames.Length == 0) return;

        // Skip animation logic when the indicator is hidden to save mobile/web performance
        if (indicatorCanvasGroup != null && indicatorCanvasGroup.alpha <= 0.01f) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrameIndex++;

            if (currentFrameIndex >= animationFrames.Length)
            {
                if (loopAnimation)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = animationFrames.Length - 1;
                }
            }

            indicatorImage.sprite = animationFrames[currentFrameIndex];
        }
    }

    private void HandleActionInput()
    {
        if (!enableActionKey || !isPlayerInRange) return;

        // Block interaction if a full-screen dialogue is currently typing/active
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

        if (Input.GetKeyDown(actionKey))
        {
            TriggerAction();
        }
    }

    private void TriggerAction()
    {
        Debug.Log($"UniversalIndicator: Triggered action key {actionKey} on {gameObject.name}!");

        // Hook back directly to our dialogue manager Canvas system to trigger the text box!
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(dialogueText))
        {
            DialogueManager.Instance.ShowDialogue(speakerName, dialogueText);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a visual wire circle in the editor showing the trigger radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}