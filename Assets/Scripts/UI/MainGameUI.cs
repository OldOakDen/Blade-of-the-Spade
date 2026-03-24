using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameUI : MonoBehaviour
{
    [Header("Panely")]
    public GameObject navigationPanel;
    public GameObject komunikacePanel;
    public GameObject mapaPanel;
    public GameObject badatelnaPanel;
    public GameObject sbirkaPanel;
    public GameObject statistikyPanel;

    [Header("Navigační tlačítka")]
    public Button buttonBack;
    public Button buttonKomunikace;
    public Button buttonMapa;
    public Button buttonBadatelna;
    public Button buttonSbirka;
    public Button buttonStatistiky;

    [Header("Tlačítka Back v panelech")]
    public Button buttonBackMapa;
    public Button buttonBackBadatelna;
    public Button buttonBackSbirka;
    public Button buttonBackStatistiky;

    [Header("Vykřičníky na navigačních tlačítkách")]
    public Image exclamationKomunikace;
    public Image exclamationMapa;
    public Image exclamationBadatelna;
    public Image exclamationSbirka;
    public Image exclamationStatistiky;

    [Header("Dependencies")]
    public SceneController sceneController;

    // -------------------------------------------------------------------------

    private Dictionary<string, GameObject> _panels;
    private Dictionary<string, Image>      _exclamations;
    private bool                           _contentPanelOpen;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _panels = new Dictionary<string, GameObject>
        {
            { NotificationManager.SectionKomunikace, komunikacePanel },
            { NotificationManager.SectionMapa,       mapaPanel       },
            { NotificationManager.SectionBadatelna,  badatelnaPanel  },
            { NotificationManager.SectionSbirka,     sbirkaPanel     },
            { NotificationManager.SectionStatistiky, statistikyPanel },
        };

        _exclamations = new Dictionary<string, Image>
        {
            { NotificationManager.SectionKomunikace, exclamationKomunikace },
            { NotificationManager.SectionMapa,       exclamationMapa       },
            { NotificationManager.SectionBadatelna,  exclamationBadatelna  },
            { NotificationManager.SectionSbirka,     exclamationSbirka     },
            { NotificationManager.SectionStatistiky, exclamationStatistiky },
        };
    }

    private void OnEnable()
    {
        NotificationManager.OnChanged += OnNotificationChanged;
    }

    private void OnDisable()
    {
        NotificationManager.OnChanged -= OnNotificationChanged;
    }

    private void Start()
    {
        if (buttonBack != null)
            buttonBack.onClick.AddListener(GoToWorkroom);

        if (buttonBackMapa       != null) buttonBackMapa      .onClick.AddListener(GoBack);
        if (buttonBackBadatelna  != null) buttonBackBadatelna .onClick.AddListener(GoBack);
        if (buttonBackSbirka     != null) buttonBackSbirka    .onClick.AddListener(GoBack);
        if (buttonBackStatistiky != null) buttonBackStatistiky.onClick.AddListener(GoBack);

        buttonKomunikace.onClick.AddListener(() => ShowPanel(NotificationManager.SectionKomunikace));
        buttonMapa      .onClick.AddListener(() => ShowPanel(NotificationManager.SectionMapa));
        buttonBadatelna .onClick.AddListener(() => ShowPanel(NotificationManager.SectionBadatelna));
        buttonSbirka    .onClick.AddListener(() => ShowPanel(NotificationManager.SectionSbirka));
        buttonStatistiky.onClick.AddListener(() => ShowPanel(NotificationManager.SectionStatistiky));

        // Inicializuj vykřičníky podle aktuálního stavu NotificationManageru
        if (NotificationManager.Instance != null)
        {
            foreach (var pair in _exclamations)
                SetExclamation(pair.Key, NotificationManager.Instance.Has(pair.Key));
        }

        ShowNavigationPanel();
    }

    // -------------------------------------------------------------------------
    // Veřejné metody
    // -------------------------------------------------------------------------

    public void ShowPanel(string panelID)
    {
        HideAllContentPanels();

        if (_panels.TryGetValue(panelID, out GameObject panel))
            panel.SetActive(true);
        else
            Debug.LogWarning($"[MainGameUI] Neznámé panelID: {panelID}");

        navigationPanel.SetActive(false);
        _contentPanelOpen = true;
    }

    public void GoBack()
    {
        ShowNavigationPanel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_contentPanelOpen)
                GoBack();
            else
                GoToWorkroom();
        }
    }

    public void GoToWorkroom()
    {
        if (sceneController != null)
            sceneController.LoadWorkroom();
        else
            Debug.LogWarning("[MainGameUI] sceneController není přiřazen.");
    }

    // -------------------------------------------------------------------------

    private void ShowNavigationPanel()
    {
        HideAllContentPanels();
        navigationPanel.SetActive(true);
        _contentPanelOpen = false;
    }

    private void HideAllContentPanels()
    {
        foreach (var panel in _panels.Values)
            panel.SetActive(false);
    }

    private void OnNotificationChanged(string sectionID, bool active)
    {
        SetExclamation(sectionID, active);
    }

    private void SetExclamation(string sectionID, bool active)
    {
        if (_exclamations.TryGetValue(sectionID, out Image icon) && icon != null)
            icon.gameObject.SetActive(active);
    }
}
