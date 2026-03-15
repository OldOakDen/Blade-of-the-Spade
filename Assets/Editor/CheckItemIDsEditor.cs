using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CheckItemIDsEditor : EditorWindow
{
    [MenuItem("Tools/Check Item IDs")]
    public static void CheckIDs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item"); // Najde všechny Item ScriptableObject podle typu
        HashSet<string> uniqueIDs = new HashSet<string>();
        Dictionary<string, string> itemFiles = new Dictionary<string, string>(); // Mapování ID na jméno souboru

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

            if (item == null)
            {
                Debug.LogWarning($"Asset at path {assetPath} is not of type Item.");
                continue;
            }

            string fileName = System.IO.Path.GetFileName(assetPath); // Získá jméno souboru
            string itemID = item.itemID;

            if (uniqueIDs.Contains(itemID))
            {
                Debug.LogError($"Duplicate item ID found: {itemID}. Files: {itemFiles[itemID]}, {fileName}");
            }
            else
            {
                uniqueIDs.Add(itemID);
                itemFiles[itemID] = fileName;
            }
        }

        if (uniqueIDs.Count == 0)
        {
            Debug.Log("No items found.");
        }
        else
        {
            Debug.Log("Item ID check completed.");
        }
    }
}
