using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public void ClickRestart()
    {
        // This talks to your Singleton GameManager
        GameManager.Instance.RestartLevel();
    }

    public void ClickMainMenu()
    {
        // If you have a Main Menu function in GameManager, call it here
        // GameManager.Instance.LoadNextLevel(); // Or a specific menu function
    }
}