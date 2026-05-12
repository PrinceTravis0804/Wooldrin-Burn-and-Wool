using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    // This triggers automatically when Wooldrin's collider enters the valve area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Valve: Automatically loading next stage...");
            GameManager.Instance.LoadNextLevel();
        }
    }
}