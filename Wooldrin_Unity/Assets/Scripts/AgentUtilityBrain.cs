using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Simulation Settings")]
    public float detectionRadius = 5f;
    public float moveSpeed = 2f;

    [Header("Utility Weights")]
    public float attractionWeight = 10f; // Scent of Wool
    public float aversionWeight = 15f;   // Heat/Fire

    void Update()
    {
        // 1. Sense the environment
        // 2. Calculate Utility
        // 3. Move toward the highest utility
        SimulateDecision();
    }

    void SimulateDecision()
    {
        // Find all objects within sensory range
        Collider2D[] sensedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Vector2 moveDirection = Vector2.zero;

        foreach (var obj in sensedObjects)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance <= 0.1f) continue; // Avoid dividing by zero

            Vector2 directionToObj = (obj.transform.position - transform.position).normalized;

            // MATH: Utility = Weight / Distance
            if (obj.CompareTag("Wool"))
            {
                // Move TOWARD Attraction
                moveDirection += directionToObj * (attractionWeight / distance);
            }
            else if (obj.CompareTag("Fire"))
            {
                // Move AWAY from Aversion
                moveDirection -= directionToObj * (aversionWeight / distance);
            }
        }

        // Apply the simulated movement
        transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }

    // This draws the "Senses" in the editor for your class presentation
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
