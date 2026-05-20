using UnityEngine;

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

    private bool hasBeenRescuedThisSession = false;
    public bool HasBeenRescued => hasBeenRescuedThisSession;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.rescuedFamilyMembers.Contains(familyMemberID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasBeenRescuedThisSession && other.CompareTag("Player"))
        {
            ExecuteRescue(other.gameObject);
        }
    }

    private void ExecuteRescue(GameObject player)
    {
        hasBeenRescuedThisSession = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RescueFamilyMember(familyMemberID);
        }

        if (rescueClip != null)
        {
            AudioSource.PlayClipAtPoint(rescueClip, transform.position, 1.0f);
        }

        if (rescueParticles != null)
        {
            Instantiate(rescueParticles, transform.position, Quaternion.identity);
        }

        // --- NEW: KICK OFF RESCUE DIALOGUE ---
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue(familyMemberID, rescueDialogue);
        }

        GetComponent<Collider2D>().enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject, 0.1f);
    }
}