using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blade of the Spade/Location")]
public class LocationData : ScriptableObject
{
    public string locationID;
    public string mapID;
    public int    requiredPrestigeLevel;

    [Header("Lokalizace → tabulka: LocationsTable")]
    public string locationNameKey;
    public string hintTextKey;
    public string hypothesisNameKey;
    public string hypothesisDescKey;

    [Header("Propojení s Komunikací")]
    [Tooltip("actionID zprávy v KomunikaceUI která potvrzuje tuto hypotézu. Prázdné = žádné čekání na potvrzení.")]
    public string hypothesisActionID;
    [Tooltip("messageID zprávy která se odemkne po odeslání hypotézy pro tuto lokaci.")]
    public string hypothesisMessageID;

    public List<ClueData> requiredClues = new List<ClueData>();
}
