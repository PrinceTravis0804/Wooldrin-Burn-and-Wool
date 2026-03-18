using UnityEngine;

public class WoolResource : MonoBehaviour
{
    [Header("Health Settings")]
    public float nourishment = 100f; // How much "food" is in this wool
    public float shrinkSpeed = 0.5f; // How fast it visually shrinks while eaten

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    // This is called by the Agent when they are touching the wool
    public void GetConsumed(float amount)
    {
        nourishment -= amount;

        // Visually shrink the wool as it gets eaten
        float scalePercent = nourishment / 100f;
        transform.localScale = initialScale * scalePercent;

        if (nourishment <= 0)
        {
            // Optional: Trigger a "munch" sound or particles here
            Destroy(gameObject);
        }
    }
}