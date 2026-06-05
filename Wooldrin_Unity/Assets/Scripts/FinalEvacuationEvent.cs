using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FinalEvacuationEvent : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("How many enemies need to be in this zone to trigger the exit.")]
    public int requiredFecesCount = 15;
    [Tooltip("The tag of the enemies you need to herd (e.g., 'Enemy' or 'Feces').")]
    public string fecesTag = "Enemy";

    [Header("Exit Sequence")]
    [Tooltip("The actual Level Exit object to enable once the way is clear.")]
    public GameObject exitPortal;
    [Tooltip("An optional wall/obstacle to disable when the event triggers.")]
    public GameObject blockingWall;
    [Tooltip("The transform point where the enemies will be sucked towards (the 'sphincter').")]
    public Transform exitWaypoint;

    [Header("Audio & Feedback")]
    public AudioClip rumbleSound;
    public AudioClip flushSound;

    private HashSet<GameObject> collectedFeces = new HashSet<GameObject>();
    private bool isEvacuating = false;

    private void Start()
    {
        // Ensure the exit is hidden at the start of the level
        if (exitPortal != null) exitPortal.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't count if we are already doing the cutscene
        if (isEvacuating) return;

        if (other.CompareTag(fecesTag))
        {
            collectedFeces.Add(other.gameObject);
            CheckCount();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isEvacuating) return;

        if (other.CompareTag(fecesTag))
        {
            collectedFeces.Remove(other.gameObject);
        }
    }

    private void CheckCount()
    {
        // Clean up any destroyed objects just in case Wooldrin killed one in the zone
        collectedFeces.RemoveWhere(item => item == null);

        Debug.Log($"Collected Feces: {collectedFeces.Count} / {requiredFecesCount}");

        if (collectedFeces.Count >= requiredFecesCount && !isEvacuating)
        {
            StartCoroutine(EvacuationSequence());
        }
    }

    private IEnumerator EvacuationSequence()
    {
        isEvacuating = true;

        // 1. Initial Feedback
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(2.0f, 0.5f); // Long, heavy rumble
        }

        if (rumbleSound != null) AudioSource.PlayClipAtPoint(rumbleSound, transform.position);

        // Dialogue warning
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Baanita", "The dragon's stomach is rumbling fiercely... A massive evacuation is starting!");
        }

        // Wait a moment for the player to read the text
        yield return new WaitForSeconds(2.0f);

        if (flushSound != null) AudioSource.PlayClipAtPoint(flushSound, transform.position);

        // Prepare enemies for the flush
        List<Transform> fecesTransforms = new List<Transform>();
        Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();

        foreach (var feces in collectedFeces)
        {
            if (feces != null)
            {
                // Turn off their AI brain so they stop fighting Wooldrin
                AgentUtilityBrain brain = feces.GetComponent<AgentUtilityBrain>();
                if (brain != null) brain.enabled = false;

                // Turn off colliders so they slide right through walls/player
                Collider2D[] colliders = feces.GetComponentsInChildren<Collider2D>();
                foreach (var col in colliders) col.enabled = false;

                fecesTransforms.Add(feces.transform);
                startPositions[feces.transform] = feces.transform.position;
            }
        }

        float duration = 2.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (var feces in fecesTransforms)
            {
                if (feces != null && exitWaypoint != null)
                {
                    // Sucks them into the center point
                    feces.position = Vector3.Lerp(startPositions[feces], exitWaypoint.position, t);

                    // Shrinks them down as they "fall" out
                    feces.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                }
            }
            yield return null;
        }

        // Destroy the enemies entirely
        foreach (var feces in fecesTransforms)
        {
            if (feces != null) Destroy(feces.gameObject);
        }

        // Reveal the exit!
        if (blockingWall != null) blockingWall.SetActive(false);
        if (exitPortal != null) exitPortal.SetActive(true);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue("Baanita", "The path is clear! Go, Wooldrin, my son, escape while it's open!");
        }
    }
}