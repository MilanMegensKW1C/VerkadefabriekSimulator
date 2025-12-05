using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    private class MachineData
    {
        public VendingMachine machine;
        public int income;
        public float interval;
        public float nextTime;
    }

    public static MoneyManager Instance;

    private List<MachineData> machines = new List<MachineData>();

    private AudioSource incomeAudioSource;

    [Header("Start Geld")]
    public int startMoney = 250;
    public bool resetMoneyOnStart = true;

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
        if (resetMoneyOnStart)
            currentMoney = startMoney;

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

        OnMoneyChanged?.Invoke(currentMoney);

        return true;
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

        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    public void RegisterVendingMachine(VendingMachine machine, int income)
    {
        // check of hij al bestaat → update
        foreach (var m in machines)
        {
            if (m.machine == machine)
            {
                m.income = income;
                m.interval = machine.moneyInterval;
                return;
            }
        }

        // anders → nieuwe toevoegen
        machines.Add(new MachineData
        {
            machine = machine,
            income = income,
            interval = machine.moneyInterval,
            nextTime = Time.time + machine.moneyInterval
        });
    }

    IEnumerator GlobalIncomeLoop()
    {
        while (true)
        {
            yield return null; // elke frame checken

            float t = Time.time;

            foreach (var m in machines)
            {
                if (t >= m.nextTime)
                {
                    AddMoney(m.income);
                    if (m.machine.incomeSound)
                        incomeAudioSource.PlayOneShot(m.machine.incomeSound);

                    m.nextTime = t + m.interval;
                }
            }
        }
    }
}
