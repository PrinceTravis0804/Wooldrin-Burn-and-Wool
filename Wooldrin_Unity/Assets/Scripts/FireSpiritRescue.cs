using UnityEngine;

public class FireSpiritRescue : MonoBehaviour
{
    [Header("Visual Feedback")]
    public GameObject rescueEffect; // Drag a particle prefab here if you have one
    public float destroyDelay = 0.2f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the lamb touches the rescue point
        if (!hasTriggered && other.CompareTag("Player"))
        {
            ExecuteRescue();
        }
    }

    private void ExecuteRescue()
    {
        hasTriggered = true;

        // 1. Tell the GameManager to unlock the spirit forever
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFireSpiritRescued();
        }
        else
        {
            Debug.LogError("FireSpiritRescue: Could not find GameManager! Is the _GameManager object in your scene?");
        }

        // 2. Play effects
        if (rescueEffect != null)
        {
            Instantiate(rescueEffect, transform.position, Quaternion.identity);
        }

        Debug.Log("<color=orange>FireSpiritRescue:</color> You've rescued the flame! It will now travel with you.");

        // 3. Remove the 'Caged' object
        // The GameManager's SetFireSpiritRescued() will handle spawning the 'Active' follow-prefab
        Destroy(gameObject, destroyDelay);
    }
}