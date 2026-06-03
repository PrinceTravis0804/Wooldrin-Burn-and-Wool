using UnityEngine;
using System.Collections;

public class TimedAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;
    public float interval = 20f;

    [Header("Dynamic Settings")]
    public bool randomizePitch = false;
    public float minPitch = 0.85f;
    public float maxPitch = 1.15f;

    void Start()
    {
        if (audioSource != null && audioClip != null)
        {
            StartCoroutine(PlaySoundRoutine());
        }
    }

    IEnumerator PlaySoundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (randomizePitch)
            {
                // Assign a slightly different speed/pitch each time it plays
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            audioSource.PlayOneShot(audioClip);
        }
    }
}