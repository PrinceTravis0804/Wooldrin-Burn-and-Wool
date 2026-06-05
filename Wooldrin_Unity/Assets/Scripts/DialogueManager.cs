using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The main parent panel containing the Dialogue UI.")]
    public GameObject dialogueBoxParent;
    [Tooltip("TMP text component for the Speaker's Name panel.")]
    public TextMeshProUGUI speakerNameText;
    [Tooltip("TMP text component for the Main Dialogue body.")]
    public TextMeshProUGUI dialogueBodyText;
    [Tooltip("Optional panel container holding the speaker name text.")]
    public GameObject namePanelBox;

    [Header("Typewriter Settings")]
    [Tooltip("Time delay in seconds between each character appearing.")]
    public float typingSpeed = 0.03f;
    [Tooltip("Optional sound effect played per character reveal for retro feedback.")]
    public AudioSource typingAudioSource;
    [Tooltip("Volume level for the typing clicks.")]
    [Range(0f, 1f)] public float typingVolume = 0.5f;

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    private Action onDialogueCompleteCallback;

    public bool IsDialogueActive { get; private set; } = false;

    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(3, 5)]
        public string textContent;

        public DialogueLine(string name, string content)
        {
            speakerName = name;
            textContent = content;
        }
    }

    private void Awake()
    {
        // Simple non-persistent singleton. This allows the local copy in 
        // each scene to register itself as the active instance automatically on load.
        Instance = this;
    }

    private void Start()
    {
        // Hide dialogue box automatically on level start
        if (dialogueBoxParent != null)
        {
            dialogueBoxParent.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

        // Press Space, Left Click, or Return to skip typing or advance text
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                CompleteCurrentLineInstantly();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    public void ShowDialogue(string speaker, string content, Action onComplete = null)
    {
        List<DialogueLine> singleLineList = new List<DialogueLine>
        {
            new DialogueLine(speaker, content)
        };
        StartDialogueSequence(singleLineList, onComplete);
    }

    public void StartDialogueSequence(List<DialogueLine> lines, Action onComplete = null)
    {
        if (dialogueBoxParent == null)
        {
            Debug.LogError("DialogueManager: Dialogue Box Parent reference is missing from the Inspector!");
            return;
        }

        onDialogueCompleteCallback = onComplete;
        dialogueQueue.Clear();

        foreach (var line in lines)
        {
            dialogueQueue.Enqueue(line);
        }

        IsDialogueActive = true;
        dialogueBoxParent.SetActive(true);

        // Instantly halt player physical movement velocities when entering talk mode
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.velocity = Vector2.zero;
        }

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueQueue.Dequeue();
        Debug.Log("DialogueManager: Displaying line from: " + line.speakerName + " Content: " + line.textContent);
        if (speakerNameText != null)
        {
            if (string.IsNullOrEmpty(line.speakerName))
            {
                if (namePanelBox != null) namePanelBox.SetActive(false);
                speakerNameText.text = "";
            }
            else
            {
                if (namePanelBox != null) namePanelBox.SetActive(true);
                speakerNameText.text = line.speakerName;
            }
        }

        currentFullText = line.textContent;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextCoroutine(currentFullText));
    }

    private IEnumerator TypeTextCoroutine(string textToType)
    {
        isTyping = true;
        dialogueBodyText.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueBodyText.text += letter;

            if (typingAudioSource != null && typingAudioSource.clip != null)
            {
                typingAudioSource.PlayOneShot(typingAudioSource.clip, typingVolume);
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteCurrentLineInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueBodyText.text = currentFullText;
        isTyping = false;
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        isTyping = false;

        if (dialogueBoxParent != null)
        {
            dialogueBoxParent.SetActive(false);
        }

        onDialogueCompleteCallback?.Invoke();
        onDialogueCompleteCallback = null;
    }
}