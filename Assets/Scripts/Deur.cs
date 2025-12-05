using UnityEngine;

[DisallowMultipleComponent]
public class Deur : MonoBehaviour
{
    [Header("Instellingen")]
    public string playerTag = "Player";
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool invertAngleDirection = false;

    [Header("Audio (optioneel)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isOpen = false;
    private bool playerInRange = false;
    private AudioSource audioSource;

    void Start()
    {
        closedRot = transform.localRotation;

        float angle = invertAngleDirection ? -openAngle : openAngle;
        openRot = closedRot * Quaternion.Euler(0f, angle, 0f);

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
        // bepaal de doelrotatie (smooth bewegen)
        Quaternion targetRot = isOpen ? openRot : closedRot;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * openSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (openSound != null && audioSource != null)
            audioSource.PlayOneShot(openSound, volume);
    }

    void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (closeSound != null && audioSource != null)
            audioSource.PlayOneShot(closeSound, volume);
    }
}
