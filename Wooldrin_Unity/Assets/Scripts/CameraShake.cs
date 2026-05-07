using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Sensitivity Settings")]
    [Range(0f, 1f)]
    [Tooltip("Adjust this to make all shakes globally more or less intense. 0.1 to 0.3 is usually best for subtle effects.")]
    public float shakeMultiplier = 0.2f;

    private Vector3 initialLocalPos;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void OnEnable()
    {
        // Store the default local position (usually 0,0,0 or 0,0,-10)
        initialLocalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        // Use a coroutine so multiple shakes don't compound strangely
        if (!isShaking)
        {
            StartCoroutine(PerformShake(duration, magnitude));
        }
    }

    private IEnumerator PerformShake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0.0f;

        // Apply the multiplier to the incoming magnitude to make it subtle
        float adjustedMagnitude = magnitude * shakeMultiplier;

        while (elapsed < duration)
        {
            // Generate a small random offset
            float x = Random.Range(-1f, 1f) * adjustedMagnitude;
            float y = Random.Range(-1f, 1f) * adjustedMagnitude;

            // Apply shake to the LOCAL position
            // This requires the camera to be a child of a 'Camera Holder' follower
            transform.localPosition = initialLocalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to the exact initial position relative to the parent
        transform.localPosition = initialLocalPos;
        isShaking = false;
    }
}