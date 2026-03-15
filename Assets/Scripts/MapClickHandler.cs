using UnityEngine;
using UnityEngine.EventSystems;

public class MapClickHandler : MonoBehaviour, IPointerClickHandler
{
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 clickPosition = hit.point;
            Vector2 latLon = WorldToGeoPosition(clickPosition);
            Debug.Log("Clicked coordinates: " + latLon);

            if (IsCorrectLocation(latLon))
            {
                StartVirtualSearch(latLon);
            }
            else
            {
                Debug.Log("Wrong location! Try again.");
            }
        }
    }

    Vector2 WorldToGeoPosition(Vector3 worldPosition)
    {
        // Zjednodušený pøevod pro demonstraèní úèely
        float mapWidth = 100f; // Šíøka mapy v Unity jednotkách
        float mapHeight = 100f; // Výška mapy v Unity jednotkách

        float latMin = 49.0f; // Minimální latituda mapy
        float latMax = 51.0f; // Maximální latituda mapy
        float lonMin = 13.0f; // Minimální longituda mapy
        float lonMax = 15.0f; // Maximální longituda mapy

        float lat = Mathf.Lerp(latMin, latMax, worldPosition.z / mapHeight);
        float lon = Mathf.Lerp(lonMin, lonMax, worldPosition.x / mapWidth);

        return new Vector2(lat, lon);
    }

    bool IsCorrectLocation(Vector2 latLon)
    {
        Vector2 correctLocation = new Vector2(50.0755f, 14.4378f); // Pøíklad správné lokace
        float tolerance = 0.01f; // Tolerance v stupních

        return Vector2.Distance(latLon, correctLocation) < tolerance;
    }

    void StartVirtualSearch(Vector2 latLon)
    {
        Debug.Log("Starting virtual search at: " + latLon);
        // Naètení scény nebo aktivace módu pro virtuální hledání
    }
}
