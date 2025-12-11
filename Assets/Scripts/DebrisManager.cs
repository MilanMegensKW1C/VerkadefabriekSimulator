using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisManager : MonoBehaviour
{
    [Header("Debris points (assign in inspector)")]
    public List<DebrisPoint> debrisPoints = new List<DebrisPoint>();

    [Header("Linked bin ID")]
    public string binId = "trash_1";

    [Header("Timing")]
    public float spawnInterval = 12f;
    public int spawnBatch = 1; // hoeveel per spawn attempt

    void Start()
    {
        // Zorg dat alle punten in eerste instantie INACTIVE zijn
        foreach (var d in debrisPoints)
            if (d != null)
                d.HideDebris();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (MoneyManager.Instance == null)
                continue;

            // Als prullenbak gekocht → STOP met spawnen (maar laat bestaande propjes staan)
            if (MoneyManager.Instance.IsItemPurchased(binId))
            {
                // do not cleanup existing; just skip spawning
                continue;
            }

            // find inactive (dormant) debris points
            List<DebrisPoint> dormant = new List<DebrisPoint>();
            foreach (var d in debrisPoints)
                if (d != null && !d.gameObject.activeSelf)
                    dormant.Add(d);

            int toSpawn = Mathf.Min(spawnBatch, dormant.Count);
            for (int i = 0; i < toSpawn; i++)
            {
                DebrisPoint chosen = dormant[Random.Range(0, dormant.Count)];
                chosen.ShowDebris();
                dormant.Remove(chosen);
            }
        }
    }

    // helper om alles in area op te ruimen (kan door trashbin gebruikt worden)
    public void CleanupAll()
    {
        foreach (var d in debrisPoints)
            if (d != null)
                d.HideDebris();
    }
}
