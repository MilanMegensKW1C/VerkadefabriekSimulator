using UnityEngine;

[DisallowMultipleComponent]
public class SlidingDoor : MonoBehaviour
{
    [Header("Target (optioneel: laat leeg en gebruik Player Tag)")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Interactie")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Sliding motion")]
    public Vector3 slideDirection = Vector3.right;
    public float slideDistance = 0.5f;
    public float slideSpeed = 3f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private bool isOpen = false;
    private bool playerInRange = false;

    void Start()
    {
        closedPos = transform.localPosition;
        openPos = closedPos + slideDirection.normalized * slideDistance;

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (isOpen) CloseDoor();
            else OpenDoor();
        }

        Vector3 desiredPos = isOpen ? openPos : closedPos;
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, Time.deltaTime * slideSpeed);
    }

    public void OpenDoor() => isOpen = true;
    public void CloseDoor() => isOpen = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }
}
