using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find if there is a FamilyMemberRescue object in this scene
            FamilyMemberRescue rescue = FindObjectOfType<FamilyMemberRescue>();

            // If a rescue object exists and has not been saved yet, block the portal
            if (rescue != null && !rescue.HasBeenRescued)
            {
                Debug.Log("<color=red>LevelPortal Locked:</color> You must rescue the family member before leaving the stage!");
                // Note: You can trigger a UI pop-up warning or sound effect here if desired!
                return;
            }

            Debug.Log("Valve: Automatically loading next stage...");
            GameManager.Instance.LoadNextLevel();
        }
    }
}