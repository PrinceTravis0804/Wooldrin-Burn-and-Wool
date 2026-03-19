using UnityEngine;

public class LevelExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("LEVEL COMPLETE! You reached the Stomach.");
            // Later, this will load the next scene
        }
    }
}