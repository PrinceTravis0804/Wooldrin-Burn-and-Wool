using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public AudioSource walkSound;
    public AudioSource fireSound;
    public AudioSource woolSound;
    public AudioSource damageSound;

    void Update()
    {
        // --- WALKING LOGIC ---
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            if (walkSound != null && !walkSound.isPlaying)
            {
                walkSound.Play();
            }
        }
        else
        {
            if (walkSound != null && walkSound.isPlaying)
            {
                walkSound.Stop();
            }
        }

        // --- FIRING LOGIC ---
        if (Input.GetMouseButtonDown(1))
        {
            if (fireSound != null && fireSound.clip != null)
            {
                fireSound.PlayOneShot(fireSound.clip);
            }
        }
    }

    public void PlayWoolPlacementSound()
    {
        if (woolSound != null && woolSound.clip != null)
        {
            Debug.Log("PlaySound: Playing Wool Sound");
            woolSound.PlayOneShot(woolSound.clip);
        }
        else
        {
            Debug.LogWarning("PlaySound: Wool AudioSource or Clip is MISSING!");
        }
    }

    public void PlayDamageSound()
    {
        if (damageSound != null && damageSound.clip != null)
        {
            Debug.Log("PlaySound: Playing Damage Sound (Baa!)");
            damageSound.PlayOneShot(damageSound.clip);
        }
        else
        {
            Debug.LogWarning("PlaySound: Damage AudioSource or Clip is MISSING!");
        }
    }
}