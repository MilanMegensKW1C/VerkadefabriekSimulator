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
            PlayAnimation(MoneyManager.Instance.Money);   // gebruik Money, geen CurrentMoney
        }
        else
        {
            Debug.LogWarning("MoneyManager missing in scene");
        }
    }

    void OnDestroy()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.OnMoneyChanged -= PlayAnimation;
    }

    void PlayAnimation(int target)
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(AnimateCounter(currentDisplayed, target));
    }

    IEnumerator AnimateCounter(int startValue, int endValue)
    {
        float timer = 0f;

        while (timer < countDuration)
        {
            timer += Time.deltaTime;
            float t = timer / countDuration;

            int value = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            UpdateMoneyText(value);

            yield return null;
        }

        UpdateMoneyText(endValue);
        currentDisplayed = endValue;
        animRoutine = null;
    }

    void UpdateMoneyText(int amount)
    {
        currentDisplayed = amount;
        string formatted = FormatAmount(amount);
        tmpText.text = prefix + formatted;
    }

    private string FormatAmount(int amount)
    {
        if (amount == 0) return "0";

        string s = Mathf.Abs(amount).ToString();
        for (int i = s.Length - 3; i > 0; i -= 3)
            s = s.Insert(i, thousandSeparator);

        return (amount < 0 ? "-" : "") + s;
    }
}
