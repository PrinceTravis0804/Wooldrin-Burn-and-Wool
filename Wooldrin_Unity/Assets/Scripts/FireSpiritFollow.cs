using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpiritController : MonoBehaviour
{
    [Header("Speeds")]
    public float followSpeed = 6f;
    public float actionSpeed = 10f;
    public float returnSpeed = 15f;

    [Header("Follow Settings")]
    public Transform player;
    public Vector3 followOffset = new Vector3(-0.7f, 0.7f, 0);
    public float lockOnProximity = 1.2f;

    [Header("Action Settings")]
    public GameObject firePrefab;

    [Header("BFS Pathfinding (Physics-Based)")]
    public float nodeSize = 0.3f;
    [Range(100, 5000)]
    public int searchLimit = 400;

    [Header("Visualization")]
    public bool showSearchRange = true;
    public Color gizmoColor = new Color(1, 1, 0, 0.2f);

    [Header("State")]
    public bool isReady = true;
    private bool isExecutingAction = false;
    private bool isReturning = false;

    private Animator animator;
    private List<Vector3> currentPath = new List<Vector3>();
    private int pathIndex = 0;
    private Vector2 lastDir;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        lastPosition = transform.position;
        if (currentPath == null) currentPath = new List<Vector3>();
    }

    void Update()
    {
        if (player == null) return;

        // --- NEW: RIGHT CLICK INPUT ---
        if (Input.GetMouseButtonDown(1) && isReady)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            StartFireAction(mousePos);
        }

        Vector3 guardTarget = player.position + followOffset;
        float distToWooldrin = Vector3.Distance(transform.position, player.position);

        if (distToWooldrin < lockOnProximity && !isExecutingAction)
        {
            ResetToReady();
        }

        if (isExecutingAction || isReturning)
        {
            MoveAlongPath();
        }
        else
        {
            if (HasLineOfSight(transform.position, guardTarget))
            {
                float hover = Mathf.Sin(Time.time * 3f) * 0.15f;
                Vector3 target = guardTarget + new Vector3(0, hover, 0);
                transform.position = Vector3.MoveTowards(transform.position, target, followSpeed * Time.deltaTime);
            }
            else
            {
                isReturning = true;
                currentPath = FindPathBFS(transform.position, guardTarget);
                pathIndex = 0;
            }
        }

        UpdateAnims();
    }

    public void StartFireAction(Vector3 destination)
    {
        isReady = false;
        isReturning = false;

        if (IsPointBlocked(destination))
        {
            ResetToReady();
            return;
        }

        currentPath = FindPathBFS(transform.position, destination);

        if (currentPath == null || currentPath.Count == 0)
        {
            ResetToReady();
            return;
        }

        isExecutingAction = true;
        pathIndex = 0;
        Debug.Log("FireSpirit: Flying to destination!");
    }

    void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            if (isExecutingAction) DropFireAndReturn();
            else ResetToReady();
            return;
        }

        float currentMoveSpeed = isExecutingAction ? actionSpeed : returnSpeed;
        Vector3 targetNode = currentPath[pathIndex];

        Vector3 nextStep = Vector3.MoveTowards(transform.position, targetNode, currentMoveSpeed * Time.deltaTime);

        if (!IsPointBlocked(nextStep))
        {
            transform.position = nextStep;
        }

        if (Vector3.Distance(transform.position, targetNode) < 0.05f)
        {
            pathIndex++;
        }
    }

    void DropFireAndReturn()
    {
        if (firePrefab != null) Instantiate(firePrefab, transform.position, Quaternion.identity);
        isExecutingAction = false;
        isReturning = true;
        currentPath = FindPathBFS(transform.position, player.position + followOffset);
        pathIndex = 0;
    }

    void ResetToReady()
    {
        isExecutingAction = false;
        isReturning = false;
        isReady = true;
        if (currentPath != null) currentPath.Clear();
    }

    List<Vector3> FindPathBFS(Vector3 start, Vector3 end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        Vector2Int startGrid = WorldToGrid(start);
        Vector2Int endGrid = WorldToGrid(end);

        queue.Enqueue(startGrid);
        cameFrom[startGrid] = startGrid;

        bool found = false;
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == endGrid) { found = true; break; }
            if (cameFrom.Count > searchLimit) break;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    if (!IsPointBlocked(GridToWorld(neighbor)))
                    {
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        if (!found) return null;

        List<Vector3> path = new List<Vector3>();
        Vector2Int curr = endGrid;
        while (curr != startGrid)
        {
            path.Add(GridToWorld(curr));
            curr = cameFrom[curr];
        }
        path.Reverse();
        return path;
    }

    bool IsPointBlocked(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(worldPos, nodeSize * 0.4f);
        if (hit == null || hit.isTrigger || hit.CompareTag("Player") || hit.CompareTag("Wool") || hit.CompareTag("Fire") || hit.gameObject == gameObject)
        {
            return false;
        }
        return true;
    }

    bool HasLineOfSight(Vector3 start, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, (end - start).normalized, dist);

        foreach (var hit in hits)
        {
            if (!hit.collider.isTrigger &&
                !hit.collider.CompareTag("Player") &&
                !hit.collider.CompareTag("Wool") &&
                !hit.collider.CompareTag("Fire") &&
                hit.collider.gameObject != gameObject)
            {
                return false;
            }
        }
        return true;
    }

    IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        yield return new Vector2Int(pos.x + 1, pos.y);
        yield return new Vector2Int(pos.x - 1, pos.y);
        yield return new Vector2Int(pos.x, pos.y + 1);
        yield return new Vector2Int(pos.x, pos.y - 1);
    }

    Vector2Int WorldToGrid(Vector3 pos) => new Vector2Int(Mathf.RoundToInt(pos.x / nodeSize), Mathf.RoundToInt(pos.y / nodeSize));
    Vector3 GridToWorld(Vector2Int pos) => new Vector3(pos.x * nodeSize, pos.y * nodeSize, 0);

    void OnDrawGizmos()
    {
        if (!showSearchRange) return;
        Gizmos.color = gizmoColor;
        float estimatedRadius = Mathf.Sqrt((searchLimit * (nodeSize * nodeSize)) / Mathf.PI);
        Gizmos.DrawWireSphere(transform.position, estimatedRadius);
    }

    void UpdateAnims()
    {
        if (animator == null) return;
        Vector3 delta = transform.position - lastPosition;
        if (delta.sqrMagnitude > 0.001f) lastDir = delta.normalized;
        animator.SetFloat("moveX", lastDir.x);
        animator.SetFloat("moveY", lastDir.y);
        animator.SetFloat("speed", delta.sqrMagnitude > 0.001f ? 1 : 0);
        lastPosition = transform.position;
    }
}