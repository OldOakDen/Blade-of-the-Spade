using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class ItemConfiguration
{
    public GameObject prefab; // Prefab pøedmìtu
    public int count; // Poèet pøedmìtù
    public Vector2 depthRange; // Rozmezí hloubek (Y osa)
}

public class UndergroundDiggingArea : MonoBehaviour
{
    public Vector3 size; // Velikost oblasti
    public Vector3 center; // Støed oblasti
    public GameObject treasures; // Rodièovský objekt pro poklady
    private LocationConfigurationSO itemConfigurationsSO; // ScriptableObject s konfigurací
    public int seed = 401001; // Seed pro generátor náhodných èísel

    private System.Random random;

    void Start()
    {
        print("LOKACE: "+LocationHolder.Instance.selectedLocation + " ID: "+ LocationHolder.Instance.selectedLocation.id);
        itemConfigurationsSO = LocationHolder.Instance.selectedLocation;

        SpawnItems(); //spawne predmety, ktere hrac v teto lokaci jeste nenasel

        //CloudSaveManager.Instance.LoadData();

        center = transform.position;
        size = transform.localScale;
        random = new System.Random(seed);

        /* foreach (var itemConfig in itemConfigurationsSO.itemConfigurations)
         {
             GameObject prefab = itemConfig.prefab;
             if (prefab == null)
             {
                 Debug.LogError("Prefab not assigned in ItemConfiguration.");
                 continue;
             }

             for (int i = 0; i < itemConfig.count; i++)
             {
                 SpawnTreasure(prefab, itemConfig.depthRange);
             }
         }*/
    }

    public void SpawnTreasure(GameObject prefab, Vector2 depthRange)
    {
        float depth = (float)(random.NextDouble() * (depthRange.y - depthRange.x) + depthRange.x);
        Vector3 pos = center + new Vector3(RandomRange(-size.x / 2, size.x / 2), depth, RandomRange(-size.z / 2, size.z / 2));

        Quaternion rotation = RandomRotation();

        GameObject newTarget = Instantiate(prefab, pos, rotation);
        newTarget.transform.SetParent(treasures.transform);
    }

    public async void SpawnItems()
    {
        // Naèti seznam nalezených ID položek
        List<string> foundItemIDs = await CloudSaveManager.Instance.LoadItems();

        foreach (var itemConfig in itemConfigurationsSO.itemConfigurations)
        {
            GameObject prefab = itemConfig.prefab;
            if (prefab == null)
            {
                Debug.LogError("Prefab not assigned in ItemConfiguration.");
                continue;
            }

            for (int i = 0; i < itemConfig.count; i++)
            {
                string itemID = prefab.GetComponent<DetectTarget>().itemID; // itemID je sice stejne jako jmeno prefabu, ale to nemusi byt vzdy pravda. Smerodatne je itemID.

                // Zkontroluj, zda ID položky je v seznamu nalezených položek
                if (foundItemIDs.Contains(itemID))
                {
                    //Debug.Log($"Item {itemID} already found, skipping spawn.");
                    Debug.Log($"Item already found, skipping spawn.");
                    continue; // Vynechej položku, pokud byla nalezena
                }

                SpawnTreasure(prefab, itemConfig.depthRange);
            }
        }
    }

    private float RandomRange(float min, float max)
    {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    private Quaternion RandomRotation()
    {
        return Quaternion.Euler((float)random.NextDouble() * 360, (float)random.NextDouble() * 360, (float)random.NextDouble() * 360);
    }
}
