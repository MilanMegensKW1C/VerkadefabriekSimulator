using UnityEngine;
using TMPro;

public class SeatSlot : MonoBehaviour
{
    [Header("Seat Level Models")]
    public GameObject level1Model;
    public GameObject level2Model;
    public GameObject level3Model;

    [Header("Materials")]
    public Material ghostMaterial;
    private Material[] originalLevel1Mats;

    [Header("Seat Data")]
    public string seatId;
    public int currentLevel = 0;

    [Header("Economy")]
    public int buyPrice = 100;
    public int upgrade1Price = 200;
    public int upgrade2Price = 300;

    public int incomeLv1 = 5;
    public int incomeLv2 = 10;
    public int incomeLv3 = 20;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    private Transform cam;

    [Header("UI")]
    public Canvas buyPopup;
    public Canvas upgradePopup;

    [Header("UI Text Fields")]
    public TextMeshProUGUI buyPriceText;
    public TextMeshProUGUI buyIncomeText;

    public TextMeshProUGUI currentIncomeText;
    public TextMeshProUGUI upgradePriceText;
    public TextMeshProUGUI upgradeIncomeText;

    [Header("Max Level UI (STATIC)")]
    public Canvas maxLevelCanvas;
    public TextMeshProUGUI maxIncomeLeftText;
    public TextMeshProUGUI maxIncomeRightText;

    [Header("Audio")]
    public AudioClip purchaseSuccessSound;
    public AudioClip purchaseFailSound;
    private AudioSource audioSource;

    [Header("UI Feedback Colors")]
    public Color errorColor = Color.red;
    public float errorFlashDuration = 0.4f;
    private Color originalPriceColor;

    private bool isInRange;
    Renderer[] level1Renderers;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;

        level1Renderers = level1Model.GetComponentsInChildren<Renderer>();

        originalLevel1Mats = new Material[level1Renderers.Length];
        for (int i = 0; i < level1Renderers.Length; i++)
            originalLevel1Mats[i] = level1Renderers[i].sharedMaterial;

        buyPopup.enabled = false;
        upgradePopup.enabled = false;
        if (maxLevelCanvas != null)
            maxLevelCanvas.enabled = false;

        // restore saved level
        if (MoneyManager.Instance != null)
        {
            int saved = MoneyManager.Instance.GetSeatLevel(seatId);
            if (saved > 0)
                currentLevel = saved;

            if (currentLevel > 0)
                MoneyManager.Instance.RegisterSeat(this, GetCurrentIncome());
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (buyPriceText != null)
            originalPriceColor = buyPriceText.color;

        UpdateSeatVisual();
        UpdatePopupText();
    }

    void Update()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        CheckRange2D();
        BillboardPopups();

