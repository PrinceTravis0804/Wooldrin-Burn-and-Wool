using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Final Fire Spirit Logic for the Dragon's Throat:
// - Optimized for tight corridors: Reduced collision buffers.
// - Orthogonal Pathfinding: Removed diagonals to prevent clipping through wall corners.
// - Right Click ability is DISABLED if the spirit is in 'Waiting' state.
// - Spirit must be retrieved by Wooldrin (get close) to re-enable the ability.
// - FIX: Spirit now ignores the player's collider during its dash and pathfinding checks.
public class FireSpiritController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Vector3 followOffset = new Vector3(-0.7f, 0.7f, 0);
    public float followSpeed = 10f;
    public float retrievalDistance = 1.2f;

    [Header("Action Settings")]
    public float actionSpeed = 12f;
    public GameObject firePrefab;
    public LayerMask wallLayer;
    public LayerMask enemyLayer;
    public float spiritRepelForce = 15f;

    [Header("Pathfinding (BFS)")]
    [Tooltip("Size of grid nodes. 0.35 provides better precision in narrow gaps.")]
    public float nodeSpacing = 0.35f;
    public int maxSearchIterations = 2000;

    private Animator animator;
    private bool isExecutingAction = false;
    private bool isWaitingAtLocation = false;
    private List<Vector3> currentPath = new List<Vector3>();
    private int pathIndex = 0;

    private Vector2 lastMoveDirection = Vector2.down;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        lastPosition = transform.position;

        // GHOST FIX: Hide any child sprites that aren't the animated one
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in childRenderers)
        {
            if (sr.gameObject != gameObject && sr.GetComponent<Animator>() == null)
            {
                sr.enabled = false;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. RETRIEVAL LOGIC
        if (isWaitingAtLocation && distToPlayer < retrievalDistance)
        {
            isWaitingAtLocation = false;
            Debug.Log("Spirit Retrieved! Ability Re-enabled.");
        }

        // 2. INPUT: Right Click (1)
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            StartFireAction(mousePos);
        }

        // 3. MOVEMENT LOGIC
        if (isExecutingAction)
        {
            MoveAlongPath();
            RepelNearbyEnemies();
        }
        else if (!isWaitingAtLocation)
        {
            HandleLockedFollow();
        }

        UpdateAnimations();
    }

    void HandleLockedFollow()
    {
        float hover = Mathf.Sin(Time.time * 2f) * 0.15f;
        Vector3 targetPos = player.position + followOffset + new Vector3(0, hover, 0);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            ExecuteFireAction();
            return;
        }

        Vector3 targetNode = currentPath[pathIndex];

        // Safety: Check for walls but ignore the player and the spirit itself
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.15f, wallLayer);
        if (hit != null && hit.transform != player && hit.transform != transform)
        {
            Debug.Log("Spirit hit a wall (" + hit.gameObject.name + ") during dash! Stopping.");
            StopAndStayPut();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetNode, actionSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetNode) < 0.1f)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                ExecuteFireAction();
            }
        }
    }

    void RepelNearbyEnemies()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.8f, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null) rb.AddForce(pushDir * spiritRepelForce);
        }
    }

    public void StartFireAction(Vector3 destination)
    {
        if (isWaitingAtLocation)
        {
            Debug.Log("Spirit Lock: Too far! You must walk to the spirit to retrieve it.");
            return;
        }

        if (isExecutingAction)
        {
            Debug.Log("Spirit Lock: Action already in progress.");
            return;
        }

        // Check if destination is a wall, but ignore player
        Collider2D destHit = Physics2D.OverlapPoint(destination, wallLayer);
        if (destHit != null && destHit.transform != player)
        {
            Debug.Log("Target is a wall! Action cancelled.");
            return;
        }

        isExecutingAction = true;
        isWaitingAtLocation = false;

        currentPath = FindBFSPath(transform.position, destination);
        pathIndex = 0;

        if (currentPath.Count == 0)
        {
            // If the grid search fails, allow a single-point direct dash
            currentPath.Add(destination);
        }
    }

    void ExecuteFireAction()
    {
        if (firePrefab != null) Instantiate(firePrefab, transform.position, Quaternion.identity);
        StopAndStayPut();
    }

    void StopAndStayPut()
    {
        currentPath.Clear();
        isExecutingAction = false;
        isWaitingAtLocation = true;
    }

    List<Vector3> FindBFSPath(Vector3 start, Vector3 end)
    {
        Queue<Vector3> queue = new Queue<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

        Vector3 startSnapped = SnapToGrid(start);
        Vector3 endSnapped = SnapToGrid(end);

        queue.Enqueue(startSnapped);
        cameFrom[startSnapped] = startSnapped;

        // Reverted to 4-directional movement (No diagonals) to prevent ignoring walls
        Vector3[] directions = {
            Vector3.up * nodeSpacing,
            Vector3.down * nodeSpacing,
            Vector3.left * nodeSpacing,
            Vector3.right * nodeSpacing
        };

        int iterations = 0;
        while (queue.Count > 0 && iterations < maxSearchIterations)
        {
            iterations++;
            Vector3 current = queue.Dequeue();

            if (Vector3.Distance(current, endSnapped) < nodeSpacing * 1.2f)
            {
                return ReconstructPath(cameFrom, current);
            }

            foreach (Vector3 dir in directions)
            {
                Vector3 next = current + dir;
                if (!cameFrom.ContainsKey(next))
                {
                    // Check for walls but allow the path to pass through the player
                    Collider2D hit = Physics2D.OverlapCircle(next, nodeSpacing * 0.45f, wallLayer);
                    if (hit == null || hit.transform == player || hit.transform == transform)
                    {
                        cameFrom[next] = current;
                        queue.Enqueue(next);
                    }
                }
            }
        }
        return new List<Vector3>();
    }

    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
    {
        List<Vector3> path = new List<Vector3>();
        while (cameFrom.ContainsKey(current) && cameFrom[current] != current)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x / nodeSpacing) * nodeSpacing, Mathf.Round(pos.y / nodeSpacing) * nodeSpacing, 0);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        Vector3 movementDelta = transform.position - lastPosition;
        float speed = (Time.deltaTime > 0) ? movementDelta.magnitude / Time.deltaTime : 0;
        if (speed > 0.1f) lastMoveDirection = movementDelta.normalized;
        animator.SetFloat("moveX", lastMoveDirection.x);
        animator.SetFloat("moveY", lastMoveDirection.y);
        animator.SetFloat("speed", speed > 0.1f ? 1f : 0f);
        lastPosition = transform.position;
    }

    void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < currentPath.Count - 1; i++) Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }
}