using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OSMDataFetcher : MonoBehaviour
{
    private string osmApiUrl = "https://api.openstreetmap.org/api/0.6/map?bbox={0}";

    public void FetchOSMData(float minLon, float minLat, float maxLon, float maxLat)
    {
        string url = string.Format(osmApiUrl, $"{minLon},{minLat},{maxLon},{maxLat}");
        StartCoroutine(GetOSMData(url));
    }

    private IEnumerator GetOSMData(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            // Process OSM data (XML format)
            Debug.Log(www.downloadHandler.text);
        }
    }
}
