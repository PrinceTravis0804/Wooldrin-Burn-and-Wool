using UnityEngine;

public class AcidHazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("How hard Wooldrin is pushed back when touching the acid.")]
    public float hazardKnockback = 12f;
    [Tooltip("How fast the acid eats away at Wool decoys dropped into it.")]
    public float woolMeltRate = 50f;

    // This runs if you set your collider to "Is Trigger" (Perfect for Puddles you walk over)
    private void OnTriggerStay2D(Collider2D other)
    {
        AttemptDamage(other.gameObject);
    }

    // This runs if you DO NOT check "Is Trigger" (Perfect for Acid Walls that physically block you)
    private void OnCollisionStay2D(Collision2D collision)
    {
        AttemptDamage(collision.gameObject);
    }

    private void AttemptDamage(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            WooldrinHealth health = target.GetComponent<WooldrinHealth>();

            // We rely on WooldrinHealth's internal 'IsInvulnerable' check so it respects the I-Frames!
            if (health != null && !health.IsInvulnerable)
            {
                // Send the center of the acid as the attacker position so Wooldrin is knocked outward
                health.TakeDamage(transform.position, hazardKnockback);
            }
        }
        else if (target.CompareTag("Wool"))
        {
            // Acid dissolves wool rapidly! Don't let players use wool as a bridge over acid.
            WoolResource wool = target.GetComponent<WoolResource>();
            if (wool != null)
            {
                wool.TakeBite(woolMeltRate * Time.deltaTime);
            }
        }
    }
}