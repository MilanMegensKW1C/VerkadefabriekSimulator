using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DoorPurchase : MonoBehaviour
{
    [Header("Identiteit")]
    public string doorId = "zaal1";

    [Header("Prijs")]
    public int price = 500;

    [Header("Worldspace UI")]
    public GameObject worldECanvas;
    public float interactDistance = 3f;

    [Header("Tape")]
    public GameObject tapeObject;

    [Header("Popup UI")]
    public GameObject purchasePopupPanel;
    public TMP_Text priceTMPText;

    [Header("Teleport")]
    public string targetSceneName;

    [Header("Geluiden")]
    public AudioClip purchaseSuccessSound;
    public AudioClip purchaseFailedSound;

    private bool isPopupOpen = false;
    private bool isPlayerNearby = false;
    private Transform player;
    private bool bought = false;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (MoneyManager.Instance != null)
            bought = MoneyManager.Instance.IsDoorPurchased(doorId);

        UpdateTape();

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);
    }

    void Update()
    {
        BillboardECanvas();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        isPlayerNearby = dist <= interactDistance;

        if (worldECanvas != null)
            worldECanvas.SetActive(isPlayerNearby && !isPopupOpen);

        if (isPlayerNearby && !bought && !isPopupOpen && Input.GetKeyDown(KeyCode.E))
            OpenPopup();

        if (isPlayerNearby && bought && Input.GetKeyDown(KeyCode.E))
            ProceedEnter();

        if (isPopupOpen)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                HandlePurchase();

            else if (Input.GetKeyDown(KeyCode.Backspace))
                ClosePopup();
        }
    }

    private void BillboardECanvas()
    {
        if (worldECanvas == null || mainCam == null) return;

        worldECanvas.transform.LookAt(
            worldECanvas.transform.position + mainCam.transform.rotation * Vector3.forward,
            mainCam.transform.rotation * Vector3.up
        );
    }

    private void OpenPopup()
    {
        isPopupOpen = true;

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(true);

        if (priceTMPText != null)
            priceTMPText.text = price.ToString();
    }

    private void ClosePopup()
    {
        isPopupOpen = false;

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);
    }

    private void HandlePurchase()
    {
        if (MoneyManager.Instance == null) return;

        bool success = MoneyManager.Instance.TryBuyDoor(doorId, price);

        if (success)
        {
            bought = true;

            if (purchaseSuccessSound != null)
                AudioSource.PlayClipAtPoint(purchaseSuccessSound, Camera.main.transform.position);

            UpdateTape();
            ClosePopup();
        }
        else
        {
            if (purchaseFailedSound != null)
                AudioSource.PlayClipAtPoint(purchaseFailedSound, Camera.main.transform.position);

            StartCoroutine(FlashPriceRed());
        }
    }

    IEnumerator FlashPriceRed()
    {
        if (priceTMPText == null) yield break;

        Color original = priceTMPText.color;

        priceTMPText.color = Color.red;
        yield return new WaitForSeconds(0.25f);
        priceTMPText.color = original;
    }

    private void UpdateTape()
    {
        if (tapeObject != null)
            tapeObject.SetActive(!bought);
    }

    private void ProceedEnter()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                // Sla NIET de wereldpositie op (die hoort bij de huidige scène).
                // Sla wél de deur-id en gewenste rotatie (yaw +180) op — die gebruiken we later in de LOBBY-scene.
                SpawnManager.returnDoorId = doorId;

                Vector3 rot = p.transform.eulerAngles;
                rot.y += 180f; // zodanig dat bij terugkomst je van de deur af kijkt
                // bewaar pitch (x) en z indien gewenst:
                SpawnManager.returnEuler = new Vector3(rot.x, rot.y, rot.z);

                SpawnManager.hasReturnSpawn = true;
            }

            if (SceneFader.Instance != null)
                SceneFader.Instance.FadeToScene(targetSceneName);
            else
                SceneManager.LoadScene(targetSceneName);
        }
    }
}
