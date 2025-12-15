/*
 * VendingMachine.cs
 * Auteur: Milan Megens
 */

using UnityEngine;
using TMPro;
using System.Collections;

public class VendingMachine : MonoBehaviour
{
    // Basis instellingen
    [Header("Instellingen")]
    public string playerTag = "Player";  
    public int maxLevel = 5;             

    // Kosten en inkomsten per level
    [Header("Level Costs / Income")]
    public int[] levelCosts = new int[6];    
    public int[] levelIncome = new int[6];    
    public float moneyInterval = 3f;         

    // UI: kopen
    [Header("UI - Buy Popup")]
    public GameObject buyPopup;               
    public TextMeshProUGUI buyPriceText;    
    public TextMeshProUGUI buyIncomeText;    

    // UI: upgraden
    [Header("UI - Upgrade Popup")]
    public GameObject upgradePopup;             
    public TextMeshProUGUI upgradePriceText;      
    public TextMeshProUGUI upgradeIncomeText;    
    public TextMeshProUGUI upgradePreviousIncomeText;

    // UI: countdown / geld indicatie
    [Header("UI - Geld Popup")]
    public TextMeshProUGUI countdownText;

    // Interactie UI
    [Header("Interact Prompt (E)")]
    public GameObject interactCanvas;   

    // Audio
    [Header("Audio")]
    public AudioSource audioSource;             
    public AudioClip buySound;                 
    public AudioClip errorSound;                
    public AudioClip incomeSound;              

    // Interne state
    private bool isPurchased = false;    
    private bool playerNear = false;    
    private int upgradeLevel = 0;     

    // Huidige inkomsten op basis van level
    private int CurrentIncome => levelIncome[upgradeLevel];

    void Start()
    {
        // Zorg dat er altijd een AudioSource aanwezig is
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        // Alle UI standaard uit
        buyPopup.SetActive(false);
        upgradePopup.SetActive(false);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (interactCanvas) interactCanvas.SetActive(false);
    }

    void Update()
    {
        // Zorgt dat de interact UI altijd naar de camera kijkt
        Billboard();

        // Interactie met E
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!isPurchased)
                ShowBuyPopup();       // Nog niet gekocht
            else
                ShowUpgradePopup();   // Al gekocht → upgraden
        }

        // Afhandeling van popups
        if (buyPopup.activeSelf || upgradePopup.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (buyPopup.activeSelf) ConfirmBuy();
                if (upgradePopup.activeSelf) ConfirmUpgrade();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
                HidePopups();
        }

        // Toon interact prompt alleen als speler dichtbij is
        if (interactCanvas)
            interactCanvas.SetActive(playerNear && !buyPopup.activeSelf && !upgradePopup.activeSelf);
    }

    // Billboard: laat UI naar camera kijken
    void Billboard()
    {
        if (interactCanvas && Camera.main)
        {
            interactCanvas.transform.LookAt(Camera.main.transform);
            interactCanvas.transform.Rotate(0, 180, 0);
        }
    }

    // BUY: eerste aankoop
    void ShowBuyPopup()
    {
        // Toon prijs en income van level 0
        buyPriceText.text = levelCosts[0].ToString();
        buyIncomeText.text = levelIncome[0].ToString();

        buyPopup.SetActive(true);
        upgradePopup.SetActive(false);
    }

    void ConfirmBuy()
    {
        // Check of speler genoeg geld heeft
        if (!MoneyManager.Instance.TryRemoveMoney(levelCosts[0]))
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        isPurchased = true;

        if (buySound) audioSource.PlayOneShot(buySound);

        // Registreer machine bij MoneyManager
        MoneyManager.Instance.RegisterVendingMachine(this, CurrentIncome);

        HidePopups();
    }

    // UPGRADE
    void ShowUpgradePopup()
    {
        bool maxed = upgradeLevel >= maxLevel;

        if (maxed)
        {
            // Max level bereikt
            upgradePriceText.text = "MAX";
            upgradeIncomeText.text = levelIncome[upgradeLevel].ToString();
            upgradePreviousIncomeText.gameObject.SetActive(false);
        }
        else
        {
            // Toon volgende upgrade info
            upgradePriceText.text = levelCosts[upgradeLevel + 1].ToString();
            upgradeIncomeText.text = levelIncome[upgradeLevel + 1].ToString();
            upgradePreviousIncomeText.text = "nu " + levelIncome[upgradeLevel];
            upgradePreviousIncomeText.gameObject.SetActive(true);
        }

        upgradePopup.SetActive(true);
        buyPopup.SetActive(false);
    }

    void ConfirmUpgrade()
    {
        // Beveiliging tegen upgraden boven max
        if (upgradeLevel >= maxLevel)
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        int cost = levelCosts[upgradeLevel + 1];

        // Check geld
        if (!MoneyManager.Instance.TryRemoveMoney(cost))
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        upgradeLevel++;

        // Update income in MoneyManager
        MoneyManager.Instance.RegisterVendingMachine(this, CurrentIncome);

        if (buySound) audioSource.PlayOneShot(buySound);

        HidePopups();
    }

    // UI sluiten
    void HidePopups()
    {
        buyPopup.SetActive(false);
        upgradePopup.SetActive(false);
    }

    // Trigger detectie
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerNear = false;

        HidePopups();
    }
}
