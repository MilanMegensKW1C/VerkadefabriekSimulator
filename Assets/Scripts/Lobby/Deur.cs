/*
 * Deur.cs
 * Auteur: Milan Megens
 */

using UnityEngine;

[DisallowMultipleComponent]
public class Deur : MonoBehaviour
{
    [Header("Instellingen")]
    // Tag waarmee de speler wordt herkend
    public string playerTag = "Player";

    // Hoek waarnaar de deur opent
    public float openAngle = 90f;

    // Snelheid van openen en sluiten
    public float openSpeed = 2f;

    // Keert de draairichting van de deur om
    public bool invertAngleDirection = false;

    [Header("Audio (optioneel)")]
    // Geluid bij openen
    public AudioClip openSound;

    // Geluid bij sluiten
    public AudioClip closeSound;

    // Volume voor deur geluiden
    [Range(0f, 1f)] public float volume = 0.5f;

    // Oorspronkelijke rotatie (gesloten)
    private Quaternion closedRot;

    // Rotatie wanneer deur open staat
    private Quaternion openRot;

    // Houdt bij of de deur open is
    private bool isOpen = false;

    // Of de speler binnen de trigger staat
    private bool playerInRange = false;

    // AudioSource voor deur geluiden
    private AudioSource audioSource;

    void Start()
    {
        // Sla gesloten rotatie op
        closedRot = transform.localRotation;

        // Bepaal openingshoek
        float angle = invertAngleDirection ? -openAngle : openAngle;

        // Bereken open rotatie
        openRot = closedRot * Quaternion.Euler(0f, angle, 0f);

        // Alleen AudioSource toevoegen als er geluiden zijn ingesteld
        if (openSound != null || closeSound != null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    void Update()
    {
        // Bepaal doelrotatie op basis van open of gesloten status
        Quaternion targetRot = isOpen ? openRot : closedRot;

        // Smooth interpolatie tussen huidige en doelrotatie
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * openSpeed
        );
    }

    void OnTriggerEnter(Collider other)
    {
        // Check of speler de trigger binnenkomt
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check of speler de trigger verlaat
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            CloseDoor();
        }
    }

    // Zet de deur open
    void OpenDoor()
    {
        // Niet opnieuw openen als hij al open is
        if (isOpen) return;

        isOpen = true;

        // Speel open geluid
        if (openSound != null && audioSource != null)
            audioSource.PlayOneShot(openSound, volume);
    }

    // Zet de deur dicht
    void CloseDoor()
    {
        // Niet sluiten als hij al dicht is
        if (!isOpen) return;

        isOpen = false;

        // Speel sluit geluid
        if (closeSound != null && audioSource != null)
            audioSource.PlayOneShot(closeSound, volume);
    }
}
