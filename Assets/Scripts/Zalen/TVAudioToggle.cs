using UnityEngine;

public class TVAudioToggleTrigger : MonoBehaviour
{
    public AudioSource tvAudio;
    private bool playerInRange = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (tvAudio != null)
                tvAudio.mute = !tvAudio.mute;
        }
    }
}
