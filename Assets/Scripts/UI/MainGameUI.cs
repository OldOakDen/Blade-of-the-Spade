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
    public Button buttonKomunikace;
    public Button buttonMapa;
    public Button buttonBadatelna;
    public Button buttonSbirka;
    public Button buttonStatistiky;

    [Header("Vykřičníky na navigačních tlačítkách")]
    public Image exclamationKomunikace;
    public Image exclamationMapa;
    public Image exclamationBadatelna;
    public Image exclamationSbirka;
    public Image exclamationStatistiky;

    // -------------------------------------------------------------------------

    private Dictionary<string, GameObject> _panels;
    private Dictionary<string, Image>      _exclamations;

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
    }

    public void GoBack()
    {
        ShowNavigationPanel();
    }

    // -------------------------------------------------------------------------

    private void ShowNavigationPanel()
    {
        HideAllContentPanels();
        navigationPanel.SetActive(true);
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
