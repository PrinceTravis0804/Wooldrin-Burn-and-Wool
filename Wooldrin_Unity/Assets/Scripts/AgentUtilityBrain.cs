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
        Collider2D[] sensedObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        Vector2 aversionForce = Vector2.zero;
        Vector2 attractionForce = Vector2.zero;
        int fireCount = 0;
        int woolCount = 0;

        foreach (var obj in sensedObjects)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance <= 0.1f) continue;

            Vector2 directionToObj = (obj.transform.position - transform.position).normalized;

            if (obj.CompareTag("Fire"))
            {
                // Summing Aversion: More fire = more fear
                aversionForce -= directionToObj * (aversionWeight / distance);
                fireCount++;
            }
            else if (obj.CompareTag("Wool"))
            {
                // Summing Attraction: More wool = more lure
                attractionForce += directionToObj * (attractionWeight / distance);
                woolCount++;
            }

            if (fireCount > 0)
            {
                GetComponent<SpriteRenderer>().color = Color.red; // Panic state
            }
            else if (woolCount > 0)
            {
                GetComponent<SpriteRenderer>().color = Color.green; // Attracted state
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.white; // Neutral state
            }
        }

        // PRIORITY LOGIC: 
        // If there is ANY fire nearby, the agent prioritizes escape.
        if (fireCount > 0)
        {
            // The agent "Panics" and ignores wool to escape the heat
            transform.Translate(aversionForce.normalized * (moveSpeed * 1.5f) * Time.deltaTime);
        }
        else if (woolCount > 0)
        {
            // If it's safe (no fire), it pursues the wool
            transform.Translate(attractionForce.normalized * moveSpeed * Time.deltaTime);
        }
    }

    // This draws the "Senses" in the editor for your class presentation
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
