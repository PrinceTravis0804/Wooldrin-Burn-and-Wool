using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySound : MonoBehaviour
{
    public AudioSource walkSound;
    public AudioSource fireSound;
    public AudioSource woolSound;
    public AudioSource damageSound;

    [Header("Stomach Level Overrides")]
    [Tooltip("The exact name of your stomach scene asset.")]
    public string stomachSceneName = "Level_02_Stomach";
    [Tooltip("The wet sloshing walk sound effect file.")]
    public AudioClip wetWalkClip;
    [Range(0f, 1f)] public float wetWalkVolume = 0.5f;

    private AudioClip defaultWalkClip;
    private float defaultWalkVolume;
    private bool isStomachLevel = false;

    void Start()
    {
        // Store the original dry walk sound assigned to the AudioSource
        if (walkSound != null)
        {
            defaultWalkClip = walkSound.clip;
            defaultWalkVolume = walkSound.volume;
        }

        // Check if we started directly in the stomach level
        CheckCurrentScene();
    }

    void Update()
    {
        // --- WALKING LOGIC ---
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            // Swap the clip dynamically depending on the level
            if (walkSound != null && !walkSound.isPlaying)
            {
                if (isStomachLevel && wetWalkClip != null)
                {
                    walkSound.clip = wetWalkClip;
                    walkSound.volume = wetWalkVolume;
                }
                else
                {
                    walkSound.clip = defaultWalkClip;
                    walkSound.volume = defaultWalkVolume;
                }

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
        // CHANGED: 1 (Right Click) is now 0 (Left Click)
        if (Input.GetMouseButtonDown(0))
        {
            if (fireSound != null && fireSound.clip != null)
            {
                fireSound.PlayOneShot(fireSound.clip);
            }
        }
    }

    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        isStomachLevel = (currentScene == stomachSceneName);
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