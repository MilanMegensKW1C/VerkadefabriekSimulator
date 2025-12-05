using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightSystem : MonoBehaviour
{
    private Color originalAmbientLight;

    [Header("Blackout Timing")]
    public float minTime = 900f;   // 15 min
    public float maxTime = 1080f;  // 18 min
    public float flickerDuration = 2f;

    [Header("Lamps (tag: Lamp)")]
    private Light[] lamps;

    [Header("Light Meshes (tag: LightON)")]
    private GameObject[] lightOnObjects;

    [Header("Control Panel")]
    public Collider controlPanelTrigger;
    public KeyCode interactKey = KeyCode.E;

    [Header("Interact UI")]
    public GameObject interactCanvas;
    public float interactShowDistance = 3f;
    public bool billboardUI = true;

    [Header("Flashlight")]
    public Light playerFlashlight;

    [Header("Ambient")]
    public Color ambientNormal = new Color(0.35f, 0.35f, 0.35f);
    public float reflectNormal = 0.5f;

    public Color ambientBlackout = new Color(0.03f, 0.03f, 0.03f);
    public float reflectBlackout = 0f;

    [Header("Sounds")]
    public AudioClip blackoutSound;
    public AudioClip restoreSound;
    public AudioClip zapSound;
    [Range(0f, 1f)] public float zapVolume = 1f;

    private AudioSource audioSource;

    bool blackoutActive = false;
    Transform player;

    // -------------------------------
    // FIRST-TIME SPAWN TUTORIAL
    // -------------------------------
    [Header("Eerste blackout tutorial")]
    public bool tutorialEnabled = true;
    private bool firstBlackoutTriggered = false;

    [Tooltip("UI systeem")]
    public DialogueUI dialogueUI;

    public string speakerName = "Monteur";
    public Sprite speakerPortrait;

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
        originalAmbientLight = RenderSettings.ambientLight;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject[] lampObjs = GameObject.FindGameObjectsWithTag("Lamp");

        List<Light> lampList = new List<Light>();
        foreach (var obj in lampObjs)
        {
            Light l = obj.GetComponent<Light>();
            if (l != null)
                lampList.Add(l);
        }

        lamps = lampList.ToArray();


        lightOnObjects = GameObject.FindGameObjectsWithTag("LightON");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;  // 2D sound

        if (interactCanvas != null) interactCanvas.SetActive(false);

        if (playerFlashlight != null)
            playerFlashlight.enabled = false;

        StartCoroutine(BlackoutRoutine());
    }

    void Update()
    {
        HandleInteractCanvas();

        if (blackoutActive && PlayerInsideControlPanel())
        {
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

        float dist = Vector3.Distance(player.position, controlPanelTrigger.transform.position);

        if (blackoutActive && dist <= interactShowDistance)
            interactCanvas.SetActive(true);
        else
            interactCanvas.SetActive(false);

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

        yield return StartCoroutine(Flicker());
        StartBlackout();
    }

    IEnumerator Flicker()
    {
        float timer = flickerDuration;

        while (timer > 0)
        {
            bool state = Random.value > 0.5f;

            foreach (var l in lamps)
                l.enabled = state;

            foreach (var o in lightOnObjects)
                o.SetActive(state);

            if (zapSound != null)
                AudioSource.PlayClipAtPoint(zapSound, player.position, zapVolume);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            timer -= 0.1f;
        }
    }

    void StartBlackout()
    {
        blackoutActive = true;

        foreach (var l in lamps) l.enabled = false;
        foreach (var o in lightOnObjects) o.SetActive(false);

        RenderSettings.ambientLight = ambientBlackout;
        RenderSettings.reflectionIntensity = reflectBlackout;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ForceLockControls();
        }

        if (playerFlashlight) playerFlashlight.enabled = true;

        if (blackoutSound != null)
            audioSource.PlayOneShot(blackoutSound);

        if (!firstBlackoutTriggered && tutorialEnabled)
        {
            firstBlackoutTriggered = true;
            if (dialogueUI != null)
                dialogueUI.StartDialogue(speakerName, speakerPortrait, new List<string>(tutorialLines));
        }
    }

    void RestoreLights()
    {
        RenderSettings.ambientLight = originalAmbientLight;

        blackoutActive = false;

        foreach (var l in lamps) l.enabled = true;
        foreach (var o in lightOnObjects) o.SetActive(true);

        if (playerFlashlight) playerFlashlight.enabled = false;

        if (restoreSound)
            audioSource.PlayOneShot(restoreSound);

        StartCoroutine(BlackoutRoutine());
    }
}
