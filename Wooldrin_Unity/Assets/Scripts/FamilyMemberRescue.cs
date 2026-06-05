using UnityEngine;
using System; // Required for passing Actions/Callbacks

public class FamilyMemberRescue : MonoBehaviour
{
    [Header("Rescue Settings")]
    public string familyMemberID = "Sister Baa";

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string rescueDialogue = "Oh, Wooldrin! Thank goodness you came! Let's escape this boiling dragon belly before we're fully digested!";

    [Header("Visual & Sound Effects")]
    public GameObject rescueParticles;
    public AudioClip rescueClip;
    [Tooltip("Drag your interaction bubble/indicator child object here.")]
    public GameObject indicatorPrompt;

    private bool hasBeenRescuedThisSession = false;
    private bool isPlayerInRange = false;

    public bool HasBeenRescued => hasBeenRescuedThisSession;

    private void Start()
    {
        // Hide the prompt icon by default
        if (indicatorPrompt != null) indicatorPrompt.SetActive(false);

        // If already rescued in a previous scene/save, remove this NPC immediately
        if (GameManager.Instance != null && GameManager.Instance.rescuedFamilyMembers.Contains(familyMemberID))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Listen for the F key, but only if close by and not already rescued
        if (isPlayerInRange && !hasBeenRescuedThisSession && Input.GetKeyDown(KeyCode.F))
        {
            // Do not trigger if another dialogue is currently playing
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

            StartRescueSequence();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenRescuedThisSession)
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
        hasBeenRescuedThisSession = true;

        // Hide prompt while talking
        if (indicatorPrompt != null) indicatorPrompt.SetActive(false);

        if (DialogueManager.Instance != null)
        {
            // We pass "CompleteRescue" as the callback action.
            // This tells the DialogueManager: "When the player closes the text box, run this method."
            DialogueManager.Instance.ShowDialogue(familyMemberID, rescueDialogue, CompleteRescue);
        }
        else
        {
            // Fallback in case DialogueManager is missing
            CompleteRescue();
        }
    }

    private void CompleteRescue()
    {
        // 1. Notify the GameManager and Apply Buffs
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RescueFamilyMember(familyMemberID);

            if (familyMemberID == "Baaron")
            {
                GameManager.Instance.UnlockKickMechanic();
            }
            else
            {
                GameManager.Instance.ApplyBuff(familyMemberID);
            }
        }

        // 2. Play Effects
        if (rescueClip != null)
        {
            AudioSource.PlayClipAtPoint(rescueClip, transform.position, 1.0f);
        }

        if (rescueParticles != null)
        {
            Instantiate(rescueParticles, transform.position, Quaternion.identity);
        }

        // 3. Finally, destroy the NPC object now that the dialogue is over
        Destroy(gameObject);
    }
}