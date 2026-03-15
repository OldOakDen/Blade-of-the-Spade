using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine.UI;
using Unity.Services.Friends;
using UnityEngine.SceneManagement;

public class MainMenu : Panel
{

    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button leaderboardsButton = null;
    [SerializeField] private Button friendsButton = null;
    [SerializeField] private Button renameButton = null;
    [SerializeField] private Button customizationButton = null;

    public LocationRegistry locationRegistry;  // Odkaz na registr lokací
    public string locationID;  // ID scriptable objektu pro lokaci - toto ID se bude zadavat scriptem, podle toho jakou lokaci hrac zvoli
    public SceneLoader sceneLoader;  // Odkaz na loader, který naète scénu

    [SerializeField] private Button loadLocationButton = null; // tlacitko pro nahrani sceny s detektorem

    private bool isFriendsServiceInitialized = false;
    
    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        leaderboardsButton.onClick.AddListener(Leaderboards);
        friendsButton.onClick.AddListener(Friends);
        renameButton.onClick.AddListener(RenamePlayer);
        customizationButton.onClick.AddListener(Customization);

        loadLocationButton.onClick.AddListener(() => LoadSceneWithLocation());

        base.Initialize();
    }

    private void LoadSceneWithLocation()
    {
        // Spustí naètení scény s daným ID lokace
        sceneLoader.LoadSceneWithLocationID("DetectingScene", locationID);  // Název scény pøizpùsob svému projektu
    }

    public override void Open()
    {
        friendsButton.interactable = isFriendsServiceInitialized;
        UpdatePlayerNameUI();
        if (isFriendsServiceInitialized == false)
        {
            InitializeFriendsServiceAsync();
        }
        base.Open();
    }
    
    private void Customization()
    {
        PanelManager.Open("customization");
    }
    
    private async void InitializeFriendsServiceAsync()
    {
        try
        {
            await FriendsService.Instance.InitializeAsync();
            isFriendsServiceInitialized = true;
            friendsButton.interactable = true;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }
    
    private void SignOut()
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(SignOutResult, "Are you sure that you want to sign out?", "Yes", "No");
    }
    
    private void SignOutResult(ActionConfirmMenu.Result result)
    {
        if (result == ActionConfirmMenu.Result.Positive)
        {
            MenuManager.Singleton.SignOut();
            isFriendsServiceInitialized = false;
        }
    }
    
    private void UpdatePlayerNameUI()
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }
    
    private void Leaderboards()
    {
        PanelManager.Open("leaderboards");
    }
    
    private void Friends()
    {
        PanelManager.Open("friends");
    }
    
    private void RenamePlayer()
    {
        GetInputMenu panel = (GetInputMenu)PanelManager.GetSingleton("input");
        panel.Open(RenamePlayerConfirm, GetInputMenu.Type.String, 20, "Enter a name for your account.", "Send", "Cancel");
    }
    
    private async void RenamePlayerConfirm(string input)
    {
        renameButton.interactable = false;
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(input);
            UpdatePlayerNameUI();
        }
        catch
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to change the account name.", "OK");
        }
        renameButton.interactable = true;
    }
    
}