using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotatingAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> audioClips = new List<AudioClip>();
    public float interval = 20f;

    private int currentIndex = 0;

    void Start()
    {
        if (audioSource != null && audioClips.Count > 0)
        {
            ShuffleClips();
            StartCoroutine(PlayRotationRoutine());
        }
    }

    IEnumerator PlayRotationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Play the current clip from the 3 new sounds
            audioSource.PlayOneShot(audioClips[currentIndex]);

            // Move to the next sound file
            currentIndex++;
            if (currentIndex >= audioClips.Count)
            {
                currentIndex = 0;
                ShuffleClips(); // Reshuffle order for the next cycle
            }
        }
    }

    void ShuffleClips()
    {
        for (int i = audioClips.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            AudioClip temp = audioClips[i];
            audioClips[i] = audioClips[rnd];
            audioClips[rnd] = temp;
        }
    }
}