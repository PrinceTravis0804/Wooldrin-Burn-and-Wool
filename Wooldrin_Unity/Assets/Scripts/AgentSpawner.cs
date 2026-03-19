using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public float spawnRate = 5f;       // Seconds between spawns
    public int maxAgents = 10;         // Prevent the game from lagging
    public Vector2 spawnArea = new Vector2(10, 5); // Width and height of spawn zone

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // Count how many agents are currently alive
        int currentAgentCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (timer >= spawnRate && currentAgentCount < maxAgents)
        {
            SpawnAgent();
            timer = 0;
        }
    }

    void SpawnAgent()
    {
        // Pick a random spot within the defined rectangle
        float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
        float y = Random.Range(-spawnArea.y / 2, spawnArea.y / 2);
        Vector3 spawnPos = transform.position + new Vector3(x, y, 0);

        Instantiate(agentPrefab, spawnPos, Quaternion.identity);
    }

    // Draws the spawn zone in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 1));
    }
}