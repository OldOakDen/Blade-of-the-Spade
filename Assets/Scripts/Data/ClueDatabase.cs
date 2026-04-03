using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blade of the Spade/Clue Database")]
public class ClueDatabase : ScriptableObject
{
    public List<ClueData> allClues = new List<ClueData>();

    public ClueData GetClue(string clueID) =>
        allClues.Find(c => c.clueID == clueID);

    /// <summary>
    /// Vrátí indicie viditelné pro danou mapu:
    /// isAvailableByDefault == true NEBO mapID je v availableInMapIDs.
    /// </summary>
    public List<ClueData> GetCluesForMap(string mapID) =>
        allClues.FindAll(c => c.isAvailableByDefault || c.availableInMapIDs.Contains(mapID));
}
