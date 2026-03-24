using UnityEngine;

/// <summary>
/// Pomocné dev nástroje — dej na libovolný GameObject ve Workroom scéně.
/// Tlačítka napoj přes Inspector → On Click → DevTools → příslušná metoda.
/// </summary>
public class DevTools : MonoBehaviour
{
    public void ResetSave()
    {
        GameStateManager.Instance.DeleteSave();
        Debug.Log("[DevTools] Save resetován.");
    }
}
