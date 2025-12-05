using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyDoor : MonoBehaviour
{
    [Header("Door identity (must match DoorPurchase.doorId)")]
    public string doorId = "zaal1";

    [Header("Return spawn (assign in inspector)")]
    public Transform returnSpawn;

    [Header("Interact Settings")]
    public float interactDistance = 3f;
    public string lobbySceneName = "Lobby";
    public Canvas worldspaceCanvas;

    private Transform cameraTransform;
    private bool isInRange = false;

    void Start()
    {
        if (worldspaceCanvas != null)
            worldspaceCanvas.enabled = false;
    }

    void Update()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                return;
        }

        float dist = Vector3.Distance(transform.position, cameraTransform.position);
        isInRange = dist <= interactDistance;

        if (worldspaceCanvas != null)
            worldspaceCanvas.enabled = isInRange;

        if (worldspaceCanvas != null)
        {
            worldspaceCanvas.transform.LookAt(
                worldspaceCanvas.transform.position + cameraTransform.forward
            );
        }

        if (isInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (SceneFader.Instance == null)
            {
                Debug.LogError("Geen SceneFader gevonden in de scene!");
                return;
            }

            // ----------------------------------------------------
            //  FIX → sla op via welke deur je terugging naar lobby
            // ----------------------------------------------------
            SpawnManager.returnDoorId = doorId;
            SpawnManager.returnEuler = transform.eulerAngles;
            SpawnManager.hasReturnSpawn = true;

            SceneFader.Instance.FadeToScene(lobbySceneName);
        }
    }
}
