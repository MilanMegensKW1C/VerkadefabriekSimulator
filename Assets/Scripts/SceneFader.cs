/*
 * SceneFader.cs
 * Auteur: Milan Megens
 *
 * Verantwoordelijk voor scene fades (in en uit) en het correct
 * terugplaatsen van de speler na een scene load.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade Settings")]
    // Duur van de fade in seconden
    public float fadeDuration = 1.0f;

    // UI Image dat gebruikt wordt als zwart fade overlay
    private Image fadeImage;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Als er al een instance bestaat, deze vernietigen
            Destroy(gameObject);
            return;
        }

        // Referentie ophalen naar het Image component
        fadeImage = GetComponent<Image>();

        StartCoroutine(FadeIn());
    }

    void OnEnable()
    {
        // Luister naar scene loaded events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Stop met luisteren bij disable
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Wordt aangeroepen zodra een nieuwe scene geladen is
    private void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        // Fade van zwart naar transparant
        StartCoroutine(FadeIn());

        // Speler eventueel terugplaatsen op opgeslagen spawn
        StartCoroutine(ApplySavedSpawn());
    }

    // Publieke methode om naar een scene te faden
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // Fade naar zwart en laad daarna de nieuwe scene
    IEnumerator FadeOutAndLoad(string scene)
    {
        float t = 0;
        Color c = fadeImage.color;

        // Alpha verhogen van 0 naar 1
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        // Scene asynchroon laden
        yield return SceneManager.LoadSceneAsync(scene);
    }

    // Fade van zwart naar transparant
    IEnumerator FadeIn()
    {
        float t = 0;
        Color c = fadeImage.color;

        // Begin volledig zwart
        c.a = 1;
        fadeImage.color = c;

        // Alpha verlagen van 1 naar 0
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    // -------------------------------------------------------
    // Plaatst de speler na scene load terug bij de juiste deur
    // -------------------------------------------------------
    IEnumerator ApplySavedSpawn()
    {
        // Geen opgeslagen spawn, dus niets doen
        if (!SpawnManager.hasReturnSpawn)
            yield break;

        // Wacht een paar frames zodat alle objects bestaan
        yield return null;
        yield return null;

        // Zoek de speler via tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            yield break;

        // Zoek alle LobbyDoors in de scene
        LobbyDoor[] doors = GameObject.FindObjectsOfType<LobbyDoor>();
        LobbyDoor match = null;

        // Zoek deur met dezelfde ID als opgeslagen
        foreach (var d in doors)
        {
            if (d.doorId == SpawnManager.returnDoorId)
            {
                match = d;
                break;
            }
        }

        // Als een match gevonden is en er een spawnpunt is
        if (match != null && match.returnSpawn != null)
        {
            // Speler positioneren
            player.transform.position = match.returnSpawn.position;

            // Rotatie toepassen
            FirstPersonLook look = player.GetComponentInChildren<FirstPersonLook>();
            if (look != null)
                look.ApplyRotationEuler(SpawnManager.returnEuler);
            else
                player.transform.rotation = Quaternion.Euler(SpawnManager.returnEuler);
        }

        // Reset spawn data zodat het niet opnieuw toegepast wordt
        SpawnManager.hasReturnSpawn = false;
        SpawnManager.returnDoorId = "";
        SpawnManager.returnEuler = Vector3.zero;
    }
}
