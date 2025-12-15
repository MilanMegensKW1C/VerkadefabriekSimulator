/*
 * MoneyUI.cs
 * Auteur: Milan Megens
 */

using UnityEngine;
using TMPro;
using System.Collections;

public class MoneyUI : MonoBehaviour
{
    public TMP_Text tmpText;

    [Header("Format")]
    public string prefix = "€ ";

    public string thousandSeparator = ".";

    [Header("Animation")]
    public float countDuration = 0.4f;

    int currentDisplayed;

    Coroutine animRoutine;

    void Start()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.OnMoneyChanged += PlayAnimation;

            // Direct het juiste bedrag tonen zonder animatie bij start
            currentDisplayed = MoneyManager.Instance.Money;
            UpdateMoneyText(currentDisplayed);
        }
        else
        {
            // Waarschuwing als MoneyManager ontbreekt
            Debug.LogWarning("MoneyManager missing in scene");
        }
    }

    void OnDestroy()
    {
        // Afmelden van event
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= PlayAnimation;
    }

    // Start een animatie naar het nieuwe doelbedrag
    void PlayAnimation(int target)
    {
        // Stop eventuele lopende animatie
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        // Start nieuwe animatie van huidig naar doelbedrag
        animRoutine = StartCoroutine(AnimateCounter(currentDisplayed, target));
    }

    // Animeert het getoonde bedrag van startValue naar endValue
    IEnumerator AnimateCounter(int startValue, int endValue)
    {
        float timer = 0f;

        // Tel zolang de animatieduur niet voorbij is
        while (timer < countDuration)
        {
            timer += Time.deltaTime;
            float t = timer / countDuration;

            // Interpoleer het bedrag
            int value = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            UpdateMoneyText(value);

            // Wacht tot het volgende frame
            yield return null;
        }

        // Zorg dat het eindbedrag exact klopt
        UpdateMoneyText(endValue);
        currentDisplayed = endValue;
        animRoutine = null;
    }

    // Update de tekst in de UI
    void UpdateMoneyText(int amount)
    {
        currentDisplayed = amount;
        string formatted = FormatAmount(amount);
        tmpText.text = prefix + formatted;
    }

    // Formatteert een int naar duizendtallen (bijv. 123456 → 123.456)
    private string FormatAmount(int amount)
    {
        if (amount == 0)
            return "0";

        // Werk met absolute waarde voor formatting
        string s = Mathf.Abs(amount).ToString();

        // Voeg duizendtallen scheiding toe
        for (int i = s.Length - 3; i > 0; i -= 3)
            s = s.Insert(i, thousandSeparator);

        // Negatief teken opnieuw toevoegen indien nodig
        return (amount < 0 ? "-" : "") + s;
    }
}
