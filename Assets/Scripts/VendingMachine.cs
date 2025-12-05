using UnityEngine;
using TMPro;
using System.Collections;

public class VendingMachine : MonoBehaviour
{
    [Header("Instellingen")]
    public string playerTag = "Player";
    public int maxLevel = 5;

    [Header("Level Costs / Income")]
    public int[] levelCosts = new int[6];
    public int[] levelIncome = new int[6];
    public float moneyInterval = 3f;

    [Header("UI - Buy Popup")]
    public GameObject buyPopup;
    public TextMeshProUGUI buyPriceText;
    public TextMeshProUGUI buyIncomeText;

    [Header("UI - Upgrade Popup")]
    public GameObject upgradePopup;
    public TextMeshProUGUI upgradePriceText;
    public TextMeshProUGUI upgradeIncomeText;
    public TextMeshProUGUI upgradePreviousIncomeText;

    [Header("UI - Geld Popup")]
    public TextMeshProUGUI countdownText;

    [Header("Interact Prompt (E)")]
    public GameObject interactCanvas;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buySound;
    public AudioClip errorSound;
    public AudioClip incomeSound;

    private bool isPurchased = false;
    private bool playerNear = false;
    private int upgradeLevel = 0;

    private int CurrentIncome => levelIncome[upgradeLevel];

    void Start()
    {
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        buyPopup.SetActive(false);
        upgradePopup.SetActive(false);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (interactCanvas) interactCanvas.SetActive(false);
    }

    void Update()
    {
        Billboard();

        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (!isPurchased)
                ShowBuyPopup();
            else
                ShowUpgradePopup();
        }

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

        if (interactCanvas)
            interactCanvas.SetActive(playerNear && !buyPopup.activeSelf && !upgradePopup.activeSelf);
    }

    // ---------------------------------------------------------
    // Billboard
    // ---------------------------------------------------------
    void Billboard()
    {
        if (interactCanvas && Camera.main)
        {
            interactCanvas.transform.LookAt(Camera.main.transform);
            interactCanvas.transform.Rotate(0, 180, 0);
        }
    }

    // ---------------------------------------------------------
    // BUY
    // ---------------------------------------------------------
    void ShowBuyPopup()
    {
        buyPriceText.text = levelCosts[0].ToString();
        buyIncomeText.text = levelIncome[0].ToString();

        buyPopup.SetActive(true);
        upgradePopup.SetActive(false);
    }

    void ConfirmBuy()
    {
        if (!MoneyManager.Instance.TryRemoveMoney(levelCosts[0]))
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        isPurchased = true;

        if (buySound) audioSource.PlayOneShot(buySound);

        MoneyManager.Instance.RegisterVendingMachine(this, CurrentIncome);

        HidePopups();
    }

    // ---------------------------------------------------------
    // UPGRADE
    // ---------------------------------------------------------
    void ShowUpgradePopup()
    {
        bool maxed = upgradeLevel >= maxLevel;

        if (maxed)
        {
            upgradePriceText.text = "MAX";
            upgradeIncomeText.text = levelIncome[upgradeLevel].ToString();
            upgradePreviousIncomeText.gameObject.SetActive(false);
        }
        else
        {
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
        if (upgradeLevel >= maxLevel)
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        int cost = levelCosts[upgradeLevel + 1];

        if (!MoneyManager.Instance.TryRemoveMoney(cost))
        {
            if (errorSound) audioSource.PlayOneShot(errorSound);
            return;
        }

        upgradeLevel++;

        MoneyManager.Instance.RegisterVendingMachine(this, CurrentIncome);

        if (buySound) audioSource.PlayOneShot(buySound);

        HidePopups();
    }

    // ---------------------------------------------------------
    // Hide popups
    // ---------------------------------------------------------
    void HidePopups()
    {
        buyPopup.SetActive(false);
        upgradePopup.SetActive(false);
    }

    // ---------------------------------------------------------
    // Income Loop
    // ---------------------------------------------------------
    IEnumerator IncomeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(moneyInterval);

            MoneyManager.Instance.AddMoney(CurrentIncome);

            StartCoroutine(IncomePopup());
        }
    }

    IEnumerator IncomePopup()
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = "+" + CurrentIncome;

        if (incomeSound)
            audioSource.PlayOneShot(incomeSound);

        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------
    // Triggers
    // ---------------------------------------------------------
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
