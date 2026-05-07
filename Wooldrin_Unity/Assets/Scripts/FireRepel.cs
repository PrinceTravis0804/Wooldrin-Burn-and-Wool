using UnityEngine;

public class FireRepel : MonoBehaviour
{
    [Header("Repel Settings")]
    [Tooltip("How far the heat reaches to push enemies.")]
    public float repelRadius = 1.5f;
    [Tooltip("The strength of the push.")]
    public float repelForce = 8f;
    [Tooltip("Set this to your Enemy layer.")]
    public LayerMask enemyLayer;

    private void Update()
    {
        // Find all enemies in the blast radius
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, repelRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Calculate direction from the fire to the enemy
            Vector2 repelDir = (enemy.transform.position - transform.position).normalized;

            // Apply movement to the enemy
            // Try Rigidbody first for physics-based enemies
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(repelDir * repelForce);
            }
            else
            {
                // Fallback for simple scripted enemies
                enemy.transform.position += (Vector3)repelDir * repelForce * Time.deltaTime;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the heat zone in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, repelRadius);
    }
}