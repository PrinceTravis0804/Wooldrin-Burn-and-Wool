using UnityEngine;

public class PlaySound : MonoBehaviour
{   
    public AudioSource walkSound;
    public AudioSource fireSound;

    void Update()
    {
        // --- WALKING LOGIC ---
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

        // --- FIRING LOGIC ---
        if (Input.GetMouseButtonDown(1))
        {
            fireSound.PlayOneShot(fireSound.clip);
        }
    }
}