/*
 * LightSystem.cs
 * Auteur: Milan Megens
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSystem : MonoBehaviour
{
    // Oorspronkelijke ambient light waarde om later te herstellen
    private Color originalAmbientLight;

    [Header("Blackout Timing")]
    // Minimale tijd tot een blackout (in seconden)
    public float minTime = 900f;  
    // Maximale tijd tot een blackout (in seconden)
    public float maxTime = 1080f;
    // Hoe lang de lampen flikkeren vóór de blackout
    public float flickerDuration = 2f;

    [Header("Lamps (tag: Lamp)")]
    // Alle echte Light-components in de scene
    private Light[] lamps;

    [Header("Light Meshes (tag: LightON)")]
    // Meshes of objecten die licht uitstralen (visueel)
    private GameObject[] lightOnObjects;

    [Header("Control Panel")]
    // Trigger collider van het controlepaneel
    public Collider controlPanelTrigger;
    // Toets voor interact
    public KeyCode interactKey = KeyCode.E;

    [Header("Interact UI")]
    // Worldspace UI dat toont dat je kunt interacteren
    public GameObject interactCanvas;
    // Afstand waarop de UI zichtbaar wordt
    public float interactShowDistance = 3f;
    // Of de UI altijd naar de speler draait
    public bool billboardUI = true;

    [Header("Flashlight")]
    // Zaklamp van de speler die bij blackout aan gaat
    public Light playerFlashlight;

    [Header("Ambient")]
    // Normale ambient light kleur
    public Color ambientNormal = new Color(0.35f, 0.35f, 0.35f);
    // Reflectie-intensiteit bij normaal licht
    public float reflectNormal = 0.5f;

    // Ambient light kleur tijdens blackout
    public Color ambientBlackout = new Color(0.03f, 0.03f, 0.03f);
    // Reflectie-intensiteit tijdens blackout
    public float reflectBlackout = 0f;

    [Header("Sounds")]
    // Geluid bij het uitvallen van de stroom
    public AudioClip blackoutSound;
    // Geluid bij het herstellen van de stroom
    public AudioClip restoreSound;
    // Zap / vonk geluid tijdens flicker
    public AudioClip zapSound;
    // Volume van het zap-geluid
    [Range(0f, 1f)] public float zapVolume = 1f;

    // AudioSource voor 2D geluiden
    private AudioSource audioSource;

    // Houdt bij of de blackout actief is
    bool blackoutActive = false;
    // Referentie naar de speler
    Transform player;

    // -------------------------------
    // FIRST-TIME SPAWN TUTORIAL
    // -------------------------------

    [Header("Eerste blackout tutorial")]
    // Of de tutorial actief is
    public bool tutorialEnabled = true;
    // Zorgt ervoor dat de tutorial maar één keer afgaat
    private bool firstBlackoutTriggered = false;

    [Tooltip("UI systeem")]
    // Referentie naar het dialoog UI systeem
    public DialogueUI dialogueUI;

    // Naam van de spreker in de tutorial
    public string speakerName = "Monteur";
    // Portret van de spreker
    public Sprite speakerPortrait;

    // Zinnen die tijdens de tutorial worden getoond
    [TextArea(3, 7)]
    public string[] tutorialLines =
    {
        "Hé! De lampen zijn uitgevallen...",
        "Dit gebouw is oud, dus dat gebeurt soms.",
        "Je moet naar de controlekamer. Zoek het zekeringkastje op.",
        "Gebruik E om de stroom weer aan te zetten.",
        "Wees snel, anders blijft het donker!"
    };

    void Start()
    {
        // Sla de originele ambient light op
        originalAmbientLight = RenderSettings.ambientLight;

        // Zoek de speler
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Zoek alle lamp-objecten in de scene
        GameObject[] lampObjs = GameObject.FindGameObjectsWithTag("Lamp");

        // Verzamel alle Light-components
        List<Light> lampList = new List<Light>();
        foreach (var obj in lampObjs)
        {
            Light l = obj.GetComponent<Light>();
            if (l != null)
                lampList.Add(l);
        }

        lamps = lampList.ToArray();

        // Zoek alle licht-meshes
        lightOnObjects = GameObject.FindGameObjectsWithTag("LightON");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        // Zet interact UI uit bij start
        if (interactCanvas != null)
            interactCanvas.SetActive(false);

        // Zet zaklamp standaard uit
        if (playerFlashlight != null)
            playerFlashlight.enabled = false;

        // Start de blackout timer
        StartCoroutine(BlackoutRoutine());
    }

    void Update()
    {
        // Update de interact UI
        HandleInteractCanvas();

        // Als blackout actief is en speler bij paneel staat
        if (blackoutActive && PlayerInsideControlPanel())
        {
            // Herstel stroom bij indrukken van de interact toets
            if (Input.GetKeyDown(interactKey))
                RestoreLights();
        }
    }

    // ----------------------------------------------------------
    //  E-CANVAS
    // ----------------------------------------------------------

    void HandleInteractCanvas()
    {
        if (interactCanvas == null) return;

        // Afstand tussen speler en controlepaneel
        float dist = Vector3.Distance(player.position, controlPanelTrigger.transform.position);

        // Toon UI alleen tijdens blackout en binnen afstand
        if (blackoutActive && dist <= interactShowDistance)
            interactCanvas.SetActive(true);
        else
            interactCanvas.SetActive(false);

        // Laat de UI altijd naar de speler kijken
        if (billboardUI)
        {
            interactCanvas.transform.LookAt(player);
            interactCanvas.transform.Rotate(0, 180, 0);
        }
    }

    bool PlayerInsideControlPanel()
    {
        Collider[] hits = Physics.OverlapBox(
            controlPanelTrigger.bounds.center,
            controlPanelTrigger.bounds.extents
        );

        foreach (var h in hits)
            if (h.CompareTag("Player"))
                return true;

        return false;
    }

    // ----------------------------------------------------------
    //  MAIN LOGIC
    // ----------------------------------------------------------

    IEnumerator BlackoutRoutine()
    {
        float t = Random.Range(minTime, maxTime);
        yield return new WaitForSeconds(t);

        // Eerst flikkeren, daarna echte blackout
        yield return StartCoroutine(Flicker());
        StartBlackout();
    }

    IEnumerator Flicker()
    {
        float timer = flickerDuration;

        while (timer > 0)
        {
            bool state = Random.value > 0.5f;

            // Zet alle lampen aan of uit
            foreach (var l in lamps)
                l.enabled = state;

            foreach (var o in lightOnObjects)
                o.SetActive(state);

            // Speel zap-geluid
            if (zapSound != null)
                AudioSource.PlayClipAtPoint(zapSound, player.position, zapVolume);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            timer -= 0.1f;
        }
    }

    void StartBlackout()
    {
        blackoutActive = true;

        // Zet alle lampen uit
        foreach (var l in lamps) l.enabled = false;
        foreach (var o in lightOnObjects) o.SetActive(false);

        // Pas ambient light aan
        RenderSettings.ambientLight = ambientBlackout;
        RenderSettings.reflectionIntensity = reflectBlackout;

        // Vergrendel spelerbesturing via DialogueUI
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ForceLockControls();
        }

        // Zet zaklamp aan
        if (playerFlashlight) playerFlashlight.enabled = true;

        // Speel blackout geluid
        if (blackoutSound != null)
            audioSource.PlayOneShot(blackoutSound);

        // Start tutorial bij eerste blackout
        if (!firstBlackoutTriggered && tutorialEnabled)
        {
            firstBlackoutTriggered = true;
            if (dialogueUI != null)
                dialogueUI.StartDialogue(
                    speakerName,
                    speakerPortrait,
                    new List<string>(tutorialLines)
                );
        }
    }

    void RestoreLights()
    {
        // Zet ambient light terug
        RenderSettings.ambientLight = originalAmbientLight;

        blackoutActive = false;

        // Zet alle lampen weer aan
        foreach (var l in lamps) l.enabled = true;
        foreach (var o in lightOnObjects) o.SetActive(true);

        // Zet zaklamp uit
        if (playerFlashlight) playerFlashlight.enabled = false;

        // Speel herstelgeluid
        if (restoreSound)
            audioSource.PlayOneShot(restoreSound);

        // Start opnieuw de blackout timer
        StartCoroutine(BlackoutRoutine());
    }
}
