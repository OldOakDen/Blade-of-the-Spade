using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class OSMTileLoader : MonoBehaviour
{
    public int initialZoom = 15; // Úroveò pøiblížení pøi startu
    public double latitude = 49.4894; // GPS souøadnice støedu mapy
    public double longitude = 16.6581;
    public float zoomSpeed = 5f; // Rychlost zoomování
    public float panSpeed = 10f; // Rychlost posunu

    private int currentZoom; // Aktuální úroveò pøiblížení

    void Start()
    {
        currentZoom = initialZoom;
        LoadMap();
    }

    void Update()
    {
        // Zoomování pomocí koleèka myši
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom += (int)(scroll * zoomSpeed);

        // Posun mapy pomocí kurzorových tlaèítek
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector2 panDirection = new Vector2(horizontal, vertical) * panSpeed * Time.deltaTime;
        latitude += panDirection.y;
        longitude += panDirection.x;

        // Naètení mapy pøi zmìnì zoomu nebo posunu
        if (Mathf.Abs(scroll) > 0 || Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0)
        {
            LoadMap();
        }
    }

    void LoadMap()
    {
        // Pøevod GPS souøadnic na dlaždicové souøadnice
        int tileX = LonToTile(longitude, currentZoom);
        int tileY = LatToTile(latitude, currentZoom);

        StartCoroutine(LoadTile(currentZoom, tileX, tileY));
    }

    IEnumerator LoadTile(int zoom, int x, int y)
    {
        string url = $"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load tile: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            GetComponent<Renderer>().material.mainTexture = texture;
        }
    }

    int LonToTile(double lon, int zoom)
    {
        return (int)((lon + 180.0) / 360.0 * Mathf.Pow(2, zoom));
    }

    int LatToTile(double lat, int zoom)
    {
        double latRad = lat * Mathf.Deg2Rad;
        float result = (float)((1.0 - Mathf.Log(Mathf.Tan((float)latRad) + 1.0f / Mathf.Cos((float)latRad)) / Mathf.PI) / 2.0 * Mathf.Pow(2.0f, zoom));
        return (int)result;
    }

}
