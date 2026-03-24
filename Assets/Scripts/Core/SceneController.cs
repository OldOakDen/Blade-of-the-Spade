using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Název scén – změň podle skutečných názvů v Build Settings
    private const string SceneMainMenu  = "MainMenu";
    private const string SceneWorkroom  = "WorkroomScene";
    private const string SceneMainGame  = "MainGameScene";
    private const string SceneLocation  = "LocationScene";

    // Předává locationID do cílové scény přes změnu scény
    public static string PendingLocationID { get; private set; }

    // -------------------------------------------------------------------------

    public void LoadMainMenu()
    {
        SaveAndLoad(SceneMainMenu);
    }

    public void LoadWorkroom()
    {
        SaveAndLoad(SceneWorkroom);
    }

    public void LoadMainGame()
    {
        SaveAndLoad(SceneMainGame);
    }

    public void LoadLocation(string locationID)
    {
        PendingLocationID = locationID;
        SaveAndLoad(SceneLocation);
    }

    // -------------------------------------------------------------------------

    private void SaveAndLoad(string sceneName)
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SaveGame();

        SceneManager.LoadScene(sceneName);
    }
}
