using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // ------------------------------
    //  Data structs
    // ------------------------------
    private class SeatData
    {
        public string seatId;
        public int incomePerMinute;
    }

    private class VendingData
    {
        public VendingMachine machine;
        public int incomePerMinute;
        public float interval;
        public float nextTime;
    }

    // ------------------------------
    //  Singleton
    // ------------------------------
    public static MoneyManager Instance;

    // runtime data
    private Dictionary<string, SeatData> seatData = new Dictionary<string, SeatData>();
    private List<VendingData> vendingMachines = new List<VendingData>();

    // persistent seat levels
    private Dictionary<string, int> savedSeatLevels = new Dictionary<string, int>();

    private AudioSource incomeAudio;

    [Header("Start Geld")]
    public int startMoney = 250;
    public bool resetMoneyOnStart = true;

    [Header("Geluiden")]
    public AudioClip gainMoneySound;

    public event Action<int> OnMoneyChanged;

    private int currentMoney;
    public int Money => currentMoney;

    private float globalTimer = 0f;
    private const float GLOBAL_PAY_INTERVAL = 60f;

    private readonly HashSet<string> purchasedItems = new HashSet<string>();


    // ------------------------------
    //  Awake + Start
    // ------------------------------
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            incomeAudio = gameObject.AddComponent<AudioSource>();
            incomeAudio.spatialBlend = 0f;
            incomeAudio.playOnAwake = false;
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
        StartCoroutine(TickerLoop());
    }

    // ------------------------------
    //  Money ops
    // ------------------------------
    public void AddMoney(int amount)
    {
        if (amount == 0) return;

        currentMoney += amount;

        if (gainMoneySound != null)
            incomeAudio.PlayOneShot(gainMoneySound);

        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool TryRemoveMoney(int amount)
    {
        if (currentMoney < amount) return false;

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }


    // ------------------------------
    // Door purchase
    // ------------------------------
    private readonly HashSet<string> purchasedDoors = new HashSet<string>();

    public bool IsDoorPurchased(string doorId) => purchasedDoors.Contains(doorId);

    public bool TryBuyDoor(string doorId, int price)
    {
        if (currentMoney < price) return false;

        currentMoney -= price;
        purchasedDoors.Add(doorId);
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }


    // ------------------------------
    //  Seat persistence
    // ------------------------------
    public void SaveSeatLevel(string seatId, int level)
    {
        savedSeatLevels[seatId] = level;
    }

    public int GetSeatLevel(string seatId)
    {
        if (savedSeatLevels.TryGetValue(seatId, out int lvl))
            return lvl;

        return 0;
    }


    // ------------------------------
    // Seat API (runtime income)
    // ------------------------------
    public void RegisterSeat(SeatSlot seat, int incomePerMinute)
    {
        if (seatData.ContainsKey(seat.seatId))
        {
            seatData[seat.seatId].incomePerMinute = incomePerMinute;
        }
        else
        {
            seatData.Add(seat.seatId, new SeatData
            {
                seatId = seat.seatId,
                incomePerMinute = incomePerMinute
            });
        }
    }

    public void UpdateSeatIncome(SeatSlot seat, int newIncomePerMinute)
    {
        if (seatData.ContainsKey(seat.seatId))
        {
            seatData[seat.seatId].incomePerMinute = newIncomePerMinute;
        }
        else
        {
            RegisterSeat(seat, newIncomePerMinute);
        }
    }


    // ------------------------------
    // Vending API
    // ------------------------------
    public void RegisterVendingMachine(VendingMachine machine, int incomePerMinute)
    {
        float interval = 60f;

        try { interval = machine.moneyInterval; }
        catch { interval = 60f; }

        foreach (var v in vendingMachines)
        {
            if (v.machine == machine)
            {
                v.incomePerMinute = incomePerMinute;
                v.interval = interval;
                return;
            }
        }

        vendingMachines.Add(new VendingData
        {
            machine = machine,
            incomePerMinute = incomePerMinute,
            interval = interval,
            nextTime = Time.time + interval
        });
    }

    public void UpdateVendingMachine(VendingMachine machine, int newIncomePerMinute)
    {
        foreach (var v in vendingMachines)
        {
            if (v.machine == machine)
            {
                v.incomePerMinute = newIncomePerMinute;
                return;
            }
        }

        RegisterVendingMachine(machine, newIncomePerMinute);
    }


    // ------------------------------
    //  Income Ticker
    // ------------------------------
    IEnumerator TickerLoop()
    {
        while (true)
        {
            yield return null;
            float now = Time.time;

            // vending payout timers
            foreach (var v in vendingMachines)
            {
                if (now >= v.nextTime)
                {
                    if (v.incomePerMinute > 0)
                        AddMoney(v.incomePerMinute);

                    v.nextTime = now + Mathf.Max(0.01f, v.interval);
                }
            }

            // seat payouts
            globalTimer += Time.deltaTime;

            if (globalTimer >= GLOBAL_PAY_INTERVAL)
            {
                int total = 0;

                foreach (var s in seatData.Values)
                    total += s.incomePerMinute;

                if (total > 0)
                    AddMoney(total);

                globalTimer %= GLOBAL_PAY_INTERVAL;
            }
        }
    }


    // ------------------------------
    // Utility
    // ------------------------------
    public int GetTotalIncomePerMinute()
    {
        int total = 0;
        foreach (var s in seatData.Values) total += s.incomePerMinute;
        foreach (var v in vendingMachines) total += v.incomePerMinute;
        return total;
    }

    // Check of generiek item gekocht is
    public bool IsItemPurchased(string itemId)
    {
        return purchasedItems.Contains(itemId);
    }

    // Probeer generiek item te kopen; return true als geslaagd
    public bool TryBuyItem(string itemId, int price)
    {
        if (currentMoney < price) return false;
        currentMoney -= price;
        purchasedItems.Add(itemId);
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }
}
