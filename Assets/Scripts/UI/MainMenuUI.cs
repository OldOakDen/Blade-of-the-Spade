using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Tlačítka")]
    public Button buttonNewGame;
    public Button buttonContinue;
    public Button buttonSettings;
    public Button buttonQuit;

    [Header("Dependencies")]
    public SceneController sceneController;

    [Header("DEBUG — přímý vstup do detekce")]
    public Button           buttonDebugDetect;
    public SceneLoader      debugSceneLoader;
    public LocationRegistry debugLocationRegistry;
    public string           debugLocationID = "loc_001";

    private void Start()
    {
        buttonContinue.interactable = GameStateManager.Instance != null
                                      && GameStateManager.Instance.HasSaveData();

        buttonNewGame.onClick.AddListener(OnNewGame);
        buttonContinue.onClick.AddListener(OnContinue);
        buttonSettings.onClick.AddListener(OnSettings);
        buttonQuit.onClick.AddListener(OnQuit);

        if (buttonDebugDetect != null)
            buttonDebugDetect.onClick.AddListener(OnDebugDetect);
    }

    private void OnNewGame()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.DeleteSave();

        sceneController.LoadMainGame();
    }

    private void OnContinue()
    {
        sceneController.LoadMainGame();
    }

    private void OnSettings()
    {
        Debug.Log("Nastavení TODO");
    }

    private void OnQuit()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.SaveGame();

        Application.Quit();
    }

    private void OnDebugDetect()
    {
        if (debugSceneLoader == null || debugLocationRegistry == null)
        {
            Debug.LogError("[MainMenuUI] DEBUG: debugSceneLoader nebo debugLocationRegistry není přiřazen.");
            return;
        }

        debugSceneLoader.locationRegistry = debugLocationRegistry;
        debugSceneLoader.LoadSceneWithLocationID("DetectingScene", debugLocationID);
    }
}
