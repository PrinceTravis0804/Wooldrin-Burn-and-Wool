using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject woolPrefab;
    public GameObject firePrefab;
    public Transform fireSpirit; // Drag the Spirit object here in Inspector

    [Header("Settings")]
    public float dropDistance = 0.5f;
    public float moveSpeed = 5f;

    private Vector3 targetLocation;
    private bool isMovingToDrop = false;

    void Update()
    {
        // LEFT CLICK: Wooldrin goes to drop Wool
        if (Input.GetMouseButtonDown(0))
        {
            targetLocation = GetMouseWorldPos();
            isMovingToDrop = true;
        }

        // RIGHT CLICK: Fire Spirit targets the location
        if (Input.GetMouseButtonDown(1))
        {
            SpawnFire(GetMouseWorldPos());
        }

        // Handle physical movement for Wool
        if (isMovingToDrop)
        {
            MoveToWoolSpot();
        }
    }

    void MoveToWoolSpot()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetLocation, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetLocation) < dropDistance)
        {
            Instantiate(woolPrefab, targetLocation, Quaternion.identity);
            isMovingToDrop = false;
        }
    }

    void SpawnFire(Vector3 pos)
    {
        if (fireSpirit != null)
        {
            // Tell the spirit to go do the work!
            fireSpirit.GetComponent<FireSpiritController>().StartFireAction(pos);
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }
}