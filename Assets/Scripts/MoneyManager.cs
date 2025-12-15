/*
 * Moneymanager.cs
 * Auteur: Milan Megens
 * Bewerker: Lev Posthumus
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
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

    // Globaal toegankelijke instance
    public static MoneyManager Instance;

    // Runtime seat data
    private Dictionary<string, SeatData> seatData = new Dictionary<string, SeatData>();

    // Runtime vending machine data
    private List<VendingData> vendingMachines = new List<VendingData>();

    // Opgeslagen seat levels (persistent tussen scenes)
    private Dictionary<string, int> savedSeatLevels = new Dictionary<string, int>();

    // Audio voor income feedback
    private AudioSource incomeAudio;

    [Header("Start Geld")]
    // Startbedrag bij begin van het spel
    public int startMoney = 250;

    // Of het geld bij start gereset moet worden
    public bool resetMoneyOnStart = true;

    [Header("Geluiden")]
    // Geluid bij geld ontvangen
    public AudioClip gainMoneySound;

    // Event dat afgaat bij geldverandering
    public event Action<int> OnMoneyChanged;

    // Huidig geldbedrag
    private int currentMoney;

    // Publieke read-only access
    public int Money => currentMoney;

    // Timer voor globale seat payout
    private float globalTimer = 0f;

    // Interval voor seat inkomsten
    private const float GLOBAL_PAY_INTERVAL = 60f;

    // Gekochte generieke items
    private readonly HashSet<string> purchasedItems = new HashSet<string>();


    // ------------------------------
    // Awake + Start
    // ------------------------------

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // AudioSource dynamisch toevoegen
            incomeAudio = gameObject.AddComponent<AudioSource>();
            incomeAudio.spatialBlend = 0f;
            incomeAudio.playOnAwake = false;
        }
        else
        {
            // Dubbele manager verwijderen
            Destroy(this.gameObject);
            return;
        }
    }

    void Start()
    {
        // Startgeld instellen
        if (resetMoneyOnStart)
            currentMoney = startMoney;

        // UI direct updaten
        OnMoneyChanged?.Invoke(currentMoney);

        // Start inkomsten loop
        StartCoroutine(TickerLoop());
    }

    // Voeg geld toe
    public void AddMoney(int amount)
    {
        if (amount == 0) return;

        currentMoney += amount;

        // Speel geluid af bij inkomsten
        if (gainMoneySound != null)
            incomeAudio.PlayOneShot(gainMoneySound);

        // Event triggeren
        OnMoneyChanged?.Invoke(currentMoney);
    }

    // Probeer geld af te trekken
    public bool TryRemoveMoney(int amount)
    {
        if (currentMoney < amount) return false;

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }


    // ------------------------------
    // Door purchases
    // ------------------------------

    // Gekochte deuren
    private readonly HashSet<string> purchasedDoors = new HashSet<string>();

    // Check of een deur al gekocht is
    public bool IsDoorPurchased(string doorId) => purchasedDoors.Contains(doorId);

    // Probeer een deur te kopen
    public bool TryBuyDoor(string doorId, int price)
    {
        if (currentMoney < price) return false;

        currentMoney -= price;
        purchasedDoors.Add(doorId);
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }


    // ------------------------------
    // Seat persistence
    // ------------------------------

    // Sla seat level op
    public void SaveSeatLevel(string seatId, int level)
    {
        savedSeatLevels[seatId] = level;
    }

    // Haal seat level op
    public int GetSeatLevel(string seatId)
    {
        if (savedSeatLevels.TryGetValue(seatId, out int lvl))
            return lvl;

        return 0;
    }


    // ------------------------------
    // Seat API (runtime income)
    // ------------------------------

    // Registreer een stoel en zijn inkomsten
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

    // Update inkomsten van een stoel
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
    // Vendingmachine
    // ------------------------------

    // Registreer vending machine
    public void RegisterVendingMachine(VendingMachine machine, int incomePerMinute)
    {
        float interval = 60f;

        // Probeer interval uit machine te halen
        try { interval = machine.moneyInterval; }
        catch { interval = 60f; }

        // Update bestaande machine indien al geregistreerd
        foreach (var v in vendingMachines)
        {
            if (v.machine == machine)
            {
                v.incomePerMinute = incomePerMinute;
                v.interval = interval;
                return;
            }
        }

        // Nieuwe machine toevoegen
        vendingMachines.Add(new VendingData
        {
            machine = machine,
            incomePerMinute = incomePerMinute,
            interval = interval,
            nextTime = Time.time + interval
        });
    }

    // Update vending inkomsten
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

        // Nog niet geregistreerd
        RegisterVendingMachine(machine, newIncomePerMinute);
    }


    // ------------------------------
    // Income
    // ------------------------------

    // Centrale loop die alle inkomsten afhandelt
    IEnumerator TickerLoop()
    {
        while (true)
        {
            // Elke frame checken
            yield return null;
            float now = Time.time;

            // Vending machine payouts
            foreach (var v in vendingMachines)
            {
                if (now >= v.nextTime)
                {
                    if (v.incomePerMinute > 0)
                        AddMoney(v.incomePerMinute);

                    // Volgende payout plannen
                    v.nextTime = now + Mathf.Max(0.01f, v.interval);
                }
            }

            // Seat payouts via globale timer
            globalTimer += Time.deltaTime;

            if (globalTimer >= GLOBAL_PAY_INTERVAL)
            {
                int total = 0;

                // Alle stoel inkomsten optellen
                foreach (var s in seatData.Values)
                    total += s.incomePerMinute;

                if (total > 0)
                    AddMoney(total);

                // Timer resetten met resttijd
                globalTimer %= GLOBAL_PAY_INTERVAL;
            }
        }
    }


    // ------------------------------
    // Utility
    // ------------------------------

    // Totale inkomsten per minuut berekenen
    public int GetTotalIncomePerMinute()
    {
        int total = 0;
        foreach (var s in seatData.Values) total += s.incomePerMinute;
        foreach (var v in vendingMachines) total += v.incomePerMinute;
        return total;
    }

    // Check of een item gekocht is
    public bool IsItemPurchased(string itemId)
    {
        return purchasedItems.Contains(itemId);
    }

    // Probeer een item te kopen
    public bool TryBuyItem(string itemId, int price)
    {
        if (currentMoney < price) return false;

        currentMoney -= price;
        purchasedItems.Add(itemId);
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }
}
