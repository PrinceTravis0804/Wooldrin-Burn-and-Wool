using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DyingSheepNPC : MonoBehaviour
{
    public enum TutorialState
    {
        DyingWaiting,
        Approaching,
        DyingDialogue,
        Transforming,
        Tutorial_Movement,
        Tutorial_Wool,
        Completed,
        Leaving
    }

    [Header("Current State")]
    public TutorialState currentState = TutorialState.DyingWaiting;

    [Header("Visual Transitions")]
    public SpriteRenderer physicalSheepRenderer;
    public Sprite deadCarcassSprite;
    public int deadCarcassSortingOrder = -10;
    public GameObject ghostGuideObject;
    public GameObject transformationParticles;

    [Header("Auto-Walk Cutscene Settings")]
    public Vector3 offsetFromElder = new Vector3(-1.2f, 0f, 0f);
    public float autoWalkSpeed = 3.0f;

    [Header("Movement & Floating (Ghost)")]
    public float ghostFollowSpeed = 4f;
    public Vector3 ghostOffset = new Vector3(0.8f, 0.8f, 0f);
    public float ghostBobSpeed = 4f;
    public float ghostBobAmount = 0.12f;

    [Header("Departure Animation")]
    public float ascendHeight = 4.0f;
    public float fadeOutDuration = 2.5f;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip coughClip;
    public AudioClip transformClip;
    public AudioClip ghostTalkClip;
    public AudioClip departureClip;

    [Header("Dialogue")]
    [TextArea(2, 4)] public string[] dyingWords = new string[] { "Cough... wheeze... Wooldrin? Is... is that you, child?", "The great dragon... it swallowed our entire flock... I'm going to die soon...", "My spirit will guide you through this throat." };
    [TextArea(2, 4)] public string movementPrompt = "Try moving around with your WASD or Arrow Keys. Explore the space!";
    [TextArea(2, 4)] public string woolPrompt = "Splendid! Now, press SPACE to drop a patch of Wool. You can push it, it acts as a decoy for slimes!";
    [TextArea(2, 4)] public string finalBlessing = "You are ready, wooldrin. May your spirit remain strong. Farewell!";

    public float detectionRadius = 3.5f;

    private Transform playerTransform;
    private PlayerController playerController;
    private Animator playerAnimator;
    private Rigidbody2D playerRb;
    private bool interactionDebounce = false;
    private float bobTimer = 0f;
    private Vector3 cutsceneTargetWorldPos;
    private Vector2 startPlayerPos;
    private bool hasMovedTargetDistance = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
            playerAnimator = playerObj.GetComponent<Animator>();
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            startPlayerPos = playerTransform.position;
        }
        if (ghostGuideObject != null) ghostGuideObject.SetActive(false);
    }

    private void Update()
    {
        switch (currentState)
        {
            case TutorialState.DyingWaiting: HandleProximityInteract(); break;
            case TutorialState.Approaching: ExecuteAutoWalk(); break;
            case TutorialState.Tutorial_Movement:
                FollowPlayerWithGhost();
                CheckMovementCondition();
                break;
            case TutorialState.Tutorial_Wool:
                FollowPlayerWithGhost();
                CheckWoolCondition();
                break;
            case TutorialState.Completed:
                // Just let the Leaving state handle the fade out
                break;
        }
    }

    private void HandleProximityInteract()
    {
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= detectionRadius && !interactionDebounce)
        {
            interactionDebounce = true;
            InitializeAutoWalkCutscene();
        }
    }

    private void InitializeAutoWalkCutscene()
    {
        currentState = TutorialState.Approaching;
        cutsceneTargetWorldPos = transform.position + offsetFromElder;
        if (playerController != null) playerController.enabled = false;
        if (playerRb != null) playerRb.velocity = Vector2.zero;
    }

    private void ExecuteAutoWalk()
    {
        if (Vector3.Distance(playerTransform.position, cutsceneTargetWorldPos) > 0.05f)
        {
            Vector2 direction = ((Vector2)cutsceneTargetWorldPos - (Vector2)playerTransform.position).normalized;
            if (playerAnimator != null) { playerAnimator.SetFloat("moveX", direction.x); playerAnimator.SetFloat("speed", 1.0f); }
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, cutsceneTargetWorldPos, autoWalkSpeed * Time.deltaTime);
        }
        else
        {
            if (playerAnimator != null) playerAnimator.SetFloat("speed", 0f);
            if (playerController != null) playerController.enabled = true;
            TriggerDyingDialogue();
        }
    }

    private void TriggerDyingDialogue()
    {
        currentState = TutorialState.DyingDialogue;
        if (sfxSource != null) sfxSource.PlayOneShot(coughClip);
        List<DialogueManager.DialogueLine> lines = new List<DialogueManager.DialogueLine>();
        foreach (string text in dyingWords) lines.Add(new DialogueManager.DialogueLine("Elder Sheep", text));

        DialogueManager.Instance.StartDialogueSequence(lines, () => StartCoroutine(ExecuteTransformationSequence()));
    }

    private IEnumerator ExecuteTransformationSequence()
    {
        currentState = TutorialState.Transforming;
        if (transformationParticles != null) Instantiate(transformationParticles, transform.position, Quaternion.identity);

        if (physicalSheepRenderer != null) { physicalSheepRenderer.sprite = deadCarcassSprite; physicalSheepRenderer.sortingOrder = deadCarcassSortingOrder; }

        yield return new WaitForSeconds(1.0f);
        if (ghostGuideObject != null) ghostGuideObject.SetActive(true);

        // Reset the player position tracking exactly when the ghost appears
        startPlayerPos = playerTransform.position;

        // Trigger the movement stage through our dedicated method so state is set properly
        TriggerTutorialStage(TutorialState.Tutorial_Movement, "Elder Ghost", movementPrompt);
    }

    private void TriggerTutorialStage(TutorialState stage, string speaker, string instruction)
    {
        // CRITICAL FIX: Ensure the state actually changes so the update loop moves on!
        currentState = stage;

        // Reset trackers when entering a new tutorial stage
        hasMovedTargetDistance = false;
        startPlayerPos = playerTransform.position;

        if (sfxSource != null && ghostTalkClip != null) sfxSource.PlayOneShot(ghostTalkClip);
        DialogueManager.Instance.ShowDialogue(speaker, instruction);

        if (stage == TutorialState.Tutorial_Wool)
        {
            if (playerController != null) playerController.UnlockWoolAbility();
        }
    }

    private void FollowPlayerWithGhost()
    {
        if (ghostGuideObject == null || playerTransform == null) return;
        ghostGuideObject.transform.position = Vector3.Lerp(ghostGuideObject.transform.position, playerTransform.position + ghostOffset, ghostFollowSpeed * Time.deltaTime);
        bobTimer += Time.deltaTime * ghostBobSpeed;
        ghostGuideObject.transform.localPosition += new Vector3(0f, Mathf.Sin(bobTimer) * ghostBobAmount * Time.deltaTime, 0f);
    }

    private void CheckMovementCondition()
    {
        // If dialogue is active, do NOT check for movement yet
        if (DialogueManager.Instance.IsDialogueActive) return;
        if (playerTransform == null) return;

        float distanceTraveled = Vector3.Distance(startPlayerPos, playerTransform.position);

        if (distanceTraveled >= 3.0f && !hasMovedTargetDistance)
        {
            hasMovedTargetDistance = true;
            TriggerTutorialStage(TutorialState.Tutorial_Wool, "Elder Ghost", woolPrompt);
        }
    }

    private void CheckWoolCondition()
    {
        // If dialogue is active, do NOT check for wool yet
        if (DialogueManager.Instance.IsDialogueActive) return;

        if (playerController != null && playerController.hasActiveWool)
        {
            // Change state to Completed immediately after wool is placed to prevent loops
            currentState = TutorialState.Completed;

            // Show farewell dialogue and trigger departure on close
            DialogueManager.Instance.ShowDialogue("Elder Ghost", finalBlessing, () => {
                StartCoroutine(ExecuteGhostLeavingSequence());
            });
        }
    }

    private IEnumerator ExecuteGhostLeavingSequence()
    {
        currentState = TutorialState.Leaving;

        if (sfxSource != null) sfxSource.PlayOneShot(departureClip);
        SpriteRenderer ghostRenderer = ghostGuideObject.GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = ghostGuideObject.transform.position;
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            ghostGuideObject.transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * ascendHeight, elapsed / fadeOutDuration);
            if (ghostRenderer != null) ghostRenderer.color = new Color(1, 1, 1, 1 - (elapsed / fadeOutDuration));
            yield return null;
        }
        Destroy(ghostGuideObject);
        this.enabled = false;
    }
}