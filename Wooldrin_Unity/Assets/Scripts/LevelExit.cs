using UnityEngine;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject winDialogue; // Drag your WinDialogue object here
    public float delayTime = 1.0f; // Wait 1 seconds

    private bool levelFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if Wooldrin touches the green box
        if (other.CompareTag("Player") && !levelFinished)
        {
            levelFinished = true;
            StartCoroutine(FinishSequence());
        }
    }

    private IEnumerator FinishSequence()
    {
        // 1. Wait while Wooldrin is still moving
        yield return new WaitForSeconds(delayTime);

        // 2. Show the dialogue box (all text appears at once)
        if (winDialogue != null)
        {
            winDialogue.SetActive(true);
        }

        // 3. Freeze the game
        Time.timeScale = 0f;
    }
}