using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    public GameObject woolPrefab;
    public GameObject firePrefab;

    void Update()
    {
        // Left Click = Drop Wool
        if (Input.GetMouseButtonDown(0))
        {
            SpawnObject(woolPrefab);
        }

        // Right Click = Drop Fire
        if (Input.GetMouseButtonDown(1))
        {
            SpawnObject(firePrefab);
        }
    }

    void SpawnObject(GameObject prefab)
    {
        // Convert Mouse Screen Position to World Position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Ensure it's on the 2D plane

        // Create the object
        Instantiate(prefab, mousePos, Quaternion.identity);
    }
}
