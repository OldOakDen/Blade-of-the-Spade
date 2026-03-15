using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LocationRegistry", menuName = "Registry/LocationRegistry")]
public class LocationRegistry : ScriptableObject
{
    public List<LocationConfigurationSO> locations;  // Seznam všech ScriptableObject pro lokace

    // Metoda pro nalezení lokace na základì ID
    public LocationConfigurationSO GetLocationByID(string id)
    {
        return locations.Find(location => location.id == id);
    }
}
