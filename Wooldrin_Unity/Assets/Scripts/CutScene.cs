using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Added this namespace for standalone loading

public class CutsceneEventHandler : MonoBehaviour
{
    public void OnCutsceneEnded()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadFirstLevel();
        }
        else
        {
            // Fallback so you can still test directly inside the CutSceneLoad scene
            Debug.LogWarning("GameManager instance not found. Loading Level_01_Throat as a fallback.");
            SceneManager.LoadScene("Level_01_Throat"); 
        }
    }
}
