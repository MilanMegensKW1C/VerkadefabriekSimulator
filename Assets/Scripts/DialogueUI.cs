/*
 * DialogueUI.cs
 * Auteur: Milan Megens
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    // Singleton instance
    public static DialogueUI Instance;

    // Houdt bij of player controls vergrendeld zijn
    private bool controlsLocked = false;

    [Header("UI")]
    // Hoofdpanel van de dialoog
    public GameObject dialoguePanel;

    // NPC portret
    public Image portraitImage;

    // Naam van de spreker
    public TextMeshProUGUI nameText;

    // Dialoogtekst
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter")]
    // Tijd tussen letters
    public float typeSpeed = 0.03f;

    // Optioneel typegeluid
    public AudioClip typeSound;

    // AudioSource voor typegeluid (2D)
    private AudioSource audioSource;

    [Header("Player Control Scripts")]
    // Script dat player movement regelt
    public MonoBehaviour movementScript;

    // Script dat camera/look regelt
    public MonoBehaviour lookScript;

    // Dialoogregels
    private List<string> lines;

    // Huidige regel index
    private int index;

    // Of de typewriter actief is
    private bool isTyping = false;

    // Referentie naar de actieve coroutine
    private Coroutine typeRoutine;

    void Awake()
    {
        // Singleton instellen
        Instance = this;

        // UI standaard verbergen
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // AudioSource instellen voor typegeluid
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        // Geen input verwerken als dialoog niet actief is
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        // Enter om door te klikken
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Tijdens typen: meteen afmaken
            if (isTyping)
                FinishTyping();
            else
                NextLine();
        }
    }

    // ----------------------------------------------------------
    // Start dialogue
    // ----------------------------------------------------------

    public void StartDialogue(string speakerName, Sprite portrait, List<string> dialogueLines)
    {
        // Geen geldige dialoog
        if (dialogueLines == null || dialogueLines.Count == 0)
            return;

        // Player controls altijd locken tijdens dialoog
        LockControls(true);

        // UI invullen
        nameText.text = speakerName;
        portraitImage.sprite = portrait;

        lines = dialogueLines;
        index = 0;

        dialoguePanel.SetActive(true);

        // Start typewriter voor eerste regel
        StartTypewriter(lines[index]);
    }

    // ----------------------------------------------------------
    // Next line
    // ----------------------------------------------------------

    // Ga naar de volgende regel
    void NextLine()
    {
        index++;

        // Einde dialoog bereikt
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

    // Start typewriter coroutine
    void StartTypewriter(string fullText)
    {
        // Stop lopende coroutine
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        typeRoutine = StartCoroutine(TypeText(fullText));
    }

    // Coroutine die letter voor letter typt
    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            // Geluid per letter (optioneel)
            if (typeSound != null)
                audioSource.PlayOneShot(typeSound);

            // Wacht tot volgende letter
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    // Maak het typen direct af
    void FinishTyping()
    {
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        dialogueText.text = lines[index];
        isTyping = false;
    }

    // ----------------------------------------------------------
    // End dialogue
    // ----------------------------------------------------------

    // Event dat afgaat na sluiten van dialoog
    public System.Action OnDialogueFinished;

    // Sluit de dialoog
    void CloseDialogue()
    {
        dialoguePanel.SetActive(false);

        // Player controls weer vrijgeven
        LockControls(false);

        // Event triggeren
        OnDialogueFinished?.Invoke();
    }

    // ----------------------------------------------------------
    // Player control lock
    // ----------------------------------------------------------

    // Zet player movement en look aan of uit
    void LockControls(bool state)
    {
        controlsLocked = state;

        // Movement script aan/uit
        if (movementScript != null)
        {
            movementScript.enabled = !state;

            // Rigidbody resetten zodat speler niet blijft glijden
            Rigidbody rb = movementScript.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;

            // CharacterController reset
            UnityEngine.CharacterController cc = movementScript.GetComponent<UnityEngine.CharacterController>();
            if (cc != null) cc.Move(Vector3.zero);
        }

        // Look script aan/uit
        if (lookScript != null)
        {
            lookScript.enabled = !state;
        }
    }

    // force lock
    public void ForceLockControls()
    {
        LockControls(true);
    }

    void LateUpdate()
    {
        if (controlsLocked)
        {
            // Camera rotatie bevroren houden
            if (lookScript != null)
            {
                Transform cam = lookScript.transform;
                cam.rotation = cam.rotation;
            }
        }
    }
}
