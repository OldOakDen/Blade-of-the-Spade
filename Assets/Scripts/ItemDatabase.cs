using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new List<Item>();

    // Metoda pro pøidání položky do databáze
    public void AddItem(Item newItem)
    {
        items.Add(newItem);
    }
}
