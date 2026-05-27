using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DyingSheepNPC : MonoBehaviour
{
    public enum TutorialState
    {
        DyingWaiting,
        Approaching, // Moves Wooldrin automatically into a perfect frame
        DyingDialogue,
        Transforming,
        Tutorial_Movement,
        Tutorial_Wool,
        Tutorial_Fire,
        Tutorial_AvoidEnemies,
        Completed,
        Leaving
    }

    [Header("Current State")]
    public TutorialState currentState = TutorialState.DyingWaiting;

    [Header("Visual Transitions")]
    [Tooltip("The main Sprite Renderer representing the physical old sheep.")]
    public SpriteRenderer physicalSheepRenderer;
    [Tooltip("The Sprite representing the old sheep's carcass/bones after dying.")]
    public Sprite deadCarcassSprite;
    [Tooltip("The sorting order of the carcass after death so it renders behind Wooldrin (e.g. -10).")]
    public int deadCarcassSortingOrder = -10;
    [Tooltip("The Ghost GameObject that floats up and follows the player.")]
    public GameObject ghostGuideObject;
    [Tooltip("Particle effect instantiated during the death/transformation puff.")]
    public GameObject transformationParticles;

    [Header("Auto-Walk Cutscene Settings")]
    [Tooltip("Where Wooldrin should walk to relative to the Elder Sheep (e.g. -1.2 on X places him just to the left).")]
    public Vector3 offsetFromElder = new Vector3(-1.2f, 0f, 0f);
    [Tooltip("How fast Wooldrin walks during the automated cutscene.")]
    public float autoWalkSpeed = 3.0f;

    [Header("Movement & Floating (Ghost)")]
    [Tooltip("How fast the ghost floats to keep up with Wooldrin.")]
    public float ghostFollowSpeed = 4f;
    [Tooltip("The distance the ghost maintains from Wooldrin.")]
    public Vector3 ghostOffset = new Vector3(0.8f, 0.8f, 0f);
    [Tooltip("Speed of the ghost's hovering/bobbing motion.")]
    public float ghostBobSpeed = 4f;
    [Tooltip("Intensity/height of the ghost's hovering/bobbing motion.")]
    public float ghostBobAmount = 0.12f;

    [Header("Departure Animation")]
    [Tooltip("How high the ghost floats up into the ceiling during departure.")]
    public float ascendHeight = 4.0f;
    [Tooltip("How many seconds the fade-out and float animation takes.")]
    public float fadeOutDuration = 2.5f;

    [Header("Sound Effects (Optional)")]
    public AudioSource sfxSource;
    public AudioClip coughClip;
    public AudioClip transformClip;
    public AudioClip ghostTalkClip;
    public AudioClip departureClip;

    [Header("Dialogue Sequences")]
    [TextArea(2, 4)]
    public string[] dyingWords = new string[]
    {
        "Cough... wheeze... Wooldrin? Is... is that you, child?",
        "The great dragon... it swallowed our entire flock... I cannot go any further...",
        "Listen closely... my physical shell is fading, but my spirit will guide you through this throat.",
        "Use your instincts... and remember what Mother taught us..."
    };

    [TextArea(2, 4)]
    public string movementPrompt = "Try moving around with your WASD or Arrow Keys. Explore the space!";
    [TextArea(2, 4)]
    public string woolPrompt = "Splendid! Now, try dropping a patch of Wool (Left Click). It acts as a decoy and decoy food for slimes!";
    [TextArea(2, 4)]
    public string firePrompt = "Excellent. Now, target a point and Right-Click to command your Fire Spirit! He will clear toxic pathways and light your way.";
    [TextArea(2, 4)]
    public string enemyPrompt = "Beware! Slimes roam these dark tunnels. If they touch you, they will eat your wool layers. Use wool decoys and your fire spirit to keep them away!";
    [TextArea(2, 4)]
    public string finalBlessing = "You are ready, child. Let's find Sister Baa and get out of this burning belly! My spirit must rest now... Farewell!";

    // Range references
    public float detectionRadius = 3.5f; // Increased slightly to give Wooldrin space to walk up smoothly

    private Transform playerTransform;
    private PlayerController playerController;
    private Animator playerAnimator;
    private Rigidbody2D playerRb;

    private bool interactionDebounce = false;
    private Vector3 ghostSpawnPosition;
    private float bobTimer = 0f;
    private Vector3 cutsceneTargetWorldPos;

    // Input tracking variables for tutorial verification
    private Vector2 startPlayerPos;
    private bool hasMovedTargetDistance = false;

    private void Start()
    {
        // Find player and cache all movement/animation components
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
            playerAnimator = playerObj.GetComponent<Animator>();
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            startPlayerPos = playerTransform.position;
        }

        // Keep the ghost guide hidden initially
        if (ghostGuideObject != null)
        {
            ghostGuideObject.SetActive(false);
        }

        // Link audio source if empty
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case TutorialState.DyingWaiting:
                // Wait for player to approach
                HandleProximityInteract();
                break;

            case TutorialState.Approaching:
                // Execute automated cutscene walk towards the elder
                ExecuteAutoWalk();
                break;

            case TutorialState.DyingDialogue:
                // Dialogue manager is active, do nothing
                break;

            case TutorialState.Transforming:
                // Handled by coroutines
                break;

            case TutorialState.Tutorial_Movement:
                // Follow player and check if player has moved a minimum distance
                FollowPlayerWithGhost();
                CheckMovementCondition();
                break;

            case TutorialState.Tutorial_Wool:
                FollowPlayerWithGhost();
                CheckWoolCondition();
                break;

            case TutorialState.Tutorial_Fire:
                FollowPlayerWithGhost();
                CheckFireCondition();
                break;

            case TutorialState.Tutorial_AvoidEnemies:
                FollowPlayerWithGhost();
                CheckEnemyWarningCondition();
                break;

            case TutorialState.Completed:
                FollowPlayerWithGhost();
                break;

            case TutorialState.Leaving:
                // Animated independently via the ExecuteGhostLeavingSequence coroutine
                break;
        }
    }

    private void HandleProximityInteract()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        // Auto-trigger the cutscene walk when the player walks into detection range
        if (dist <= detectionRadius && !interactionDebounce)
        {
            interactionDebounce = true;
            InitializeAutoWalkCutscene();
        }
    }

    private void InitializeAutoWalkCutscene()
    {
        currentState = TutorialState.Approaching;

        // Calculate absolute target coordinate in world space relative to Elder Sheep
        cutsceneTargetWorldPos = transform.position + offsetFromElder;

        // Temporarily disable Player Controller movement script so they can't fight the cutscene
        if (playerController != null) playerController.enabled = false;

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
        }

        Debug.Log("<color=cyan>DyingSheepNPC:</color> Initialized Auto-Walk. Moving Wooldrin in-frame.");
    }

    private void ExecuteAutoWalk()
    {
        if (playerTransform == null) return;

        float distanceToTarget = Vector3.Distance(playerTransform.position, cutsceneTargetWorldPos);

        if (distanceToTarget > 0.05f)
        {
            // Calculate direction and update the animator parameters so walk cycles play properly
            Vector2 direction = ((Vector2)cutsceneTargetWorldPos - (Vector2)playerTransform.position).normalized;
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("moveX", direction.x);
                playerAnimator.SetFloat("moveY", direction.y);
                playerAnimator.SetFloat("speed", 1.0f); // Switch animator state to Walk
            }

            // Translate player position physically
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                cutsceneTargetWorldPos,
                autoWalkSpeed * Time.deltaTime
            );
        }
        else
        {
            // Arrived safely! Let's shut down movement states and trigger the dialogue.
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("speed", 0f); // Switch animator state to Idle
            }

            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }

            // Restore Player control components (they will instantly freeze because dialogue opens)
            if (playerController != null) playerController.enabled = true;

            TriggerDyingDialogue();
        }
    }

    private void TriggerDyingDialogue()
    {
        currentState = TutorialState.DyingDialogue;

        if (sfxSource != null && coughClip != null)
        {
            sfxSource.PlayOneShot(coughClip);
        }

        // Convert raw string array to DialogueManager list structure
        List<DialogueManager.DialogueLine> lines = new List<DialogueManager.DialogueLine>();
        foreach (string text in dyingWords)
        {
            lines.Add(new DialogueManager.DialogueLine("Elder Sheep", text));
        }

        // Hook completion callback to trigger transformation when finished reading
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueSequence(lines, () =>
            {
                StartCoroutine(ExecuteTransformationSequence());
            });
        }
        else
        {
            Debug.LogWarning("DyingSheepNPC: DialogueManager instance not found! Speed-skipping to transformation.");
            StartCoroutine(ExecuteTransformationSequence());
        }
    }

    private IEnumerator ExecuteTransformationSequence()
    {
        currentState = TutorialState.Transforming;
        ghostSpawnPosition = transform.position;

        // Play dramatic transformation effects
        if (transformationParticles != null)
        {
            Instantiate(transformationParticles, transform.position, Quaternion.identity);
        }

        if (sfxSource != null && transformClip != null)
        {
            sfxSource.PlayOneShot(transformClip);
        }

        // Swap physical sheep to dead bones/carcass representation and send it to the background
        if (physicalSheepRenderer != null)
        {
            if (deadCarcassSprite != null)
            {
                physicalSheepRenderer.sprite = deadCarcassSprite;
            }

            // Set the sorting order way below active entities so Wooldrin walks on top of it
            physicalSheepRenderer.sortingOrder = deadCarcassSortingOrder;
        }

        yield return new WaitForSeconds(1.0f);

        // Turn on the ghost visual object and float it upward slightly
        if (ghostGuideObject != null)
        {
            ghostGuideObject.transform.position = ghostSpawnPosition;
            ghostGuideObject.SetActive(true);

            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 startPos = ghostGuideObject.transform.position;
            Vector3 endPos = startPos + Vector3.up * 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ghostGuideObject.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }
        }

        // Begin Tutorial Stage 1: Movement
        TriggerTutorialStage(TutorialState.Tutorial_Movement, "Elder Ghost", movementPrompt);
    }

    private void TriggerTutorialStage(TutorialState stage, string speaker, string instruction)
    {
        currentState = stage;

        if (sfxSource != null && ghostTalkClip != null)
        {
            sfxSource.PlayOneShot(ghostTalkClip);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue(speaker, instruction);
        }
        else
        {
            Debug.Log($"[TUTORIAL] {speaker}: {instruction}");
        }

        // Lock starting player position for movement checks
        if (stage == TutorialState.Tutorial_Movement && playerTransform != null)
        {
            startPlayerPos = playerTransform.position;
        }
    }

    private void FollowPlayerWithGhost()
    {
        if (ghostGuideObject == null || playerTransform == null) return;

        // Determine target position relative to Wooldrin's current spot
        Vector3 targetPos = playerTransform.position + ghostOffset;

        // Smoothly interpolate position towards player
        ghostGuideObject.transform.position = Vector3.Lerp(
            ghostGuideObject.transform.position,
            targetPos,
            ghostFollowSpeed * Time.deltaTime
        );

        // Apply a gentle floating bobbing motion using Sine math
        bobTimer += Time.deltaTime * ghostBobSpeed;
        ghostGuideObject.transform.localPosition += new Vector3(0f, Mathf.Sin(bobTimer) * ghostBobAmount * Time.deltaTime, 0f);
    }

    // --- REACTIVE INPUT VERIFICATION CHECKS ---

    private void CheckMovementCondition()
    {
        if (playerTransform == null) return;

        float distanceTraveled = Vector3.Distance(startPlayerPos, playerTransform.position);

        // Wait for player to move at least 3.0 grid units away
        if (distanceTraveled >= 3.0f && !hasMovedTargetDistance)
        {
            hasMovedTargetDistance = true;
            TriggerTutorialStage(TutorialState.Tutorial_Wool, "Elder Ghost", woolPrompt);
        }
    }

    private void CheckWoolCondition()
    {
        if (playerController == null) return;

        // If player has successfully active wool, advance!
        if (playerController.hasActiveWool)
        {
            TriggerTutorialStage(TutorialState.Tutorial_Fire, "Elder Ghost", firePrompt);
        }
    }

    private void CheckFireCondition()
    {
        // Get the FireSpiritController component in the scene
        FireSpiritController spirit = FindObjectOfType<FireSpiritController>();

        // Check if the spirit is executing an action (i.e. flying to right-clicked target)
        if (spirit != null && !spirit.isReady)
        {
            TriggerTutorialStage(TutorialState.Tutorial_AvoidEnemies, "Elder Ghost", enemyPrompt);
        }
    }

    private void CheckEnemyWarningCondition()
    {
        // Once the enemy warning is clicked through/closed, move to Completed sequence and play farewell dialogue
        if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive)
        {
            currentState = TutorialState.Completed;

            if (sfxSource != null && ghostTalkClip != null)
            {
                sfxSource.PlayOneShot(ghostTalkClip);
            }

            // Play the farewell dialogue and hook its completion callback to the leaving sequence
            DialogueManager.Instance.ShowDialogue("Elder Ghost", finalBlessing, () =>
            {
                StartCoroutine(ExecuteGhostLeavingSequence());
            });
        }
    }

    /// <summary>
    /// Coroutine handling the smooth floating ascension and opacity fading of the ghost.
    /// Leaves the carcass sprite on the floor as static atmospheric scenery, but completely clears out the ghost guide.
    /// </summary>
    private IEnumerator ExecuteGhostLeavingSequence()
    {
        currentState = TutorialState.Leaving;

        if (ghostGuideObject == null) yield break;

        // Play ascension sound
        if (sfxSource != null && departureClip != null)
        {
            sfxSource.PlayOneShot(departureClip);
        }

        SpriteRenderer ghostRenderer = ghostGuideObject.GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = ghostGuideObject.transform.position;
        Vector3 endPos = startPos + Vector3.up * ascendHeight;

        float elapsed = 0f;
        Color startColor = Color.white;

        if (ghostRenderer != null)
        {
            startColor = ghostRenderer.color;
        }

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            // Float up smoothly
            ghostGuideObject.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Interpolate alpha opacity down to 0
            if (ghostRenderer != null)
            {
                Color tempColor = startColor;
                tempColor.a = Mathf.Lerp(startColor.a, 0f, t);
                ghostRenderer.color = tempColor;
            }

            yield return null;
        }

        // Completely clean up the ghost visual object
        Destroy(ghostGuideObject);

        // Turn off this script component so Update loops completely halt
        this.enabled = false;

        Debug.Log("<color=cyan>Tutorial Companion NPC:</color> Ghost ascended successfully. Carcass remains on floor.");
    }
}