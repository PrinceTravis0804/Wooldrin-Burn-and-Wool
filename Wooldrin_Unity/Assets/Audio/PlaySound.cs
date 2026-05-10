using UnityEngine;

public class PlaySound : MonoBehaviour
{   
    public AudioSource walkSound;

    void Update()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            if (!walkSound.isPlaying)
            {
                walkSound.Play();
            }
        }
        else
        {
            walkSound.Stop();
        }
    }
}