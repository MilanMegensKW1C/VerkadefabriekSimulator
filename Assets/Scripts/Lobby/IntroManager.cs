/*
 * IntroManager.cs
 * Auteur: Milan Megens
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntroLine
{
    // Tekst van de introzin (meerdere regels toegestaan)
    [TextArea(2, 4)]
    public string text;
}

public class IntroManager : MonoBehaviour
{
    // Zorgt ervoor dat de intro maar één keer per sessie speelt
    private static bool playedThisSession = false;

    [Header("Portrait")]
    // Portret van de spreker
    public Sprite opaPortrait;

    // Naam van de spreker
    public string speakerName = "Opa Koos";

    [Header("Intro Zinnen")]
    // Lijst met intro zinnen
    public List<IntroLine> introLines = new List<IntroLine>();

    void Start()
    {
        // Als de intro deze sessie al is afgespeeld → stoppen
        if (playedThisSession)
            return;

        // Wacht tot DialogueUI beschikbaar is en start dan de intro
        StartCoroutine(WaitAndPlayIntro());
    }

    // Wacht tot DialogueUI bestaat en speel daarna de intro
    IEnumerator WaitAndPlayIntro()
    {
        float timeout = 5f;
        float t = 0f;

        // Wacht maximaal 5 seconden op DialogueUI
        while (DialogueUI.Instance == null && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Geen DialogueUI gevonden → niets doen
        if (DialogueUI.Instance == null)
            yield break;

        // Verzamel alle niet-lege intro zinnen
        List<string> lines = new List<string>();
        foreach (var l in introLines)
        {
            if (!string.IsNullOrWhiteSpace(l.text))
                lines.Add(l.text);
        }

        // Geen geldige zinnen → stoppen
        if (lines.Count == 0)
            yield break;

        // Start de dialoog
        DialogueUI.Instance.StartDialogue(speakerName, opaPortrait, lines);

        // Markeer intro als afgespeeld voor deze sessie
        playedThisSession = true;
    }
}
