using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LocationConfig", menuName = "Custom/Location Configuration", order = 1)]
public class LocationConfigurationSO : ScriptableObject
{
    public string id; // Unikátní ID každé lokality
    public string locationName; // Jméno lokality

    public List<ItemConfiguration> itemConfigurations = new List<ItemConfiguration>();

    // Funkce pro vrácení prefabu podle ID
    public GameObject GetItemPrefabByID(string itemID)
    {
        foreach (ItemConfiguration itemConfig in itemConfigurations)
        {
            DetectTarget targetComponent = itemConfig.prefab.GetComponent<DetectTarget>(); // Získáme komponentu DetectTarget
            if (targetComponent != null && targetComponent.itemID == itemID)
            {
                return itemConfig.prefab; // Vrátíme správný prefab
            }
        }
        return null; // Pokud nenajdeme, vrátíme null
    }
}