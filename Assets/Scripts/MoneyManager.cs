using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    private Dictionary<VendingMachine, int> activeMachines = new Dictionary<VendingMachine, int>();

    private AudioSource incomeAudioSource;

    [Header("Start Geld")]
    public int startMoney = 250;

    [Header("UI (optioneel)")]
    public TMP_Text moneyText;

    [Header("Geluiden")]
    public AudioClip gainMoneySound;

    public event Action<int> OnMoneyChanged;

    private int currentMoney;
    public int Money => currentMoney;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            incomeAudioSource = gameObject.AddComponent<AudioSource>();
            incomeAudioSource.spatialBlend = 0f; // 0 = volledig 2D
            incomeAudioSource.playOnAwake = false;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void Start()
    {
        currentMoney = startMoney;
        UpdateMoneyTextInstant();
        OnMoneyChanged?.Invoke(currentMoney);

        StartCoroutine(GlobalIncomeLoop());
    }

    // ----------------------------------------------------------
    // Geld toevoegen en UI animatie
    // ----------------------------------------------------------
    public void AddMoney(int amount)
    {
        int oldValue = currentMoney;
        currentMoney += amount;

        if (gainMoneySound != null && incomeAudioSource != null)
            incomeAudioSource.PlayOneShot(gainMoneySound);

        StartCoroutine(AnimateMoneyChange(oldValue, currentMoney));
        OnMoneyChanged?.Invoke(currentMoney);
    }

    // ----------------------------------------------------------
    // TryRemoveMoney (voor vending machines)
    // ----------------------------------------------------------
    public bool TryRemoveMoney(int amount)
    {
        if (currentMoney < amount)
            return false;

        int oldValue = currentMoney;
        currentMoney -= amount;

        StartCoroutine(AnimateMoneyChange(oldValue, currentMoney));
        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    // ----------------------------------------------------------
    // UI animatie
    // ----------------------------------------------------------
    IEnumerator AnimateMoneyChange(int oldValue, int newValue)
    {
        if (moneyText == null)
            yield break;

        float t = 0f;
        float duration = 0.35f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            int display = Mathf.RoundToInt(Mathf.Lerp(oldValue, newValue, t));
            moneyText.text = display.ToString();
            yield return null;
        }

        moneyText.text = newValue.ToString();
    }

    private void UpdateMoneyTextInstant()
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString();
    }

    // ====== DEUR PURCHASE SYSTEM ======

    private readonly HashSet<string> purchasedDoors = new HashSet<string>();

    public bool IsDoorPurchased(string doorId)
    {
        return purchasedDoors.Contains(doorId);
    }

    public bool TryBuyDoor(string doorId, int price)
    {
        if (currentMoney < price)
            return false;

        int oldValue = currentMoney;
        currentMoney -= price;

        purchasedDoors.Add(doorId);

        StartCoroutine(AnimateMoneyChange(oldValue, currentMoney));
        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    public void RegisterVendingMachine(VendingMachine machine, int income)
    {
        if (!activeMachines.ContainsKey(machine))
            activeMachines.Add(machine, income);
        else
            activeMachines[machine] = income;
    }

    IEnumerator GlobalIncomeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            foreach (var machine in activeMachines)
            {
                AddMoney(machine.Value);

                // 2D income sound
                if (machine.Key.incomeSound != null)
                {
                    // Gebruik incomeAudioSource die spatialBlend = 0 heeft
                    incomeAudioSource.PlayOneShot(machine.Key.incomeSound);
                }
            }
        }
    }
}
