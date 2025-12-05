using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;
    private bool controlsLocked = false;

    [Header("UI")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter")]
    public float typeSpeed = 0.03f;
    public AudioClip typeSound;      // optioneel tik-geluidje
    private AudioSource audioSource; // 2D

    [Header("Player Control Scripts")]
    public MonoBehaviour movementScript;
    public MonoBehaviour lookScript;

    private List<string> lines;
    private int index;
    private bool isTyping = false;
    private Coroutine typeRoutine;

    void Awake()
    {
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
                FinishTyping();
            else
                NextLine();
        }
    }

    // ----------------------------------------------------------
    // Start Dialogue
    // ----------------------------------------------------------
    public void StartDialogue(string speakerName, Sprite portrait, List<string> dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
            return;

        // <-- HIER: altijd controls locken
        LockControls(true);

        nameText.text = speakerName;
        portraitImage.sprite = portrait;

        lines = dialogueLines;
        index = 0;

        dialoguePanel.SetActive(true);

        StartTypewriter(lines[index]);
    }


    // ----------------------------------------------------------
    // Next line
    // ----------------------------------------------------------
    void NextLine()
    {
        index++;

        if (index >= lines.Count)
        {
            CloseDialogue();
            return;
        }

        StartTypewriter(lines[index]);
    }

    // ----------------------------------------------------------
    // Typewriter effect
    // ----------------------------------------------------------
    void StartTypewriter(string fullText)
    {
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        typeRoutine = StartCoroutine(TypeText(fullText));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            if (typeSound != null)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void FinishTyping()
    {
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        dialogueText.text = lines[index];
        isTyping = false;
    }

    // ----------------------------------------------------------
    // End Dialogue
    // ----------------------------------------------------------
    public System.Action OnDialogueFinished;

    void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        LockControls(false);

        OnDialogueFinished?.Invoke();
    }


    // ----------------------------------------------------------
    // Player control lock
    // ----------------------------------------------------------
    void LockControls(bool state)
    {
        controlsLocked = state;

        if (movementScript != null)
        {
            movementScript.enabled = !state;

            // Rigidbody resetten zodat speler niet glijdt
            Rigidbody rb = movementScript.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;

            // CharacterController reset
            UnityEngine.CharacterController cc = movementScript.GetComponent<UnityEngine.CharacterController>();
            if (cc != null) cc.Move(Vector3.zero);
        }

        if (lookScript != null)
        {
            lookScript.enabled = !state;
        }
    }

    public void ForceLockControls()
    {
        LockControls(true);
    }

    void LateUpdate()
    {
        if (controlsLocked)
        {
            // Camera op huidige rotatie bevriezen
            if (lookScript != null)
            {
                Transform cam = lookScript.transform;
                cam.rotation = cam.rotation; // ‘bevries’ huidige rotatie
            }
        }
    }
}
