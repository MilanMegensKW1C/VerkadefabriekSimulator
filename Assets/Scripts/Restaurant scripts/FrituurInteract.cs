using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class FrituurInteract : MonoBehaviour
{
    [Header("Mandjes en knop")]
    public Transform mandje1;
    public Transform mandje2;
    public Transform draaiknop;

    [Header("Audio positie")]
    public Transform frituurAudioObject;    // 🔊 Waar het geluid vandaan komt

    [Header("Instellingen")]
    public string playerTag = "Player";
    public float liftHoogte = 0.25f;
    public float dipDiepte = 0.15f;
    public float bewegingTijd = 1.2f;
    public float knopHoek = 45f;
    public float knopSnelheid = 2f;

    [Header("Timing")]
    public float delayTussenMandjes = 0.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sounds")]
    public AudioClip sizzleSound;
    public float sizzleVolume = 1f;

    private AudioSource sizzleSource;

    private bool spelerInRange = false;
    private bool isAnimating = false;
    private bool isAan = false;

    private Vector3 mandje1Start, mandje2Start;
    private Quaternion knopStartRot;
    private Quaternion knopTargetRot;

    void Start()
    {
        mandje1Start = mandje1.localPosition;
        mandje2Start = mandje2.localPosition;

        knopStartRot = draaiknop.localRotation;
        knopTargetRot = knopStartRot * Quaternion.Euler(0f, 0f, -knopHoek);

        // 🔊 AudioSource komt op de frituurpan, niet op dit scriptobject!
        if (frituurAudioObject == null)
            frituurAudioObject = this.transform; // fallback

        sizzleSource = frituurAudioObject.gameObject.AddComponent<AudioSource>();
        sizzleSource.clip = sizzleSound;
        sizzleSource.loop = true;
        sizzleSource.volume = sizzleVolume;
        sizzleSource.spatialBlend = 1f;      // volledig 3D geluid
        sizzleSource.maxDistance = 7f;
        sizzleSource.rolloffMode = AudioRolloffMode.Linear;
        sizzleSource.playOnAwake = false;
    }

    void Update()
    {
        if (spelerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isAnimating)
            {
                if (!isAan)
                    StartCoroutine(StartFrituurAnimatie());
                else
                    StartCoroutine(StopFrituurAnimatie());
            }
        }
    }

    private IEnumerator StartFrituurAnimatie()
    {
        isAnimating = true;
        isAan = true;

        StartCoroutine(RoteerKnop(true));

        StartCoroutine(BeweegMandje_DualPhase(mandje1, mandje1Start, true));
        yield return new WaitForSeconds(delayTussenMandjes);

        StartCoroutine(BeweegMandje_DualPhase(mandje2, mandje2Start, true));

        yield return new WaitForSeconds(bewegingTijd + 0.1f + delayTussenMandjes);

        isAnimating = false;
    }

    private IEnumerator StopFrituurAnimatie()
    {
        isAnimating = true;
        isAan = false;

        if (sizzleSource.isPlaying)
            sizzleSource.Stop();

        StartCoroutine(RoteerKnop(false));

        StartCoroutine(BeweegMandje_DualPhase(mandje1, mandje1Start, false));
        yield return new WaitForSeconds(delayTussenMandjes);

        StartCoroutine(BeweegMandje_DualPhase(mandje2, mandje2Start, false));

        yield return new WaitForSeconds(bewegingTijd + 0.1f + delayTussenMandjes);

        isAnimating = false;
    }

    private IEnumerator BeweegMandje_DualPhase(Transform mandje, Vector3 startPos, bool naarBeneden)
    {
        float half = bewegingTijd * 0.5f;

        if (naarBeneden)
        {
            Vector3 peakAbove = startPos + Vector3.up * liftHoogte;

            // phase 1
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                mandje.localPosition = Vector3.Lerp(startPos, peakAbove, ease.Evaluate(t / half));
                yield return null;
            }

            Vector3 dipBelow = startPos + Vector3.down * dipDiepte;

            // 🔊 BEGIN SIZZLE
            if (isAan && !sizzleSource.isPlaying)
                sizzleSource.Play();

            // phase 2
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                mandje.localPosition = Vector3.Lerp(peakAbove, dipBelow, ease.Evaluate(t / half));
                yield return null;
            }

            mandje.localPosition = dipBelow;
        }
        else
        {
            Vector3 dipBelow = startPos + Vector3.down * dipDiepte;
            Vector3 peakAbove = startPos + Vector3.up * liftHoogte;

            // 🔊 stop zodra ze uit het vet komen
            if (sizzleSource.isPlaying)
                sizzleSource.Stop();

            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                mandje.localPosition = Vector3.Lerp(dipBelow, peakAbove, ease.Evaluate(t / half));
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                mandje.localPosition = Vector3.Lerp(peakAbove, startPos, ease.Evaluate(t / half));
                yield return null;
            }

            mandje.localPosition = startPos;
        }
    }

    private IEnumerator RoteerKnop(bool naarLinks)
    {
        Quaternion from = draaiknop.localRotation;
        Quaternion to = naarLinks ? knopTargetRot : knopStartRot;

        float t = 0f;
        float dur = 1f / knopSnelheid;

        while (t < dur)
        {
            t += Time.deltaTime;
            draaiknop.localRotation = Quaternion.Slerp(from, to, ease.Evaluate(t / dur));
            yield return null;
        }

        draaiknop.localRotation = to;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            spelerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            spelerInRange = false;
    }
}
