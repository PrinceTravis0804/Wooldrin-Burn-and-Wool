using UnityEngine;

public class WoolResource : MonoBehaviour
{
    public float health = 100f;
    private Vector3 startScale;
    private PlayerController player;

    void Start()
    {
        startScale = transform.localScale;
        player = FindObjectOfType<PlayerController>();
    }

    public void TakeBite(float amount)
    {
        health -= amount;
        transform.localScale = startScale * (health / 100f);

        if (health <= 0)
        {
            if (player != null) player.NotifyWoolDestroyed();
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy if hit by fire
        if (collision.gameObject.CompareTag("Fire"))
        {
            if (player != null) player.NotifyWoolDestroyed();
            Destroy(gameObject);
        }
    }
}