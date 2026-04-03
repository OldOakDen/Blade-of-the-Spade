using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blade of the Spade/Location Database")]
public class LocationDatabase : ScriptableObject
{
    public List<LocationData> allLocations = new List<LocationData>();

    public LocationData GetLocation(string locationID) =>
        allLocations.Find(l => l.locationID == locationID);

    public List<LocationData> GetLocationsForMap(string mapID)
    {
        if (string.IsNullOrEmpty(mapID)) return new List<LocationData>();
        return allLocations.FindAll(l => l.mapID == mapID);
    }
}
