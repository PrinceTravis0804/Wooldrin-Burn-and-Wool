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
        // Now calls the Main Menu function we will define in GameManager
        GameManager.Instance.GoToMainMenu();
    }
}
