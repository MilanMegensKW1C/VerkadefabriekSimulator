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
    private static bool playedThisSession = false;

    [Header("Portrait")]
    public Sprite opaPortrait;

    public string speakerName = "Opa Koos";

    [Header("Intro Zinnen")]
    public List<IntroLine> introLines = new List<IntroLine>();

    void Start()
    {
        // tijdens dezelfde sessie → niet opnieuw
        if (playedThisSession) return;

        StartCoroutine(WaitAndPlayIntro());
    }

    IEnumerator WaitAndPlayIntro()
    {
        float timeout = 5f;
        float t = 0f;

        // wachten op DialogueUI
        while (DialogueUI.Instance == null && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (DialogueUI.Instance == null)
            yield break;

        List<string> lines = new List<string>();
        foreach (var l in introLines)
        {
            if (!string.IsNullOrWhiteSpace(l.text))
                lines.Add(l.text);
        }

        if (lines.Count == 0)
            yield break;

        DialogueUI.Instance.StartDialogue(speakerName, opaPortrait, lines);

        playedThisSession = true;
    }
}
