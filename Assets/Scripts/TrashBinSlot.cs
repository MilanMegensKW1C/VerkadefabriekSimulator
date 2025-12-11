using UnityEngine;
using TMPro;

public class TrashBinSlot : MonoBehaviour
{
    [Header("Identity")]
    public string binId = "trash_1";

    [Header("Purchase")]
    public int buyPrice = 150;

    [Header("Visuals")]
    public GameObject model;
    public Material ghostMaterial;
    private Material[] originalMats;

    [Header("UI")]
    public GameObject purchasePopupPanel;   // alleen popup boven de bak
    public TMP_Text priceText;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public float cleanRadius = 4f;

    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip cleanSound;
    public AudioClip purchaseFailSound;   // NIEUW
    public AudioClip purchaseSuccessSound; // NIEUW

    private bool purchased = false;
    private Transform player;
    private AudioSource audioSource;
    private Camera mainCam;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCam = Camera.main;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        // originele materialen cachen
        if (model != null)
        {
            Renderer[] rends = model.GetComponentsInChildren<Renderer>();
            originalMats = new Material[rends.Length];
            for (int i = 0; i < rends.Length; i++)
                originalMats[i] = rends[i].sharedMaterial;
        }

        if (purchasePopupPanel != null) purchasePopupPanel.SetActive(false);
        if (priceText != null) priceText.text = buyPrice.ToString();

        // staat herstellen
        if (MoneyManager.Instance != null && MoneyManager.Instance.IsItemPurchased(binId))
            SetPurchasedState(true);
        else
            SetPurchasedState(false);
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        // billboard popup each frame so it points to camera
        if (purchasePopupPanel != null && purchasePopupPanel.activeSelf && mainCam != null)
        {
            purchasePopupPanel.transform.LookAt(
                purchasePopupPanel.transform.position + mainCam.transform.rotation * Vector3.forward,
                mainCam.transform.rotation * Vector3.up
            );
        }

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactDistance;

        // Popup zichtbaar als nog NIET gekocht + speler dichtbij
        if (!purchased)
        {
            purchasePopupPanel?.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!purchased)
                TryPurchase();
            else
                CleanDebris();
        }
    }

    void TryPurchase()
    {
        if (MoneyManager.Instance == null) return;

        bool bought = MoneyManager.Instance.TryBuyItem(binId, buyPrice);

        if (bought)
        {
            // ✔️ aankoop gelukt
            SetPurchasedState(true);

            if (purchaseSuccessSound != null)
                audioSource.PlayOneShot(purchaseSuccessSound);
            else if (purchaseSound != null)
                audioSource.PlayOneShot(purchaseSound);
        }
        else
        {
            // ❌ te weinig geld
            if (purchaseFailSound != null)
                audioSource.PlayOneShot(purchaseFailSound);
        }
    }

    void SetPurchasedState(bool state)
    {
        purchased = state;

        Renderer[] rends = model.GetComponentsInChildren<Renderer>();

        if (state)
        {
            // echte materialen terug
            for (int i = 0; i < rends.Length && i < originalMats.Length; i++)
                rends[i].material = originalMats[i];

            purchasePopupPanel?.SetActive(false); // popup weg
        }
        else
        {
            // ghost look voor niet gekocht
            for (int i = 0; i < rends.Length; i++)
                rends[i].material = ghostMaterial;

            purchasePopupPanel?.SetActive(true);
        }
    }

    void CleanDebris()
    {
        // vind DebrisPoint binnen radius en hide each (player cleans single ones via DebrisPoint E as well)
        Collider[] hits = Physics.OverlapSphere(transform.position, cleanRadius);

        int found = 0;
        foreach (var c in hits)
        {
            if (c.TryGetComponent<DebrisPoint>(out DebrisPoint dp))
            {
                dp.HideDebris();
                found++;
            }
        }

        if (found > 0 && cleanSound != null)
            audioSource.PlayOneShot(cleanSound);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);
    }
}
