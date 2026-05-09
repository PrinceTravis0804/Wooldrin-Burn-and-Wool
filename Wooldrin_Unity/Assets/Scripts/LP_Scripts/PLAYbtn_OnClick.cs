using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PLAYbtn_OnClick : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Level0");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
