using UnityEngine;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject winDialogue; // Drag your WinDialogue object here
    public float delayTime = 1.0f; // Wait time before showing UI

    private bool levelFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if Wooldrin touches the valve/exit sprite
        // This requires Wooldrin to have the "Player" Tag assigned in the Inspector
        if (other.CompareTag("Player") && !levelFinished)
        {
            levelFinished = true;
            StartCoroutine(FinishSequence());
        }
    }

    private IEnumerator FinishSequence()
    {
        // 1. Wait for the specified delay
        yield return new WaitForSeconds(delayTime);

        // 2. Show the dialogue box
        if (winDialogue != null)
        {
            winDialogue.SetActive(true);
        }

        // 3. Freeze the game
        // Note: Any buttons in the WinDialogue must be set to "Unscaled Time" 
        // in their animator/logic if you want them to move while paused.
        Time.timeScale = 0f;

        Debug.Log("Level Exit Triggered: Game Paused.");
    }
}