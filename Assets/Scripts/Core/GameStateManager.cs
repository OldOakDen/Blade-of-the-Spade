using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

[Serializable]
public class MapMarkerData
{
    public float x;
    public float z;
    public string placementID;
}

[Serializable]
public class GameSaveData
{
    public bool                       gameStarted       = false;
    public Dictionary<string, bool>   messages          = new Dictionary<string, bool>();
    public Dictionary<string, bool>   foundArtifacts    = new Dictionary<string, bool>();
    public Dictionary<string, bool>   hypotheses        = new Dictionary<string, bool>();
    public Dictionary<string, string> polygons          = new Dictionary<string, string>();
    public List<MapMarkerData>        mapMarkers        = new List<MapMarkerData>();
    public List<string>               processedActions  = new List<string>();
    public int                        prestigePoints    = 0;
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameSaveData State { get; private set; } = new GameSaveData();

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
        SceneManager.LoadScene("WorkroomScene");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // -------------------------------------------------------------------------
    // Save / Load
    // -------------------------------------------------------------------------

    public void SaveGame()
    {
        try
        {
            string json = JsonConvert.SerializeObject(State, Formatting.Indented);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log("[GameStateManager] Saved to: " + SaveFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("[GameStateManager] SaveGame failed: " + e.Message);
        }
    }

    public bool HasSaveData() => File.Exists(SaveFilePath) && State.gameStarted;

    public void MarkGameStarted()
    {
        State.gameStarted = true;
        SaveGame();
    }

    public void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
            File.Delete(SaveFilePath);
        State = new GameSaveData();
        Debug.Log("[GameStateManager] Save deleted, state reset.");
    }

    public void LoadGame()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                State = JsonConvert.DeserializeObject<GameSaveData>(json) ?? new GameSaveData();
                Debug.Log("[GameStateManager] Loaded from: " + SaveFilePath);
            }
            else
            {
                State = new GameSaveData();
                Debug.Log("[GameStateManager] No save found, created new game state.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[GameStateManager] LoadGame failed: " + e.Message);
            State = new GameSaveData();
        }
    }

    // -------------------------------------------------------------------------
    // Messages
    // -------------------------------------------------------------------------

    public bool IsMessageRead(string id) =>
        State.messages.TryGetValue(id, out bool read) && read;

    public void MarkMessageRead(string id) =>
        State.messages[id] = true;

    public void SetMessageState(string id, bool read) =>
        State.messages[id] = read;

    // -------------------------------------------------------------------------
    // Artifacts
    // -------------------------------------------------------------------------

    public bool IsArtifactFound(string id) =>
        State.foundArtifacts.TryGetValue(id, out bool found) && found;

    public void MarkArtifactFound(string id) =>
        State.foundArtifacts[id] = true;

    // -------------------------------------------------------------------------
    // Hypotheses
    // -------------------------------------------------------------------------

    public bool IsHypothesisBuilt(string id) =>
        State.hypotheses.TryGetValue(id, out bool built) && built;

    public void MarkHypothesisBuilt(string id) =>
        State.hypotheses[id] = true;

    // -------------------------------------------------------------------------
    // Polygons
    // -------------------------------------------------------------------------

    public string GetPolygonState(string id) =>
        State.polygons.TryGetValue(id, out string state) ? state : "locked";

    public void SetPolygonState(string id, string state) =>
        State.polygons[id] = state;

    // -------------------------------------------------------------------------
    // Map markers
    // -------------------------------------------------------------------------

    public void AddMapMarker(float x, float z, string placementID)
    {
        State.mapMarkers.Add(new MapMarkerData { x = x, z = z, placementID = placementID });
    }

    public void RemoveMapMarker(string placementID)
    {
        State.mapMarkers.RemoveAll(m => m.placementID == placementID);
    }

    // -------------------------------------------------------------------------
    // Prestige
    // -------------------------------------------------------------------------

    public void AddPrestigePoints(int amount) =>
        State.prestigePoints += amount;

    public int GetPrestigePoints() =>
        State.prestigePoints;

    // -------------------------------------------------------------------------
    // Processed actions
    // -------------------------------------------------------------------------

    public bool IsActionProcessed(string id) =>
        State.processedActions.Contains(id);

    public void MarkActionProcessed(string id)
    {
        if (!State.processedActions.Contains(id))
            State.processedActions.Add(id);
    }
}
