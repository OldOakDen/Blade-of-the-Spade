using UnityEngine;

/// <summary>
/// Zpracovává herní akce spouštěné potvrzením zprávy v komunikaci.
/// Přidej nové case pro každé nové actionID.
/// </summary>
public static class MessageActionHandler
{
    public static void Execute(string actionID)
    {
        if (string.IsNullOrEmpty(actionID))
        {
            Debug.LogWarning("[MessageActionHandler] actionID je prázdné.");
            return;
        }

        switch (actionID)
        {
            case "update_map_C3":
                GameStateManager.Instance?.SetPolygonState("C3", "unlocked");
                EventBus.PolygonUnlocked("C3");
                Debug.Log("[MessageActionHandler] Polygon C3 odemčen.");
                break;

            // --- přidej další akce sem ---
            // case "unlock_artifact_001":
            //     GameStateManager.Instance?.MarkArtifactFound("artifact_001");
            //     break;

            default:
                Debug.LogWarning($"[MessageActionHandler] Neznámé actionID: '{actionID}'");
                break;
        }
    }
}
