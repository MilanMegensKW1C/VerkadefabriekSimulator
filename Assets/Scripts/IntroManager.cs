using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntroLine
{
    [TextArea(2, 4)]
    public string text;
}

public class IntroManager : MonoBehaviour
{
    [Header("Intro instellingen")]
    [Tooltip("Als true wordt de intro alleen de eerste keer in deze sessie gespeeld")]
    public bool playOncePerSession = true;

    [Tooltip("Portrait van Opa Koos, mag leeg blijven")]
    public Sprite opaPortrait;

    [Tooltip("Naam die boven de dialoog verschijnt")]
    public string speakerName = "Opa Koos";

    [Header("Intro Zinnen (aanpasbaar in Inspector!)")]
    public List<IntroLine> introLines = new List<IntroLine>();

    // intern
    private static bool playedThisSession = false;

    void Start()
    {
        if (playOncePerSession && playedThisSession) return;

        StartCoroutine(WaitAndPlayIntro());
    }

    IEnumerator WaitAndPlayIntro()
    {
        float timeout = 5f;
        float t = 0f;
        while (DialogueUI.Instance == null && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (DialogueUI.Instance == null)
        {
            Debug.LogWarning("IntroManager: DialogueUI niet gevonden. Zorg dat DialogueUI in de scene staat.");
            yield break;
        }

        // Zet Inspector-zinnen om naar string-lijst
        List<string> lines = new List<string>();
        foreach (var l in introLines)
        {
            if (!string.IsNullOrWhiteSpace(l.text))
                lines.Add(l.text);
        }

        if (lines.Count == 0)
        {
            Debug.LogWarning("IntroManager: Geen introLines ingesteld in de Inspector!");
            yield break;
        }

        // start dialoog
        DialogueUI.Instance.StartDialogue(speakerName, opaPortrait, lines);

        playedThisSession = true;
    }
}
