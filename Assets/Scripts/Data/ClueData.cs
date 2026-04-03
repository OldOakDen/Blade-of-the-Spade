using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blade of the Spade/Clue")]
public class ClueData : ScriptableObject
{
    public enum ClueType { Document, Map, Note, Artifact }

    public string clueID;

    [Header("Lokalizace → tabulka: CluesTable")]
    public string clueNameKey;
    public string clueContentKey;
    public ClueType     clueType;
    public bool         isAvailableByDefault;
    public List<string> availableInMapIDs = new List<string>();
    public Sprite       icon;
}
