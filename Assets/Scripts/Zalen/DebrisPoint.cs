using UnityEngine;
using TMPro;

public class DebrisPoint : MonoBehaviour
{
    [Header("Optional UI")]
    public GameObject ePrompt;           // worldspace canvas with "E" (optional)
    public float cleanDistance = 2.0f;   // fallback if no trigger

    [Header("Audio")]
    public AudioClip pickSound;

    bool playerNearby = false;
    Transform player;
    AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        // UI off at start
        if (ePrompt != null) ePrompt.SetActive(false);

        // ensure object starts hidden if you want manager to control it
        // (DebrisManager will manage active state).
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            SetNearby(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            SetNearby(false);
    }

    void Update()
    {
        // fallback: if no trigger used, use distance check
        if (!playerNearby && player != null && GetComponent<Collider>() == null)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d <= cleanDistance) SetNearby(true);
        }

        // handle input
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            CleanThis();
        }
    }

    void SetNearby(bool on)
    {
        playerNearby = on;
        if (ePrompt != null) ePrompt.SetActive(on);
    }

    public void CleanThis()
    {
        // play sound
        if (pickSound != null && audioSource != null)
            audioSource.PlayOneShot(pickSound);

        // Hide / destroy
        gameObject.SetActive(false);

        // notify DebrisManager (optional) — DebrisManager will see it inactive
    }

    // helper so other scripts can force-show/hide
    public void ShowDebris() => gameObject.SetActive(true);
    public void HideDebris() => gameObject.SetActive(false);
}
