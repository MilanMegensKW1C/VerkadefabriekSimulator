using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class RekInteract : MonoBehaviour
{
    [Header("Target (optioneel: laat leeg en gebruik Player Tag)")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Animatie instellingen")]
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.6f;
    public AnimationCurve jumpCurve;

    private Transform[] objectsToAnimate;
    private Vector3[] originalPositions;
    private bool isAnimating = false;
    private bool playerInRange = false;

    void Start()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }

        int childCount = transform.childCount;
        objectsToAnimate = new Transform[childCount];
        originalPositions = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            objectsToAnimate[i] = transform.GetChild(i);
            originalPositions[i] = objectsToAnimate[i].localPosition;
        }

        if (jumpCurve == null)
            jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isAnimating)
                StartCoroutine(JumpAll());
        }
    }

    private IEnumerator JumpAll()
    {
        isAnimating = true;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;
            float height = jumpCurve.Evaluate(t) * jumpHeight;

            for (int i = 0; i < objectsToAnimate.Length; i++)
            {
                Vector3 start = originalPositions[i];
                objectsToAnimate[i].localPosition = start + Vector3.up * height;
            }

            yield return null;
        }

        for (int i = 0; i < objectsToAnimate.Length; i++)
        {
            objectsToAnimate[i].localPosition = originalPositions[i];
        }

        isAnimating = false;
    }

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
