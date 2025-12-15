/*
 * SpawnManager.cs
 * Auteur: Milan Megens
 */

using UnityEngine;

public static class SpawnManager
{
    // Id van de deur waar de speler vandaan komt
    public static string returnDoorId = "";

    // Rotatie (Euler angles) waarmee de speler moet spawnen
    public static Vector3 returnEuler = Vector3.zero;

    // Geeft aan of er geldige return-spawn data aanwezig is
    public static bool hasReturnSpawn = false;

    public static void Clear()
    {
        returnDoorId = "";
        returnEuler = Vector3.zero;
        hasReturnSpawn = false;
    }
}