        if (currentLevel == 0)
        {
            buyPopup.enabled = isInRange;
            upgradePopup.enabled = false;

            if (isInRange && Input.GetKeyDown(KeyCode.E))
                TryBuySeat();
        }
        else if (currentLevel < 3)
        {
            buyPopup.enabled = false;
            upgradePopup.enabled = isInRange;

            if (isInRange && Input.GetKeyDown(KeyCode.E))
                TryUpgradeSeat();
        }
        else
        {
            buyPopup.enabled = false;
            upgradePopup.enabled = false;
        }
    }

    void CheckRange2D()
    {
        if (!cam) return;

        Vector3 seatPos = transform.position;
        Vector3 camPos = cam.position;

        seatPos.y = 0f;
        camPos.y = 0f;

        float dist = Vector3.Distance(seatPos, camPos);
        isInRange = dist <= interactDistance;
    }

    void BillboardPopups()
    {
        if (!cam) return;

        if (buyPopup != null)
            buyPopup.transform.LookAt(buyPopup.transform.position + cam.forward);

        if (upgradePopup != null)
            upgradePopup.transform.LookAt(upgradePopup.transform.position + cam.forward);

        // static maxLevelCanvas → blijft staan
    }

    void TryBuySeat()
    {
        if (MoneyManager.Instance == null) return;

        if (MoneyManager.Instance.TryRemoveMoney(buyPrice))
        {
            // ✔ aankoop gelukt
            if (purchaseSuccessSound != null)
                audioSource.PlayOneShot(purchaseSuccessSound);

            currentLevel = 1;
            MoneyManager.Instance.SaveSeatLevel(seatId, currentLevel);
            MoneyManager.Instance.RegisterSeat(this, GetCurrentIncome());

            UpdateSeatVisual();
            UpdatePopupText();
        }
        else
        {
            // ❌ te weinig geld
            if (purchaseFailSound != null)
                audioSource.PlayOneShot(purchaseFailSound);

            StartCoroutine(FlashPriceRed(buyPriceText));
        }
    }

    void TryUpgradeSeat()
    {
        if (MoneyManager.Instance == null) return;

        int upgradeCost = (currentLevel == 1) ? upgrade1Price : upgrade2Price;

        if (MoneyManager.Instance.TryRemoveMoney(upgradeCost))
        {
            // ✔ upgrade gelukt
            if (purchaseSuccessSound != null)
                audioSource.PlayOneShot(purchaseSuccessSound);

            currentLevel++;
            MoneyManager.Instance.SaveSeatLevel(seatId, currentLevel);
            MoneyManager.Instance.UpdateSeatIncome(this, GetCurrentIncome());

            UpdateSeatVisual();
            UpdatePopupText();
        }
        else
        {
            // ❌ te weinig geld
            if (purchaseFailSound != null)
                audioSource.PlayOneShot(purchaseFailSound);

            StartCoroutine(FlashPriceRed(upgradePriceText));
        }
    }

    public void UpdateSeatVisual_Public()
    {
        UpdateSeatVisual();
    }

    void UpdateSeatVisual()
    {
        level1Model.SetActive(false);
        level2Model.SetActive(false);
        level3Model.SetActive(false);

        if (maxLevelCanvas != null)
            maxLevelCanvas.enabled = false;

        if (currentLevel == 0)
        {
            level1Model.SetActive(true);
            foreach (var r in level1Renderers)
                r.material = ghostMaterial;
        }
        else if (currentLevel == 1)
        {
            level1Model.SetActive(true);
            for (int i = 0; i < level1Renderers.Length; i++)
                level1Renderers[i].material = originalLevel1Mats[i];
        }
        else if (currentLevel == 2)
        {
            level2Model.SetActive(true);
        }
        else if (currentLevel == 3)
        {
            level3Model.SetActive(true);

            if (maxLevelCanvas != null)
            {
                maxLevelCanvas.enabled = true;

                string txt = incomeLv3 + "\np/m";
                maxIncomeLeftText.text = txt;
                maxIncomeRightText.text = txt;
            }
        }
    }

    void UpdatePopupText()
    {
        if (buyPriceText != null)
            buyPriceText.text = buyPrice.ToString();
        if (buyIncomeText != null)
            buyIncomeText.text = incomeLv1.ToString();

        if (currentLevel == 3)
            return;

        int currentIncome = (currentLevel == 1) ? incomeLv1 :
                            (currentLevel == 2) ? incomeLv2 : 0;

        int nextPrice = (currentLevel == 1) ? upgrade1Price : upgrade2Price;
        int nextIncome = (currentLevel == 1) ? incomeLv2 : incomeLv3;

        if (currentIncomeText != null)
            currentIncomeText.text = $"Huidig: {currentIncome}";

        if (upgradePriceText != null)
            upgradePriceText.text = nextPrice.ToString();

        if (upgradeIncomeText != null)
            upgradeIncomeText.text = nextIncome.ToString();
    }

    public int GetCurrentIncome()
    {
        if (currentLevel == 1) return incomeLv1;
        if (currentLevel == 2) return incomeLv2;
        if (currentLevel == 3) return incomeLv3;
        return 0;
    }

    System.Collections.IEnumerator FlashPriceRed(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        text.color = errorColor;
        yield return new WaitForSeconds(errorFlashDuration);
        text.color = originalPriceColor;
    }
}
