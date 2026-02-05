using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpiritController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Vector3 followOffset = new Vector3(-0.7f, 0.7f, 0);
    public float followSpeed = 4f;

    [Header("Action Settings")]
    public float actionSpeed = 8f;
    public GameObject firePrefab;

    private Vector3 targetPos;
    private bool isExecutingAction = false;

    void Update()
    {
        if (isExecutingAction)
        {
            // STATE: TRAVELING TO TARGET
            transform.position = Vector3.MoveTowards(transform.position, targetPos, actionSpeed * Time.deltaTime);

            // If arrived at target
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                ExecuteFire();
            }
        }
        else
        {
            // STATE: GUARDING WOOLDRIN (Floating by his side)
            float hover = Mathf.Sin(Time.time * 2f) * 0.2f;
            Vector3 guardPos = player.position + followOffset + new Vector3(0, hover, 0);
            transform.position = Vector3.Lerp(transform.position, guardPos, followSpeed * Time.deltaTime);
        }
    }

    // This is called by Wooldrin's AbilitySystem
    public void StartFireAction(Vector3 destination)
    {
        targetPos = destination;
        isExecutingAction = true;
    }

    void ExecuteFire()
    {
        // Drop the fire
        Instantiate(firePrefab, transform.position, Quaternion.identity);

        // Return to Wooldrin
        isExecutingAction = false;
    }
}