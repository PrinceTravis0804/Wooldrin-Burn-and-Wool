using UnityEngine;
using System.Collections;

public class AgentUtilityBrain : MonoBehaviour
{
    [Header("Stats")]
    public float baseSpeed = 2.5f;
    public float panicSpeed = 5.5f;
    public float roamRadius = 5f;
    public float huntRange = 7f;
    public float woolDetectionRange = 8f;
    public float biteStrength = 20f;

    [Header("Attack Settings")]
    public float attackReboundTime = 1.0f;
    [Tooltip("Adjust this to change the distance Wooldrin is pushed!")]
    public float knockbackPower = 15f;

    [Header("Audio")]
    public AudioSource hurtSource;
    public AudioClip hurtClip;
    public float volume = 0.7f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform player;
    private Vector2 wanderTarget;
    private bool isAttacking = false;
    private bool isWaiting = false;
    private bool isPanicked = false;
    private bool isEating = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (rb != null) rb.freezeRotation = true;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        PickNewWanderPoint();
    }

    void Update()
    {
        if (isAttacking)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }
        DetermineBehavior();
        if (sr != null)
        {
            if (isPanicked) sr.color = Color.red;
            else if (isEating) sr.color = Color.green;
            else sr.color = Color.white;
        }
    }

    void DetermineBehavior()
    {
        isPanicked = false;
        GameObject fire = FindClosestWithTagSafe("Fire", 6f);
        if (fire != null)
        {
            isPanicked = true; isWaiting = false; isEating = false;
            Vector2 escapeDir = ((Vector2)transform.position - (Vector2)fire.transform.position).normalized;
            Move(escapeDir, panicSpeed);
            return;
        }
        GameObject wool = FindClosestWithTagSafe("Wool", woolDetectionRange);
        if (wool != null)
        {
            isWaiting = false;
            if (!isEating) Move(((Vector2)wool.transform.position - (Vector2)transform.position).normalized, baseSpeed);
            else { rb.velocity = Vector2.zero; UpdateAnimation(Vector2.zero); }
            return;
        }
        if (player != null && Vector2.Distance(transform.position, player.position) < huntRange)
        {
            isWaiting = false; isEating = false;
            Move(((Vector2)player.position - (Vector2)transform.position).normalized, baseSpeed);
            return;
        }
        isEating = false;
        Roam();
    }

    private GameObject FindClosestWithTagSafe(string tag, float maxDist)
    {
        try
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            GameObject closest = null; float min = maxDist;
            foreach (var o in objs)
            {
                float d = Vector2.Distance(transform.position, o.transform.position);
                if (d < min) { min = d; closest = o; }
            }
            return closest;
        }
        catch { return null; }
    }

    void Move(Vector2 dir, float speed) { rb.velocity = dir * speed; UpdateAnimation(dir); }

    void UpdateAnimation(Vector2 dir)
    {
        if (anim == null) return;
        if (dir.sqrMagnitude > 0.01f) { anim.SetFloat("moveX", dir.x); anim.SetFloat("moveY", dir.y); }
        anim.SetFloat("speed", dir.sqrMagnitude);
    }

    void Roam()
    {
        if (isWaiting) { rb.velocity = Vector2.zero; UpdateAnimation(Vector2.zero); return; }
        if (Vector2.Distance(transform.position, wanderTarget) < 0.5f) StartCoroutine(WaitRoutine());
        else Move((wanderTarget - (Vector2)transform.position).normalized, baseSpeed * 0.6f);
    }

    IEnumerator WaitRoutine() { isWaiting = true; yield return new WaitForSeconds(Random.Range(1f, 3f)); isWaiting = false; PickNewWanderPoint(); }
    void PickNewWanderPoint() { wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * roamRadius; }

    public void TakeDamage()
    {
        if (anim != null) anim.SetTrigger("isHurt");

        if (hurtSource != null && hurtClip != null)
        {
            hurtSource.PlayOneShot(hurtClip, volume);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("FireSpirit"))
        {            
            TakeDamage(); 
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && !isAttacking)
        {
            WooldrinHealth health = c.gameObject.GetComponent<WooldrinHealth>();
            if (health != null && !health.IsInvulnerable)
            {
                // Passing the slime's specific knockbackPower to the Health script
                health.TakeDamage(transform.position, knockbackPower);
                StartCoroutine(AttackRebound());
            }
        }
        if (c.gameObject.CompareTag("Wool")) c.gameObject.GetComponent<WoolResource>()?.SetEatingState(true);
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Wool")) { isEating = true; rb.velocity = Vector2.zero; c.gameObject.GetComponent<WoolResource>()?.TakeBite(biteStrength * Time.deltaTime); }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Wool")) { isEating = false; c.gameObject.GetComponent<WoolResource>()?.SetEatingState(false); }
    }

    IEnumerator AttackRebound() { isAttacking = true; rb.velocity = Vector2.zero; yield return new WaitForSeconds(attackReboundTime); isAttacking = false; }
}