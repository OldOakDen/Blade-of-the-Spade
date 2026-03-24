using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    // Vyvolá se při každé změně notifikace — UI vykřičníky se přihlásí k tomuto eventu
    public static event Action<string, bool> OnChanged;

    // Sekce workroomu
    public const string SectionKomunikace = "komunikace";
    public const string SectionMapa       = "mapa";
    public const string SectionBadatelna  = "badatelna";
    public const string SectionSbirka     = "sbirka";
    public const string SectionStatistiky = "statistiky";

    private readonly Dictionary<string, bool> _notifications = new()
    {
        { SectionKomunikace, false },
        { SectionMapa,       false },
        { SectionBadatelna,  false },
        { SectionSbirka,     false },
        { SectionStatistiky, false },
    };

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventBus.OnMessageRead      += HandleMessageRead;
        EventBus.OnArtifactReported += HandleArtifactReported;
        EventBus.OnHypothesisBuilt  += HandleHypothesisBuilt;
        EventBus.OnPolygonUnlocked  += HandlePolygonUnlocked;
        EventBus.OnPrestigeChanged  += HandlePrestigeChanged;
    }

    private void OnDisable()
    {
        EventBus.OnMessageRead      -= HandleMessageRead;
        EventBus.OnArtifactReported -= HandleArtifactReported;
        EventBus.OnHypothesisBuilt  -= HandleHypothesisBuilt;
        EventBus.OnPolygonUnlocked  -= HandlePolygonUnlocked;
        EventBus.OnPrestigeChanged  -= HandlePrestigeChanged;
    }

    private void Start()
    {
        // Kamarádův uvítací mail čeká na hráče při prvním spuštění
        Show(SectionKomunikace);
    }

    // -------------------------------------------------------------------------
    // Veřejné metody
    // -------------------------------------------------------------------------

    public void Show(string id)
    {
        if (!_notifications.ContainsKey(id))
        {
            Debug.LogWarning($"[NotificationManager] Neznámá sekce: {id}");
            return;
        }

        if (_notifications[id]) return; // už je zobrazena

        _notifications[id] = true;
        OnChanged?.Invoke(id, true);
    }

    public void Hide(string id)
    {
        if (!_notifications.ContainsKey(id)) return;
        if (!_notifications[id]) return; // už je skryta

        _notifications[id] = false;
        OnChanged?.Invoke(id, false);
    }

    public bool Has(string id) =>
        _notifications.TryGetValue(id, out bool active) && active;

    // -------------------------------------------------------------------------
    // EventBus handlery
    // -------------------------------------------------------------------------

    private void HandleMessageRead(string id)
    {
        // Hráč přečetl zprávu — schovat notifikaci komunikace
        // (UI komunikace samo rozhodne, zda jsou všechny zprávy přečteny)
        Hide(SectionKomunikace);
    }

    private void HandleArtifactReported(string id)
    {
        // Nový artefakt čeká na zaevidování ve sbírce
        Show(SectionSbirka);
    }

    private void HandleHypothesisBuilt(string id)
    {
        // Nová hypotéza k prozkoumání v badatelně
        Show(SectionBadatelna);
    }

    private void HandlePolygonUnlocked(string id)
    {
        // Nový polygon odemčen — upozorni na mapu
        Show(SectionMapa);
    }

    private void HandlePrestigeChanged(int newValue)
    {
        // Změna prestiže — zobraz ve statistikách
        Show(SectionStatistiky);
    }
}
