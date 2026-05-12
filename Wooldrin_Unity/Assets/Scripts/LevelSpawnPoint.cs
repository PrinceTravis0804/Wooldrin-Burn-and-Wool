using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        // As soon as this object exists in a new level, 
        // it tells the GameManager: "Use my position!"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RequestSpawn(this.transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        // Visual aid in the editor (Blue Diamond)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.down);
        Gizmos.DrawLine(transform.position + Vector3.left, transform.position + Vector3.right);
    }
}