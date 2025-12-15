/*
 * SceneFader.cs
 * Auteur: Milan Megens
 */

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DoorPurchase : MonoBehaviour
{
    [Header("Identiteit")]
    // Unieke ID van deze deur
    public string doorId = "zaal1";

    [Header("Prijs")]
    // Kosten om de deur te ontgrendelen
    public int price = 500;

    [Header("Worldspace UI")]
    // Canvas met "E" prompt
    public GameObject worldECanvas;

    // Afstand waarop de speler kan interacteren
    public float interactDistance = 3f;

    [Header("Tape")]
    // Object dat laat zien dat de deur nog gesloten is
    public GameObject tapeObject;

    [Header("Popup UI")]
    // Popup panel voor bevestigen van aankoop
    public GameObject purchasePopupPanel;

    // TMP text waarin de prijs getoond wordt
    public TMP_Text priceTMPText;

    [Header("Teleport")]
    // Scene waar de speler naartoe gaat
    public string targetSceneName;

    [Header("Geluiden")]
    // Geluid bij succesvolle aankoop
    public AudioClip purchaseSuccessSound;

    // Geluid bij mislukte aankoop
    public AudioClip purchaseFailedSound;

    // Of de aankoop popup open is
    private bool isPopupOpen = false;

    // Of de speler dichtbij genoeg is
    private bool isPlayerNearby = false;

    // Referentie naar speler
    private Transform player;

    // Of deze deur al gekocht is
    private bool bought = false;

    // Referentie naar de main camera (voor billboard UI)
    private Camera mainCam;

    void Start()
    {
        // Camera cachen
        mainCam = Camera.main;

        // Check of deur al gekocht is
        if (MoneyManager.Instance != null)
            bought = MoneyManager.Instance.IsDoorPurchased(doorId);

        // Tape status updaten
        UpdateTape();

        // Popup standaard verbergen
        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);
    }

    void Update()
    {
        // Worldspace UI naar camera laten kijken
        BillboardECanvas();

        // Speler zoeken indien nog niet gevonden
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        // Afstand tot speler berekenen
        float dist = Vector3.Distance(player.position, transform.position);
        isPlayerNearby = dist <= interactDistance;

        // E-prompt alleen tonen als speler dichtbij is en popup niet open is
        if (worldECanvas != null)
            worldECanvas.SetActive(isPlayerNearby && !isPopupOpen);

        // E om deur te kopen
        if (isPlayerNearby && !bought && !isPopupOpen && Input.GetKeyDown(KeyCode.E))
            OpenPopup();

        // E om door te lopen als deur al gekocht is
        if (isPlayerNearby && bought && Input.GetKeyDown(KeyCode.E))
            ProceedEnter();

        // Input afhandelen zolang popup open is
        if (isPopupOpen)
        {
            // Enter = kopen
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                HandlePurchase();

            // Backspace = annuleren
            else if (Input.GetKeyDown(KeyCode.Backspace))
                ClosePopup();
        }
    }

    // Zorgt ervoor dat het worldspace canvas altijd naar de camera kijkt
    private void BillboardECanvas()
    {
        if (worldECanvas == null || mainCam == null) return;

        worldECanvas.transform.LookAt(
            worldECanvas.transform.position + mainCam.transform.rotation * Vector3.forward,
            mainCam.transform.rotation * Vector3.up
        );
    }

    // Opent de aankoop popup
    private void OpenPopup()
    {
        isPopupOpen = true;

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(true);

        // Prijs tonen
        if (priceTMPText != null)
            priceTMPText.text = price.ToString();
    }

    // Sluit de aankoop popup
    private void ClosePopup()
    {
        isPopupOpen = false;

        if (purchasePopupPanel != null)
            purchasePopupPanel.SetActive(false);
    }

    // Probeert de deur te kopen
    private void HandlePurchase()
    {
        if (MoneyManager.Instance == null) return;

        bool success = MoneyManager.Instance.TryBuyDoor(doorId, price);

        if (success)
        {
            // Deur is nu gekocht
            bought = true;

            // Succesgeluid
            if (purchaseSuccessSound != null)
                AudioSource.PlayClipAtPoint(purchaseSuccessSound, Camera.main.transform.position);

            // Tape verwijderen en popup sluiten
            UpdateTape();
            ClosePopup();
        }
        else
        {
            // Faalgeluid
            if (purchaseFailedSound != null)
                AudioSource.PlayClipAtPoint(purchaseFailedSound, Camera.main.transform.position);

            // Prijs kort rood laten knipperen
            StartCoroutine(FlashPriceRed());
        }
    }

    // Laat de prijs kort rood oplichten bij te weinig geld
    IEnumerator FlashPriceRed()
    {
        if (priceTMPText == null) yield break;

        Color original = priceTMPText.color;

        priceTMPText.color = Color.red;
        yield return new WaitForSeconds(0.25f);
        priceTMPText.color = original;
    }

    // Zet tape aan of uit afhankelijk van aankoopstatus
    private void UpdateTape()
    {
        if (tapeObject != null)
            tapeObject.SetActive(!bought);
    }

    // Betreed de zaal achter de deur
    private void ProceedEnter()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                // Bewaar deur ID en gewenste terugkeer rotatie
                SpawnManager.returnDoorId = doorId;

                Vector3 rot = p.transform.eulerAngles;
                rot.y += 180f; // Bij terugkomst van de deur af kijken

                SpawnManager.returnEuler = new Vector3(rot.x, rot.y, rot.z);
                SpawnManager.hasReturnSpawn = true;
            }

            // Scene laden met fade indien mogelijk
            if (SceneFader.Instance != null)
                SceneFader.Instance.FadeToScene(targetSceneName);
            else
                SceneManager.LoadScene(targetSceneName);
        }
    }
}
