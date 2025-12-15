/*
 * FlashlightToggle.cs
 * Auteur: Milan Megens
 */

using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    // Referentie naar het Light component (zaklamp)
    public Light flashlight;

    // Toets om de zaklamp te togglen
    public KeyCode toggleKey = KeyCode.F;

    void Update()
    {
        // Controleer of de toggle toets is ingedrukt
        if (Input.GetKeyDown(toggleKey))
        {
            // Zet de zaklamp aan of uit
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
