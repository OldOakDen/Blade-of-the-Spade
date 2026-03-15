using UnityEngine;

public class LocationHolder : MonoBehaviour
{
    public static LocationHolder Instance;
    public LocationConfigurationSO selectedLocation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Uchování objektu mezi scénami
        }
        else
        {
            Destroy(gameObject);  // Zniè další instance
        }
    }

    public void SetLocation(LocationConfigurationSO location)
    {
        selectedLocation = location;
    }
}
