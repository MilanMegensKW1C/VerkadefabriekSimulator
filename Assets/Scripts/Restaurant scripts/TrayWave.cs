using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class TrayWave : MonoBehaviour
{
    [Header("Trigger instellingen")]
    public string playerTag = "Player";
    private bool playerInRange = false;

    [Header("Animatie instellingen")]
    public float jumpHeight = 0.3f;        // hoe hoog de trays springen
    public float jumpDuration = 0.5f;      // hoelang één sprong duurt
    public float waveDelay = 0.08f;        // vertraging tussen trays
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Transform[] trays;
    private Vector3[] startPositions;
    private bool isAnimating = false;

    void Start()
    {
        int count = transform.childCount;
        trays = new Transform[count];
        startPositions = new Vector3[count];

        // Alle trays verzamelen
        for (int i = 0; i < count; i++)
        {
            trays[i] = transform.GetChild(i);
            startPositions[i] = trays[i].localPosition;
        }

        // ✅ Reverse de volgorde → bovenste tray eerst
        System.Array.Reverse(trays);
        System.Array.Reverse(startPositions);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isAnimating)
                StartCoroutine(WaveAnimation());
        }
    }

    private IEnumerator WaveAnimation()
    {
        isAnimating = true;

        // start animatie voor elk tray met delay
        for (int i = 0; i < trays.Length; i++)
        {
            StartCoroutine(Jump(trays[i], startPositions[i]));
            yield return new WaitForSeconds(waveDelay);
        }

        // wacht tot alles klaar is
        yield return new WaitForSeconds(jumpDuration + trays.Length * waveDelay);

        // reset
        for (int i = 0; i < trays.Length; i++)
        {
            trays[i].localPosition = startPositions[i];
        }

        isAnimating = false;
    }

    private IEnumerator Jump(Transform tray, Vector3 startPos)
    {
        float t = 0f;
        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float height = jumpCurve.Evaluate(t / jumpDuration) * jumpHeight;
            tray.localPosition = startPos + Vector3.up * height;
            yield return null;
        }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
    }
}
