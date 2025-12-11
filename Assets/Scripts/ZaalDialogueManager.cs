using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class ZaalLine
{
    [TextArea(2, 4)]
    public string text;
}

public class ZaalDialogueManager : MonoBehaviour
{
    private static bool playedThisSession = false;

    [Header("Portrait")]
    public Sprite portrait;

    public string speakerName = "Opa Koos";

    [Header("Zaal uitleg")]
    public List<ZaalLine> lines = new List<ZaalLine>();

    [Header("Video (niet gebruikt)")]
    public VideoPlayer videoPlayer;

    [Header("Bonus")]
    public int startBonus = 100;

    void Start()
    {
        if (playedThisSession)
            return;

        StartCoroutine(WaitAndPlay());
    }

    IEnumerator WaitAndPlay()
    {
        float timeout = 5f;
        float t = 0f;

        while (DialogueUI.Instance == null && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (DialogueUI.Instance == null)
            yield break;

        // Bouw tekstregels
        List<string> textLines = new List<string>();
        foreach (var l in lines)
        {
            if (!string.IsNullOrWhiteSpace(l.text))
                textLines.Add(l.text);
        }

        textLines.Add("Hier heb je 300 euro, koop je eerste stoel.");

        // Luisteren naar einde dialog
        DialogueUI.Instance.OnDialogueFinished += OnDialogueDone;

        // Start dialoog
        DialogueUI.Instance.StartDialogue(speakerName, portrait, textLines);

        playedThisSession = true;
    }

    void OnDialogueDone()
    {
        DialogueUI.Instance.OnDialogueFinished -= OnDialogueDone;

        // Bonus geld geven
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(startBonus);

        // GEEN video acties meer
    }
}
