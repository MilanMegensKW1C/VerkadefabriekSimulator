using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Robuuste helper: zet speler rotatie veilig zonder dat movement/look het direct overschrijft.
public static class PlayerRotationHelper
{
    // Geef player root (Player) en gewenste wereld-euler (y = yaw)
    public static IEnumerator SetPlayerRotationSafely(Transform playerRoot, Vector3 worldEuler, float waitFrames = 1)
    {
        if (playerRoot == null) yield break;

        // 1) vind veelvoorkomende components die input/rotation doen en disable ze
        var toDisable = new List<Behaviour>();
        var toDisableTypes = new string[]
        {
            "MouseLook",
            "FirstPersonController",
            "PlayerMovement",
            "PlayerController",
            "PlayerMove",
            "PlayerLook",
            "StarterAssets.FirstPersonController",
            "CharacterRotation",
            "PlayerInput",
            "MouseLookScript",
            "LookScript",
            "LookController"
        };

        // voeg alle MonoBehaviours op playerRoot en kinderen toe waarvan de type-naam in lijst staat
        var allBehaviours = playerRoot.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var b in allBehaviours)
        {
            if (b == null) continue;
            string typeName = b.GetType().FullName;
            foreach (var tn in toDisableTypes)
            {
                if (typeName.IndexOf(tn, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    toDisable.Add(b);
                    break;
                }
            }
        }

        // Forceer ook disabling op common named components via exact type lookups (if present)
        TryDisableTypeIfExists("StarterAssets.FirstPersonController", playerRoot, toDisable);
        TryDisableTypeIfExists("UnityStandardAssets.Characters.FirstPerson.FirstPersonController", playerRoot, toDisable);

        // store enabled states then disable
        var prevStates = new Dictionary<Behaviour, bool>();
        foreach (var b in toDisable)
        {
            if (b == null) continue;
            prevStates[b] = b.enabled;
            b.enabled = false;
        }

        // 2) stop physics/momentum
        Rigidbody rb = playerRoot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null)
        {
            // CharacterController heeft geen velocity, maar we can call Move zero to clear internal pending movement
            cc.Move(Vector3.zero);
        }

        // 3) wacht een frame of aantoe (soms nodig zodat other systems flushen)
        for (int i = 0; i < Math.Max(1, (int)waitFrames); i++)
            yield return null;

        // 4) zet rotatie op player root (wereldruimte)
        playerRoot.rotation = Quaternion.Euler(worldEuler);

        // 5) reset local camera rotation if camera is child
        var cam = Camera.main;
        if (cam != null)
        {
            // als de camera een child is van playerRoot, reset local rotation so pitch doesn't stay weird
            if (cam.transform.IsChildOf(playerRoot))
            {
                cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        // 6) kort wachten zodat andere scripts die nog in LateUpdate kunnen draaien niet overschrijven
        yield return null;

        // 7) show a final small delay and then re-enable previous components
        foreach (var kv in prevStates)
        {
            if (kv.Key != null)
                kv.Key.enabled = kv.Value;
        }
    }

    static void TryDisableTypeIfExists(string typeFullName, Transform root, List<Behaviour> collect)
    {
        // zoek type in assemblies
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = asm.GetType(typeFullName);
            if (t != null && typeof(MonoBehaviour).IsAssignableFrom(t))
            {
                var comps = root.GetComponentsInChildren(t, true);
                foreach (var c in comps)
                {
                    if (c is Behaviour b && !collect.Contains(b))
                        collect.Add(b);
                }
                break;
            }
        }
    }
}
