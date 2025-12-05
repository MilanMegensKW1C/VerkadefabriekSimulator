using UnityEngine;

public static class SpawnManager
{
    public static string returnDoorId = "";
    public static Vector3 returnEuler = Vector3.zero;
    public static bool hasReturnSpawn = false;

    public static void Clear()
    {
        returnDoorId = "";
        returnEuler = Vector3.zero;
        hasReturnSpawn = false;
    }
}