using UnityEngine;
using System.Collections.Generic;

public class FireSpiritRescue : MonoBehaviour
{
    [Header("Dialogue Content")]
    public string speakerName = "Ember";
    [TextArea(2, 4)]
    public string[] dialogueLines = new string[] {
        "Phew, it's cramped in there! Thanks for breaking me out of that trap, Wooldrin!",
        "I owe you one! As thanks, I'll light the way and burn through any toxic barriers in our path.",
        "Just aim where you want me to go and Left-Click. Let's roast some slimes!"
    };

    [Header("Visual Feedback")]
    public GameObject rescueEffect;
    public AudioClip rescueClip;
    [Tooltip("Drag your interaction bubble/indicator child object here.")]
    public GameObject indicatorPrompt;

    [Header("Visual Swapping (During Dialogue)")]
    [Tooltip("The static cage sprite/object.")]
    public GameObject cagedVisual;
    [Tooltip("The animated floating ember sprite/object. Hidden by default.")]
    public GameObject freedAnimatedVisual;

    private bool hasTriggered = false;
    private bool isPlayerInRange = false;

    private void Start()
    {
        if (indicatorPrompt != null) indicatorPrompt.SetActive(false);

        // Ensure the correct visuals are showing before interaction
        if (freedAnimatedVisual != null) freedAnimatedVisual.SetActive(false);
        if (cagedVisual != null) cagedVisual.SetActive(true);

        // If already rescued in a previous session/scene, remove this caged version
        if (GameManager.Instance != null && GameManager.Instance.isFireSpiritRescued)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Listen for the F key, but only if close by and not already triggered
        if (isPlayerInRange && !hasTriggered && Input.GetKeyDown(KeyCode.F))
        {
            // Do not trigger if another dialogue is currently playing
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

            StartRescueSequence();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            isPlayerInRange = true;
            if (indicatorPrompt != null) indicatorPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (indicatorPrompt != null) indicatorPrompt.SetActive(false);
        }
    }

    private void StartRescueSequence()
    {
        hasTriggered = true;

        // Hide the "F" prompt while talking
        if (indicatorPrompt != null) indicatorPrompt.SetActive(false);

        // Swap the visuals to show the freed animated Ember
        if (cagedVisual != null) cagedVisual.SetActive(false);
        if (freedAnimatedVisual != null) freedAnimatedVisual.SetActive(true);

        // Play the cage breaking effects immediately as the dialogue starts!
        if (rescueEffect != null)
        {
            Instantiate(rescueEffect, transform.position, Quaternion.identity);
        }

        if (rescueClip != null)
        {
            AudioSource.PlayClipAtPoint(rescueClip, transform.position, 1.0f);
        }

        if (DialogueManager.Instance != null)
        {
            // Convert our string array into Dialogue Lines
            List<DialogueManager.DialogueLine> lines = new List<DialogueManager.DialogueLine>();
            foreach (string text in dialogueLines)
            {
                lines.Add(new DialogueManager.DialogueLine(speakerName, text));
            }

            // Start dialogue and pass "CompleteRescue" to run ONLY when the player finishes reading
            DialogueManager.Instance.StartDialogueSequence(lines, CompleteRescue);
        }
        else
        {
            CompleteRescue();
        }
    }

    private void CompleteRescue()
    {
        // Tell GameManager to unlock the spirit
        // This command is what actually spawns the little follower next to you!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFireSpiritRescued();
        }

        // Destroy the static caged prefab now that the dialogue is over
        Destroy(gameObject);
    }
}