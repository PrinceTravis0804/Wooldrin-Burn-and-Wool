using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    private bool canExit = false;

    void Update()
    {
        if (canExit && Input.GetKeyDown(KeyCode.E)) GameManager.Instance.LoadNextLevel();
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) canExit = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) canExit = false; }
}