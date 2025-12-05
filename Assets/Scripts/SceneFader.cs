using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;

    private Image fadeImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        fadeImage = GetComponent<Image>();
        StartCoroutine(FadeIn());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
        StartCoroutine(ApplySavedSpawn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string scene)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        yield return SceneManager.LoadSceneAsync(scene);
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        Color c = fadeImage.color;
        c.a = 1;
        fadeImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
    }

    // -------------------------------------------------------
    // Nieuwe methode om na scene load speler op juiste plek te zetten
    // -------------------------------------------------------
    IEnumerator ApplySavedSpawn()
    {
        // Als er geen spawn moet gebeuren → klaar
        if (!SpawnManager.hasReturnSpawn)
            yield break;

        // Wacht tot objects bestaan
        yield return null;
        yield return null;

        // Zoek speler
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            yield break;

        // Zoek een lobbydeur met zelfde ID
        LobbyDoor[] doors = GameObject.FindObjectsOfType<LobbyDoor>();
        LobbyDoor match = null;

        foreach (var d in doors)
        {
            if (d.doorId == SpawnManager.returnDoorId)
            {
                match = d;
                break;
            }
        }

        if (match != null && match.returnSpawn != null)
        {
            // Positioneren
            player.transform.position = match.returnSpawn.position;

            // Rotatie toepassen via FPS look
            FirstPersonLook look = player.GetComponentInChildren<FirstPersonLook>();
            if (look != null)
                look.ApplyRotationEuler(SpawnManager.returnEuler);
            else
                player.transform.rotation = Quaternion.Euler(SpawnManager.returnEuler);
        }

        // Reset zodat het niet opnieuw toepast
        SpawnManager.hasReturnSpawn = false;
        SpawnManager.returnDoorId = "";
        SpawnManager.returnEuler = Vector3.zero;
    }
}
