using UnityEngine;
using UnityEngine.SceneManagement; // Essential for changing scenes

public class MainMenuNavigator : MonoBehaviour
{
    public void GoToLandingPage()
    {
        // 1. Reset time (Very important since LevelExit stops it!)
        Time.timeScale = 1f;

        // 2. Load your main menu scene by its name
        // Replace "LandingPage" with the actual name of your menu scene
        SceneManager.LoadScene("LandingPage");
    }
}