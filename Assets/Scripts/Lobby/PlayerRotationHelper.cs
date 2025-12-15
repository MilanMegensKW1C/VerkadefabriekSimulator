using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class PlayerRotationHelper
{
    public static IEnumerator SetPlayerRotationSafely(Transform playerRoot, Vector3 worldEuler, float waitFrames = 1)
    {
        if (playerRoot == null) yield break;

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

        TryDisableTypeIfExists("StarterAssets.FirstPersonController", playerRoot, toDisable);
        TryDisableTypeIfExists("UnityStandardAssets.Characters.FirstPerson.FirstPersonController", playerRoot, toDisable);

        var prevStates = new Dictionary<Behaviour, bool>();
        foreach (var b in toDisable)
        {
            if (b == null) continue;
            prevStates[b] = b.enabled;
            b.enabled = false;
        }

        Rigidbody rb = playerRoot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(Vector3.zero);
        }

        for (int i = 0; i < Math.Max(1, (int)waitFrames); i++)
            yield return null;

        playerRoot.rotation = Quaternion.Euler(worldEuler);

        var cam = Camera.main;
        if (cam != null)
        {
            if (cam.transform.IsChildOf(playerRoot))
            {
                cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        yield return null;

        foreach (var kv in prevStates)
        {
            if (kv.Key != null)
                kv.Key.enabled = kv.Value;
        }
    }

    static void TryDisableTypeIfExists(string typeFullName, Transform root, List<Behaviour> collect)
    {
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
