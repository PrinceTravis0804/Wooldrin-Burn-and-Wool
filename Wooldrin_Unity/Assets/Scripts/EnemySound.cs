using UnityEngine;

public class EnemySound : MonoBehaviour
{
    public AudioSource walkSource;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Plays sound only when the slime is actually moving
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            if (!walkSource.isPlaying) walkSource.Play();
        }
        else
        {
            walkSource.Stop();
        }
    }
}