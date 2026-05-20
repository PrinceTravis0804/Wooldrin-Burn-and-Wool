using UnityEngine;

public class FamilyMemberRescue : MonoBehaviour
{
    [Header("Rescue Settings")]
    [Tooltip("Give this family member a unique name (e.g., Mother, Brother, Uncle). This name is stored in the GameManager's list.")]
    public string familyMemberID = "FamilyMember_01";

    [Header("Visual & Sound Effects")]
    [Tooltip("Optional particle effect prefab to spawn when rescued (e.g. confetti, hearts, bubbles).")]
    public GameObject rescueParticles;

    [Tooltip("Optional sound effect to play when this family member is rescued.")]
    public AudioClip rescueClip;

    private bool hasBeenRescuedThisSession = false;

    // Public property so the level portal can check if the rescue has been completed
    public bool HasBeenRescued => hasBeenRescuedThisSession;

    private void Start()
    {
        // Safety: If the GameManager already has this member in its list,
        // it means they were rescued earlier. We instantly destroy this object
        // so they don't reappear on stage reloads or backtrack visits!
        if (GameManager.Instance != null && GameManager.Instance.rescuedFamilyMembers.Contains(familyMemberID))
        {
            Debug.Log($"<color=cyan>FamilyMemberRescue:</color> {familyMemberID} was already rescued. Removing from scene.");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure only the Player (Wooldrin) can trigger the rescue
        if (!hasBeenRescuedThisSession && other.CompareTag("Player"))
        {
            ExecuteRescue(other.gameObject);
        }
    }

    private void ExecuteRescue(GameObject player)
    {
        hasBeenRescuedThisSession = true;

        // 1. Tell the GameManager to record this rescue
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RescueFamilyMember(familyMemberID);
            Debug.Log($"<color=cyan>FamilyMemberRescue:</color> Saved {familyMemberID}! Total saved: {GameManager.Instance.rescuedFamilyMembers.Count}");
        }
        else
        {
            Debug.LogError("FamilyMemberRescue: GameManager.Instance is missing! Cannot record rescue.");
        }

        // 2. Play Audio Feedback
        if (rescueClip != null)
        {
            // We play the sound directly in the world space so it doesn't get cut off if this object is destroyed
            AudioSource.PlayClipAtPoint(rescueClip, transform.position, 1.0f);
        }

        // 3. Play Visual Feedback (Particles)
        if (rescueParticles != null)
        {
            Instantiate(rescueParticles, transform.position, Quaternion.identity);
        }

        // 4. Handle Object Removal
        // We deactivate the visual/collider first to prevent multi-triggering, then destroy
        GetComponent<Collider2D>().enabled = false;

        // If your family member has a child sprite object, we can disable it or destroy the whole container
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // Destroy the game object fully after a brief moment
        Destroy(gameObject, 0.1f);
    }
}