using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public LocationRegistry locationRegistry;  // Odkaz na registr všech lokací

    public void LoadSceneWithLocationID(string sceneName, string locationID)
    {
        // Najdi správný LocationScriptableObject na základì ID
        LocationConfigurationSO selectedLocation = locationRegistry.GetLocationByID(locationID);

        if (selectedLocation != null)
        {
            // Nastav lokaci do LocationHolder, aby byla pøístupná po naètení scény
            LocationHolder.Instance.SetLocation(selectedLocation);

            // Naèti scénu
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Lokace s tímto ID neexistuje!");
        }
    }
}
