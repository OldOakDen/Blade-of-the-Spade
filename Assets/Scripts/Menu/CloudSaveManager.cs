using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.CloudSave;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using UnityEngine;
using Newtonsoft.Json; // Pro JSON serializaci/deserializaci

public class CloudSaveManager : MonoBehaviour
{
    private static CloudSaveManager instance;

    public static CloudSaveManager Instance // Veøejná vlastnost pro pøístup k instanci
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<CloudSaveManager>(); // Rychlejší alternativa k FindObjectOfType
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Zajistí, e objekt zùstane aktivní i po zmìnì scény
        }
        else
        {
            Destroy(gameObject);  // Zabrání duplicitám objektu pøi opakovaném naètení menu
        }
    }

    public async void LoadData()
    {
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "character" }, new LoadOptions(new PublicReadAccessClassOptions()));
            if (playerData.TryGetValue("character", out var characterData))
            {
                var data = characterData.Value.GetAs<Dictionary<string, object>>();
                print("TYPE: " + int.Parse(data["type"].ToString()) + " COLOR: " + int.Parse(data["color_index"].ToString()));
            }
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }

    public async void SaveItem(string itemID)
    {
        try
        {
            string locationID = LocationHolder.Instance.selectedLocation.id;

            // Naèteme existující data pro lokaci
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { locationID });

            string foundItemsString;

            // Zkontrolujeme, jestli ji existují nálezy pro danou lokaci
            if (loadedData.TryGetValue(locationID, out var locationData))
            {
                // Získáme string hodnotu z naèteného objektu
                foundItemsString = locationData.Value.GetAsString();
            }
            else
            {
                // Pokud pro lokaci ádné pøedmìty neexistují, inicializujeme prázdnı øetìzec
                foundItemsString = "";
            }

            // Pøidáme novı nález (ID + identifikaèní stav)
            string newItemEntry = $"{itemID}:000"; // ID a inicializovanı identifikaèní stav
            if (!string.IsNullOrEmpty(foundItemsString))
            {
                foundItemsString += ","; // Pøidáme oddìlovaè
            }
            foundItemsString += newItemEntry;

            // Uloíme aktualizovanı øetìzec zpìt pod ID lokace
            var data = new Dictionary<string, object>
            {
                { locationID, foundItemsString }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);

            Debug.Log("Item successfully saved to cloud for location: " + locationID);
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save item: " + exception.Message);
        }
    }

    public async Task<List<string>> LoadItems()
    {
        List<string> foundItemIDs = new List<string>();

        try
        {
            string locationID = LocationHolder.Instance.selectedLocation.id;

            // Naèteme existující data pro lokaci
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { locationID });

            if (loadedData.TryGetValue(locationID, out var locationData))
            {
                // Získáme string hodnotu z naèteného objektu
                string foundItemsString = locationData.Value.GetAsString();

                // Rozdìlíme øetìzec na jednotlivé poloky
                string[] foundItemsArray = foundItemsString.Split(',');

                foreach (var item in foundItemsArray)
                {
                    string[] itemDetails = item.Split(':'); // Rozdìlení na ID a identifikaèní stav
                    string itemId = itemDetails[0]; // ID poloky

                    foundItemIDs.Add(itemId); // Pøidáme ID poloky do seznamu
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to load items: " + exception.Message);
        }

        return foundItemIDs; // Vra seznam nalezenıch ID
    }

    public async Task<string> GetItemIdentificationStatus(string itemID) //metoda pro zjisteni stavu identifikace premdetu - vraci tri znaky za dvojteckou
    {
        try
        {
            string locationID = LocationHolder.Instance.selectedLocation.id;

            // Naèteme data pro danou lokaci
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { locationID });

            if (loadedData.TryGetValue(locationID, out var locationData))
            {
                // Získáme string hodnotu z naèteného objektu
                string foundItemsString = locationData.Value.GetAsString();

                // Rozdìlíme string na jednotlivé poloky (formát "itemID:XXX")
                string[] foundItemsArray = foundItemsString.Split(',');

                foreach (var item in foundItemsArray)
                {
                    string[] itemDetails = item.Split(':'); // Rozdìlení na ID a identifikaèní stav
                    if (itemDetails[0] == itemID)
                    {
                        // Vrátíme tøíznakovı identifikaèní stav
                        return itemDetails[1];
                    }
                }
            }

            // Pokud pøedmìt nebyl nalezen
            Debug.LogWarning("Item not found: " + itemID);
            return null;
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to get item identification status: " + exception.Message);
            return null;
        }
    }
    public async Task<bool> UpdateItemIdentificationStatus(string itemID, string newStatus)
    {
        // Ovìøujeme, e novı stav má správnou délku
        if (newStatus.Length != 3)
        {
            Debug.LogError("New status must be exactly 3 characters long.");
            return false;
        }

        try
        {
            string locationID = LocationHolder.Instance.selectedLocation.id;

            // Naèteme existující data pro lokaci
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { locationID });

            string foundItemsString;

            // Zkontrolujeme, jestli ji existují nálezy pro danou lokaci
            if (loadedData.TryGetValue(locationID, out var locationData))
            {
                // Získáme string hodnotu z naèteného objektu
                foundItemsString = locationData.Value.GetAsString();
            }
            else
            {
                Debug.LogWarning("No items found for location: " + locationID);
                return false; // Pokud ádné pøedmìty neexistují, ukonèíme metodu
            }

            // Rozdìlíme øetìzec na jednotlivé poloky
            string[] foundItemsArray = foundItemsString.Split(',');

            // Hledáme pøedmìt a aktualizujeme jeho stav
            for (int i = 0; i < foundItemsArray.Length; i++)
            {
                string[] itemDetails = foundItemsArray[i].Split(':');
                if (itemDetails[0] == itemID)
                {
                    // Aktualizujeme stav
                    foundItemsArray[i] = $"{itemID}:{newStatus}";
                    break;
                }
            }

            // Spojíme pole zpìt do øetìzce
            foundItemsString = string.Join(",", foundItemsArray);

            // Uloíme aktualizovanı øetìzec zpìt pod ID lokace
            var data = new Dictionary<string, object>
        {
            { locationID, foundItemsString }
        };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);

            Debug.Log("Item identification status successfully updated for item: " + itemID);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to update item identification status: " + exception.Message);
            return false;
        }
    }
    // Metoda pro uloení skóre pro konkrétní lokaci
    public async void SaveScoreForLocation(int score)
    {
        string locationID = LocationHolder.Instance.selectedLocation.id;
        try
        {
            // Klíè pro ukládání skóre pro danou lokaci
            string scoreKey = $"score_{locationID}";

            // Vytvoøení SaveItem pro skóre
            var saveItem = new Unity.Services.CloudSave.Models.SaveItem(score, null); // Pøidání skóre jako hodnoty a null jako metadata

            var data = new Dictionary<string, SaveItem>
        {
            { scoreKey, saveItem }  // Ukládáme SaveItem pod klíèem specifickım pro lokaci
        };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);  // Pouijeme SaveAsync místo ForceSaveAsync

            Debug.Log("Score successfully saved for location " + locationID + ": " + score);
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save score for location " + locationID + ": " + exception.Message);
        }
    }



    // Metoda pro naètení skóre pro konkrétní lokaci
    public async Task<int> LoadScoreForLocation()
    {
        string locationID = LocationHolder.Instance.selectedLocation.id;
        try
        {
            // Klíè pro naèítání skóre pro danou lokaci
            string scoreKey = $"score_{locationID}";

            // Vytvoøení HashSet pro klíè
            var keys = new HashSet<string> { scoreKey };

            // Naètení dat ze sluby Cloud Save
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (loadedData.TryGetValue(scoreKey, out var saveItem))
            {
                // Pøevod SaveItem.Value na int
                return Convert.ToInt32(saveItem.Value);
            }

            return 0; // Pokud není skóre nalezeno, vrátíme 0
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to load score for location " + locationID + ": " + exception.Message);
            return 0; // Pokud dojde k chybì, vrátíme 0
        }
    }

}
